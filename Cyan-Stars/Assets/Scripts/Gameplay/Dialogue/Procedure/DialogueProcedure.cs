using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.Asset;
using CyanStars.Framework.Dialogue;
using CyanStars.Framework.FSM;
using Newtonsoft.Json;
using TMPro;
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
        private static readonly HashSet<Type> TempStepSet = new HashSet<Type>();

        private readonly DialogueMetadataModule MetadataModule = GameRoot.GetDataModule<DialogueMetadataModule>();
        private readonly DialogueDataModule DataModule = GameRoot.GetDataModule<DialogueDataModule>();

        private TMP_Text nameText;
        private TMP_Text contentText;

        private Image avatarImage;

        public StringBuilder ContentBuilder { get; set; } = new StringBuilder();

        private Dictionary<int, BaseNode> dialogueNodeDict;

        private BaseNode curNode;

        public override async void OnEnter()
        {
            GameRoot.MainCamera.gameObject.SetActive(false);

            bool success = await GameRoot.Asset.AwaitLoadScene(""); //TODO: Gal Scene
        }

        public override void OnUpdate(float deltaTime)
        {
            if (curNode is null) return;

            if (curNode.IsCompleted)
            {
                curNode = dialogueNodeDict[curNode.NextNodeID];
                CheckCurNode();
                curNode.OnInit();
            }

            curNode.OnUpdate(deltaTime);

            UpdateData();
        }

        public override void OnExit()
        {
            GameRoot.MainCamera.gameObject.SetActive(true);

            nameText = null;
            contentText = null;
            avatarImage = null;
            dialogueNodeDict = null;
            curNode = null;
        }

        private void UpdateData()
        {
            if (DataModule.IsContentDirty)
            {
                contentText.text = DataModule.Content.ToString();
                DataModule.IsContentDirty = false;
            }
        }

        public void UpdateContent()
        {
            contentText.text = ContentBuilder.ToString();
        }

        private async Task LoadDialogueData(string jsonFilePath)
        {
            // 目前还没有MOD实现，所以只考虑内置情况
            var jsonAsset = await GameRoot.Asset.AwaitLoadAsset<TextAsset>(jsonFilePath);
            DialogueData blackboard = await Task.Run(() =>
            {
                return JsonConvert.DeserializeObject<DialogueData>(jsonAsset.text,
                    NodeDataConverter.Converter,
                    StepConverter.Converter);
            });

            dialogueNodeDict = new Dictionary<int, BaseNode>(blackboard.Nodes.Count);
            for (int i = 0; i < blackboard.Nodes.Count; i++)
            {
                BaseNode node = blackboard.Nodes[i].Node;
                dialogueNodeDict.Add(node.ID, node);

                if (node is EntryNode)
                {
                    if (curNode != null)
                    {
                        throw new Exception("[Dialogue]找到多个入口节点");
                    }
                    curNode = node;
                }
            }

            if (curNode is null)
            {
                throw new Exception("[Dialogue]没有找到入口节点");
            }
        }

        private void CheckCurNode()
        {
            if (curNode is null)
                return;

            TempStepSet.Clear();

            if (curNode is ActionNode node)
            {
                foreach (var step in node.Steps)
                {
                    Type type = step.GetType();
                    DialogueStepAttribute attr = MetadataModule.GetDialogueStepAttribute(type);
                    if (!attr.AllowMultiple && !TempStepSet.Add(type))
                    {
                        throw new Exception($"在ID = {node.ID}的 Node 中发现了超过 1 个的Step: ${step.GetType()}");
                    }
                }
            }
        }
    }
}
