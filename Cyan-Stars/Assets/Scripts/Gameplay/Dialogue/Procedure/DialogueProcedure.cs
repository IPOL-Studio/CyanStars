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
        private bool canToNextFlowNode;
        private bool waitFlowTimer;

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

                curFlowNode?.OnInit();
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
        }

        private void UpdateFlowNode(float deltaTime)
        {
            if (curFlowNode is null) return;

            if (curFlowNode.IsCompleted)
            {
                if (canToNextFlowNode || curFlowNode.GotoNextType == GotoNextNodeActionType.Direct)
                {
                    canToNextFlowNode = false;
                    NextNode();
                    CheckCurNode();
                    curFlowNode?.OnInit();
                }
                else if (DataModule.IsAutoMode && !waitFlowTimer)
                {
                    waitFlowTimer = true;
                    GameRoot.Timer.AddTimer(0.5f, () =>
                    {
                        waitFlowTimer = false;
                        canToNextFlowNode = true;
                    });
                }
            }

            if (curFlowNode is null)
            {
                GameRoot.ChangeProcedure<MainHomeProcedure>();
                Debug.Log("End");
                return;
            }

            if (!curFlowNode.IsCompleted)
            {
                curFlowNode.OnUpdate(deltaTime);
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
            canToNextFlowNode = false;
            waitFlowTimer = false;

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

                if (curNode is null)
                {
                    throw new Exception($"[Dialogue]没有找到{typeof(TE)}节点");
                }

                if (curNode.ID != 0)
                {
                    throw new Exception($"[Dialogue]找到了{typeof(TE)}节点，入口节点的ID只能为0，当前入口节点的ID为{curNode.ID}");
                }
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
                        throw new Exception($"在ID = {node.ID}的 ActionNode 中发现了超过 1 个的ActionUnit: ${act.GetType()}");
                    }
                }
            }
        }

        private void NextNode(bool skipEntry = true)
        {
            dialogueFlowNodeDict.TryGetValue(curFlowNode.NextNodeID, out var node);
            curFlowNode = node is FlowEntryNode && skipEntry ? null : node;
        }

        private void CompleteInit()
        {
            isInitNodesAllCompleted = true;
            circleContractionController.Exit(0.5f, () => isInitCompleted = true);
        }
    }
}
