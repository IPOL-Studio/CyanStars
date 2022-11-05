using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using CyanStars.Framework.FSM;
using CyanStars.Framework.Timer;
using CyanStars.Framework.Utils;
using CyanStars.Gameplay.Base;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CyanStars.Gameplay.Dialogue
{
    /// <summary>
    /// Gal流程
    /// </summary>
    [ProcedureState]
    public class DialogueProcedure : BaseState
    {
        private const string ScenePath = "Assets/BundleRes/Scenes/GalDialogue.unity";

        private readonly HashSet<Type> TempActionUnitSet = new HashSet<Type>();

        private readonly DialogueMetadataModule MetadataModule = GameRoot.GetDataModule<DialogueMetadataModule>();
        private readonly DialogueModule DataModule = GameRoot.GetDataModule<DialogueModule>();

        private CircleContractionController circleContractionController;

        private Dictionary<int, BaseInitNode> dialogueInitNodeDict;
        private Dictionary<int, BaseFlowNode> dialogueFlowNodeDict;
        private BaseInitNode curInitNode;
        private BaseFlowNode curFlowNode;
        private bool executing;

        private Scene scene;

        public override async void OnEnter()
        {
            GameRoot.MainCamera.gameObject.SetActive(false);

            scene = await GameRoot.Asset.AwaitLoadScene(ScenePath);

            if (scene != default)
            {
                GetSceneObj();

                await LoadDialogueData(DataModule.StoryDataPath);

                while (curInitNode != null)
                {
                    await curInitNode.ExecuteAsync();
                    curInitNode = dialogueInitNodeDict.GetValueOrDefault(curInitNode.NextNodeID);
                    if (curInitNode is IEntryNode)
                    {
                        curInitNode = null;
                    }
                }

                circleContractionController.Exit(0.5f, ExecuteFlowNodes);
            }
        }

        private async void ExecuteFlowNodes()
        {
            executing = true;
            while (!(curFlowNode is null) && executing)
            {
                if (!CanExecuteNode(curFlowNode))
                {
                    curFlowNode = null;
                    break;
                }

                await curFlowNode.ExecuteAsync();
                if (!IsNodeAutoContinue(curFlowNode))
                {
                    await WaitCurNode();
                }
                NextFlowNode();
            }

            GameRoot.ChangeProcedure<MainHomeProcedure>();
            Debug.Log("End");
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsNodeAutoContinue(BaseNode node)
        {
            return !(node is IPauseableNode n) || n.IsAutoContinue;
        }

        private Task WaitCurNode()
        {
            TaskCompletionSource<object> cts = new TaskCompletionSource<object>();
            Debug.Log("node要求等待玩家操作");

            if (DataModule.IsAutoMode)
            {
                GameRoot.Timer.GetTimer<IntervalTimer>().Add(0.5f, _ =>
                {
                    cts.SetResult(null);
                });
            }

            return cts.Task;
        }

        public override void OnExit()
        {
            GameRoot.MainCamera.gameObject.SetActive(true);

            circleContractionController = null;

            dialogueInitNodeDict = null;
            dialogueFlowNodeDict = null;
            curInitNode = null;
            curFlowNode = null;

            executing = false;

            DataModule.Reset();

            GameRoot.Asset.UnloadScene(scene);
        }

        private void GetSceneObj()
        {
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

                if (node.ID < 0)
                {
                    throw new Exception("[Dialogue]找到 ID < 0 的节点，节点 ID 应大于等于0");
                }

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

        private bool CanExecuteNode(BaseFlowNode node)
        {
            if (!(node is ActionNode an))
            {
                return true;
            }

            foreach (var act in an.Actions)
            {
                Type type = act.GetType();
                var attr = MetadataModule.GetDialogueActionUnitAttribute(type);
                if (!attr.AllowMultiple && !TempActionUnitSet.Add(type))
                {
                    Debug.LogError($"在ID = {node.ID}的 ActionNode 中发现了超过 1 个的ActionUnit: ${type}");
                    TempActionUnitSet.Clear();
                    return false;
                }
            }

            TempActionUnitSet.Clear();
            return true;
        }

        private void NextFlowNode(bool skipEntry = true)
        {
            dialogueFlowNodeDict.TryGetValue(curFlowNode.NextNodeID, out var node);
            curFlowNode = node is IEntryNode && skipEntry ? null : node;
        }
    }
}
