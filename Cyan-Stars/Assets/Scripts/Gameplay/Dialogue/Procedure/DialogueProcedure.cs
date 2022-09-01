using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.Asset;
using CyanStars.Framework.Dialogue;
using CyanStars.Framework.Event;
using CyanStars.Framework.FSM;
using CyanStars.Framework.Utils;
using CyanStars.Gameplay.Base;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.Dialogue
{
    /// <summary>
    /// Gal流程
    /// </summary>
    [ProcedureState]
    public class DialogueProcedure : BaseState
    {
        private const string ScenePath = "Assets/BundleRes/Scenes/GalDialogue.unity";

        private static readonly HashSet<Type> TempActionUnitSet = new HashSet<Type>();

        private readonly DialogueMetadataModule MetadataModule = GameRoot.GetDataModule<DialogueMetadataModule>();
        private readonly DialogueModule DataModule = GameRoot.GetDataModule<DialogueModule>();

        private GameObject dialogueMainCanvas;
        private Image background;
        private CircleContractionController circleContractionController;

        private bool isInitNodesAllCompleted;
        private bool isInitCompleted;
        private Dictionary<int, BaseInitNode> dialogueInitNodeDict;
        private Dictionary<int, BaseFlowNode> dialogueFlowNodeDict;
        private BaseInitNode curInitNode;
        private BaseFlowNode curFlowNode;
        private bool isWaitingCurNode;
        private bool isWaitingFlowTimer;

        private bool loaded;

        public override async void OnEnter()
        {
            GameRoot.MainCamera.gameObject.SetActive(false);

            GameRoot.Event.AddListener(EventConst.SetBackgroundImageEvent, OnSetBackground);

            bool success = await GameRoot.Asset.AwaitLoadScene(ScenePath);

            if (success)
            {
                GetSceneObj();

                await LoadDialogueData(DataModule.StoryDataPath);

                if (curInitNode is null)
                {
                    CompleteInit();
                }
                else
                {
                    curInitNode.OnInit();
                }

                loaded = true;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            if (!loaded)
            {
                return;
            }

            if (!isInitCompleted)
            {
                if (!isInitNodesAllCompleted)
                {
                    CheckInitNodes();
                }
                return;
            }

            UpdateFlowNode(deltaTime);

            if (curFlowNode is null)
            {
                GameRoot.ChangeProcedure<MainHomeProcedure>();
                Debug.Log("End");
            }
        }

        private void UpdateFlowNode(float deltaTime)
        {
            if (curFlowNode is null) return;

            // 如果当前 node 还没有执行完毕
            // 就更新当前 node，并重复检查是否执行完毕
            if (!curFlowNode.IsCompleted)
            {
                curFlowNode.OnUpdate(deltaTime);

                if (!curFlowNode.IsCompleted)
                {
                    return;
                }

                // 如果当前 node 需要等待 自动模式计时器 或 玩家交互
                // 就进入等待交互状态
                if (curFlowNode is IPauseableNode { IsPause: true })
                {
                    WaitCurNode();
                }
            }

            // 还在等待 计时器 或 玩家交互
            if (curFlowNode is IPauseableNode { IsPause: true } && isWaitingCurNode)
            {
                return;
            }

            // 到这里时，已经执行完毕的node，要么是不需要等待交互的，要么已经完成了交互行为
            // 所以直接设置 true 跑后续 node 的更新循环
            bool canToNext = true;

            while (canToNext)
            {
                if (!NextNode())
                {
                    return;
                }
                CheckCurNode();
                curFlowNode.OnInit();

                // 当前 node 不需要等待玩家交互，并且已经执行完毕
                // 前进到下一个 node 并重复流程
                canToNext = !(curFlowNode is IPauseableNode { IsPause: true }) && curFlowNode.IsCompleted;
            }

            // 经过上面的 node 更新循环后
            // 当前的 node 已经执行完毕，但是需要等待交互
            if (curFlowNode.IsCompleted)
            {
                WaitCurNode();
            }
        }

        private void WaitCurNode()
        {
            isWaitingCurNode = true;
            Debug.Log("node要求等待玩家操作");

            if (DataModule.IsAutoMode && !isWaitingFlowTimer)
            {
                isWaitingFlowTimer = true;
                GameRoot.Timer.AddTimer(0.5f, () =>
                {
                    if (DataModule.IsAutoMode)
                    {
                        isWaitingFlowTimer = false;
                        isWaitingCurNode = false;
                    }
                });
            }
        }

        private void CheckInitNodes()
        {
            if (curInitNode is null)
            {
                CompleteInit();
                return;
            }

            if (curInitNode.IsCompleted)
            {
                var node = dialogueInitNodeDict.TryGetValue(curInitNode.NextNodeID, out var r) ? r : null;
                if (node is null || node is InitEntryNode)
                {
                    CompleteInit();
                }
                else
                {
                    curInitNode = node;
                    curInitNode.OnInit();
                }
            }
        }

        public override void OnExit()
        {
            GameRoot.MainCamera.gameObject.SetActive(true);

            dialogueMainCanvas = null;
            background = null;
            circleContractionController = null;

            isInitNodesAllCompleted = false;
            isInitCompleted = false;
            dialogueInitNodeDict = null;
            dialogueFlowNodeDict = null;
            curInitNode = null;
            curFlowNode = null;
            isWaitingCurNode = false;
            isWaitingFlowTimer = false;

            loaded = false;

            GameRoot.Event.RemoveListener(EventConst.SetBackgroundImageEvent, OnSetBackground);

            DataModule.Reset();

            GameRoot.Asset.UnloadScene(ScenePath);
        }

        private async void OnSetBackground(object sender, EventArgs e)
        {
            var filePath = (e as SingleEventArgs<string>)?.Value;
            background.sprite = (await GameRoot.Asset.AwaitLoadAsset<Texture2D>(filePath, dialogueMainCanvas)).ConvertToSprite();
        }

        private void GetSceneObj()
        {
            dialogueMainCanvas = GameObject.Find("DialogueMainCanvas");
            var trans = dialogueMainCanvas.transform;
            background = trans.Find("Background").GetComponent<Image>();

            circleContractionController =
                GameObject.Find("TransitionsCanvas").GetComponent<CircleContractionController>();
        }

        private async Task LoadDialogueData(string jsonFilePath)
        {
            // 目前还没有MOD实现，所以只考虑内置情况
            var jsonAsset = await GameRoot.Asset.AwaitLoadAsset<TextAsset>(jsonFilePath);
            DialogueData blackboard = await DialogueDataHelper.Deserialize(jsonAsset.text);
            LoadInitNodes(blackboard);
            LoadFlowNodes(blackboard);
        }

        private void LoadFlowNodes(DialogueData blackboard)
        {
            if ((blackboard.FlowNodes?.Count ?? 0) == 0)
            {
                throw new Exception("没有找到任何Flow Node");
            }

            dialogueFlowNodeDict = new Dictionary<int, BaseFlowNode>(blackboard.FlowNodes.Count);
            LoadNodes<BaseFlowNode, FlowEntryNode>(blackboard.FlowNodes, dialogueFlowNodeDict, ref curFlowNode);
        }

        private void LoadInitNodes(DialogueData blackboard)
        {
            if ((blackboard.InitNodes?.Count ?? 0) == 0)
            {
                return;
            }

            dialogueInitNodeDict = new Dictionary<int, BaseInitNode>(blackboard.InitNodes.Count);
            LoadNodes<BaseInitNode, InitEntryNode>(blackboard.InitNodes, dialogueInitNodeDict, ref curInitNode);
        }

        private void LoadNodes<T, TE>(IList<NodeData<T>> nodeDatas, IDictionary<int, T> nodeDict, ref T curNode)
            where T : BaseNode
            where TE : BaseNode, IEntryNode
        {
            for (int i = 0; i < nodeDatas.Count; i++)
            {
                T node = nodeDatas[i].Node;
                nodeDict.Add(node.ID, node);

                if (node is TE)
                {
                    if (curNode != null)
                    {
                        throw new Exception($"[Dialogue]找到多个{typeof(TE)}节点");
                    }
                    curNode = node;
                }
            }

            _ = curNode ?? throw new Exception($"[Dialogue]没有找到{typeof(TE)}节点");

            if (curNode.ID != 0)
            {
                throw new Exception($"[Dialogue]找到了{typeof(TE)}节点，入口节点的ID只能为0，当前入口节点的ID为{curNode.ID}");
            }
        }

        private void CheckCurNode()
        {
            if (curFlowNode is null)
                return;

            TempActionUnitSet.Clear();

            if (curFlowNode is ActionNode node)
            {
                foreach (var act in node.Actions)
                {
                    Type type = act.GetType();
                    DialogueActionUnitAttribute attr = MetadataModule.GetDialogueActionUnitAttribute(type);
                    if (!attr.AllowMultiple && !TempActionUnitSet.Add(type))
                    {
                        throw new Exception($"在ID = {node.ID}的 ActionNode 中发现了超过 1 个的ActionUnit: ${type}");
                    }
                }
            }
        }

        /// <returns>是否存在下一个可用的node，若存在返回true，否则返回false</returns>
        private bool NextNode(bool skipEntry = true)
        {
            dialogueFlowNodeDict.TryGetValue(curFlowNode.NextNodeID, out var node);
            curFlowNode = node is FlowEntryNode && skipEntry ? null : node;
            return !(curFlowNode is null);
        }

        private void CompleteInit()
        {
            isInitNodesAllCompleted = true;
            circleContractionController.Exit(0.5f, () =>
            {
                isInitCompleted = true;
                curFlowNode?.OnInit();
            });
        }
    }
}
