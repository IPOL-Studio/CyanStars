using System;
using System.Collections;
using System.Linq;
using System.Threading;
using CyanStars.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Gameplay.Dialogue
{

    public class DialogBox : Image, IPointerClickHandler
    {
        private TMP_Text text;
        private Cell cell = new Cell();
        private int index;
        private bool inDialogue;

        /// <summary>
        /// 初始化完成前禁止对话框对点击的响应
        /// </summary>
        protected override void Awake()
        {
            raycastTarget = false;
        }

        protected override void Start()
        {
            text = GetComponentInChildren<TMP_Text>();
            inDialogue = false;
            index = 0;
            // DialogueManager.Instance.OnSwitchDialog += SwitchDialog;
            GameRoot.Event.AddListener(DialogueEventConst.GalSwitchDialog, SwitchDialog);
            StartCoroutine(AutoShowFirstDialogue());
        }

        /// <summary>
        /// 初始化后自动播放第一句
        /// </summary>
        /// <returns></returns>
        private IEnumerator AutoShowFirstDialogue()
        {
            while (!DialogueManager.Instance.initializationComplete)
            {
                yield return null;
            }

            DialogueManager.Instance.InvokeOnSwitchDialog(DialogueManager.Instance.dialogIndex);
            raycastTarget = true;
        }

        /// <summary>
        /// 切换下一对话清空当前对话并设置开始id
        /// </summary>
        /// <param name="startIndex"></param>
        private void SwitchDialog(object sender, EventArgs e)
        {
            DialogueEventArgs args = (DialogueEventArgs)e;
            text.text = string.Empty;
            index = args.index;
            if (inDialogue == false)
            {
                StartCoroutine(DisplayDialogue());
            }
            else
            {
                //TODO:暂时使用停止全部协程，之后看用了哪个停哪个
                StopAllCoroutines();
                DirectDisplayDialogue();
            }
        }

        /// <summary>
        /// 设置对话(有顿字)
        /// </summary>
        /// <returns></returns>
        /// TODO:EXCEL顿字为-1为默认速度（通过设置更改默认速度）为0为无顿字
        private IEnumerator DisplayDialogue()
        {
            inDialogue = true;

            cell = DialogueManager.Instance.dialogueContentCells[index];

            DialogueManager.Instance.InvokeOnAnimation(cell.identifications.id);

            if (cell.textContents.stop != 0)
            {
                foreach (char c in cell.textContents.content)
                {
                    text.text += $"<color={cell.textContents.color}>" + c + "</color>";
                    yield return new WaitForSecondsRealtime(cell.textContents.stop * 0.001f);
                }
            }
            else
            {
                text.text += $"<color={cell.textContents.color}>" + cell.textContents.content + "</color>";
            }

            //等待动画播放完毕
            while (DialogueManager.Instance.stateCount != 0)
            {
                yield return null;
            }

            index = cell.identifications.jump;

            //需要连接句子的情况
            if (cell.textContents.link == 1 && DialogueManager.Instance.dialogueContentCells[index].textContents.link == 1)
            {
                cell = DialogueManager.Instance.dialogueContentCells[index];
                yield return StartCoroutine(DisplayDialogue());
            }

            DialogueManager.Instance.dialogIndex = index;
            inDialogue = false;
        }

        /// <summary>
        /// 设置对话(无顿字)
        /// </summary>
        /// <returns></returns>
        private void DirectDisplayDialogue()
        {
            inDialogue = true;

            cell = DialogueManager.Instance.dialogueContentCells[index];

            text.text += $"<color={cell.textContents.color}>" + cell.textContents.content + "</color>";

            index = cell.identifications.jump;

            //需要连接句子的情况
            while (cell.textContents.link == 1 && DialogueManager.Instance.dialogueContentCells[index].textContents.link == 1)
            {
                cell = DialogueManager.Instance.dialogueContentCells[index];
                text.text += $"<color={cell.textContents.color}>" + cell.textContents.content + "</color>";
                index = cell.identifications.jump;
            }

            DialogueManager.Instance.InvokeOnSkipAnimation(cell.identifications.id);
            DialogueManager.Instance.dialogIndex = index;
            inDialogue = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {

            if (eventData.button != PointerEventData.InputButton.Left) return;
            if ("END".Equals(DialogueManager.Instance.dialogueContentCells[DialogueManager.Instance.dialogIndex].identifications.sign))
            {
#if UNITY_EDITOR
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = false;
                }
#endif
                //TODO:剧情播放完后的操作
                return;
            }
            if ("&".Equals(DialogueManager.Instance.dialogueContentCells[DialogueManager.Instance.dialogIndex].identifications.sign))
            {
                DialogueManager.Instance.InvokeOnCreateBranchUI(DialogueManager.Instance.dialogIndex);
                return;
            }
            DialogueManager.Instance.InvokeOnSwitchDialog(DialogueManager.Instance.dialogIndex);
        }

    }
}
