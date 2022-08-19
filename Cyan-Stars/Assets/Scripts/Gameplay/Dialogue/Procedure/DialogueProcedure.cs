using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.Asset;
using CyanStars.Framework.Dialogue;
using CyanStars.Framework.Event;
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
        private const string ScenePath = ""; //TODO: Gal Scene

        private static readonly HashSet<Type> TempActionUnitSet = new HashSet<Type>();

        private readonly DialogueMetadataModule MetadataModule = GameRoot.GetDataModule<DialogueMetadataModule>();
        private readonly DialogueDataModule DataModule = GameRoot.GetDataModule<DialogueDataModule>();

        private TMP_Text nameText;
        private TMP_Text contentText;

        private Image avatarImage;

        private Dictionary<int, BaseNode> dialogueNodeDict;

        private BaseNode curNode;

        public override async void OnEnter()
        {
            GameRoot.MainCamera.gameObject.SetActive(false);

            GameRoot.Event.AddListener(EventConst.SetNameTextEvent, OnSetNameText);
            GameRoot.Event.AddListener(EventConst.SetAvatarEvent, OnSetAvatar);
            GameRoot.Event.AddListener(EventConst.SetBackgroundImageEvent, OnSetBackground);
            GameRoot.Event.AddListener(EventConst.PlaySoundEvent, OnPlaySound);

            bool success = await GameRoot.Asset.AwaitLoadScene(ScenePath);
        }

        public override void OnUpdate(float deltaTime)
        {
            if (curNode is null) return;

            if (curNode.IsCompleted)
            {
                NextNode();
                CheckCurNode();
                curNode?.OnInit();
            }

            if (curNode is null)
            {
                OnExit();
                return;
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

            GameRoot.Event.RemoveListener(EventConst.SetNameTextEvent, OnSetNameText);
            GameRoot.Event.RemoveListener(EventConst.SetAvatarEvent, OnSetAvatar);
            GameRoot.Event.RemoveListener(EventConst.SetBackgroundImageEvent, OnSetBackground);
            GameRoot.Event.RemoveListener(EventConst.PlaySoundEvent, OnPlaySound);

            GameRoot.Asset.UnloadScene(ScenePath);
        }

        private void UpdateData()
        {
            if (DataModule.IsContentDirty)
            {
                contentText.text = DataModule.Content.ToString();
                DataModule.IsContentDirty = false;
            }
        }

        private void OnSetNameText(object sender, EventArgs e)
        {
            nameText.text = (e as SingleEventArgs<string>)?.Value;
        }

        private void OnSetAvatar(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnSetBackground(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnPlaySound(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private async Task LoadDialogueData(string jsonFilePath)
        {
            // 目前还没有MOD实现，所以只考虑内置情况
            var jsonAsset = await GameRoot.Asset.AwaitLoadAsset<TextAsset>(jsonFilePath);
            DialogueData blackboard = await Task.Run(() =>
            {
                return JsonConvert.DeserializeObject<DialogueData>(jsonAsset.text,
                    NodeDataConverter.Converter,
                    ActionUnitConverter.Converter);
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

            if (curNode.ID != 0)
            {
                throw new Exception("[Dialogue]入口节点的ID不为0");
            }
        }

        private void CheckCurNode()
        {
            if (curNode is null)
                return;

            TempActionUnitSet.Clear();

            if (curNode is ActionNode node)
            {
                foreach (var act in node.Actions)
                {
                    Type type = act.GetType();
                    DialogueActionUnitAttribute attr = MetadataModule.GetDialogueActionUnitAttribute(type);
                    if (!attr.AllowMultiple && !TempActionUnitSet.Add(type))
                    {
                        throw new Exception($"在ID = {node.ID}的 Node 中发现了超过 1 个的ActionUnit: ${act.GetType()}");
                    }
                }
            }
        }

        private void NextNode(bool skipEntry = true)
        {
            dialogueNodeDict.TryGetValue(curNode.NextNodeID, out var node);
            curNode = node is EntryNode && skipEntry ? null : node;
        }
    }
}
