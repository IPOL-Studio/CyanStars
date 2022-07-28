using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Dialogue
{

    public class DialogBox : Image, IPointerClickHandler
    {
        private TMP_Text text;
        private Cell cell = new Cell();
        private int index;
        private bool inDialogue;

        protected override void Start()
        {
            text = GetComponentInChildren<TMP_Text>();
            inDialogue = false;
            index = 0;
            DialogueManager.Instance.OnSwitchDialog += SwitchDialog;

            DialogueManager.Instance.InvokeOnSwitchDialog(DialogueManager.Instance.dialogIndex);
            if (inDialogue == false)
            {
                StartCoroutine(DisplayDialogue());
            }
            else
            {
                StopAllCoroutines();
                DirectDisplayDialogue();
            }
        }

        private void SwitchDialog(int startIndex)
        {
            text.text = string.Empty;
            index = startIndex;
        }

        /// <summary>
        /// 设置富文本颜色前缀
        /// </summary>
        private void SetColor()
        {
            switch (cell.textContents.color)
            {
                case "White":
                    text.text += DialogueManager.Colors.White;
                    break;
                case "Red":
                    text.text += DialogueManager.Colors.Red;
                    break;
                case "Yellow":
                    text.text += DialogueManager.Colors.Yellow;
                    break;
                case "Blue":
                    text.text += DialogueManager.Colors.Blue;
                    break;
                case "Green":
                    text.text += DialogueManager.Colors.Green;
                    break;
                case "Purple":
                    text.text += DialogueManager.Colors.Purple;
                    break;
                case "Gray":
                    text.text += DialogueManager.Colors.Gray;
                    break;
                case "Black":
                    text.text += DialogueManager.Colors.Black;
                    break;
                default:
                    text.text += DialogueManager.Colors.White;
                    break;
            }
        }

        /// <summary>
        /// 设置对话(有顿字)
        /// </summary>
        /// <returns></returns>
        private IEnumerator DisplayDialogue()
        {
            inDialogue = true;

            cell = DialogueManager.Instance.dialogueContentCells[index];

            DialogueManager.Instance.InvokeOnAnimation(cell.identifications.id);

            if (cell.textContents.stop != 0)
            {
                foreach (char c in cell.textContents.content)
                {
                    SetColor();
                    text.text += c + "</color>";
                    yield return new WaitForSecondsRealtime(cell.textContents.stop * 0.001f);
                }
            }
            else
            {
                SetColor();
                text.text += cell.textContents.content + "</color>";
            }

            index = cell.identifications.jump;

            if ("是".Equals(cell.textContents.link) && "是".Equals(DialogueManager.Instance.dialogueContentCells[index].textContents.link))
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

            SetColor();
            text.text += cell.textContents.content + "</color>";

            index = cell.identifications.jump;

            // if ("是".Equals(cell.textContents.link))
            // {
            //     cell = DialogueManager.Instance.dialogueContentCells[index];
            //     yield return StartCoroutine(DirectDisplayDialogue());
            // }

            // DialogueManager.Instance.InvokeOnSkipAnimation(cell.identifications.id);

            while ("是".Equals(cell.textContents.link) && "是".Equals(DialogueManager.Instance.dialogueContentCells[index].textContents.link))
            {
                cell = DialogueManager.Instance.dialogueContentCells[index];
                SetColor();
                text.text += cell.textContents.content + "</color>";
                index = cell.identifications.jump;
            }

            DialogueManager.Instance.dialogIndex = index;
            inDialogue = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (DialogueManager.Instance.stateCount != 0) return;
            if (DialogueManager.Instance.dialogueContentCells[DialogueManager.Instance.dialogIndex].identifications
                .sign == "&")
            {
                DialogueManager.Instance.InvokeOnCreateBranchUI(DialogueManager.Instance.dialogIndex);
                return;
            }
            DialogueManager.Instance.InvokeOnSwitchDialog(DialogueManager.Instance.dialogIndex);
            if (inDialogue == false)
            {
                StartCoroutine(DisplayDialogue());
            }
            else
            {
                StopAllCoroutines();
                DirectDisplayDialogue();
            }
        }

    }
}
