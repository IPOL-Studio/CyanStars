using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Dialogue
{
    public static class Colors
    {
        public const string White = "<color=white>";
        public const string Red = "<color=red>";
        public const string Yellow = "<color=yellow>";
        public const string Blue = "<color=blue>";
        public const string Green = "<color=green>";
        public const string Purple = "<color=purple>";
        public const string Gray = "<color=gray>";
        public const string Black = "<color=black>";
    }
    public class DialogBox : Image
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
            DialogueManager.Instance.switchDialog += SwitchDialog;
        }

        private void SwitchDialog(int startIndex)
        {
            text.text = string.Empty;
            if (inDialogue)
            {
                StopAllCoroutines();
                text.text = string.Empty;
                inDialogue = true;
                index = startIndex;
                StartCoroutine(DirectDisplayDialogue());
            }
            else
            {
                index = startIndex;
                StartCoroutine(DisplayDialogue());
            }
        }

        /// <summary>
        /// 设置富文本颜色前缀
        /// </summary>
        private void SetColor()
        {
            switch (cell.textContents.color)
            {
                case "White":
                    text.text += Colors.White;
                    break;
                case "Red":
                    text.text += Colors.Red;
                    break;
                case "Yellow":
                    text.text += Colors.Yellow;
                    break;
                case "Blue":
                    text.text += Colors.Blue;
                    break;
                case "Green":
                    text.text += Colors.Green;
                    break;
                case "Purple":
                    text.text += Colors.Purple;
                    break;
                case "Gray":
                    text.text += Colors.Gray;
                    break;
                case "Black":
                    text.text += Colors.Black;
                    break;
                default:
                    text.text += Colors.White;
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

            if (cell.textContents.stop != 0)
            {
                foreach (char c in cell.textContents.content)
                {
                    SetColor();
                    text.text += c + "</color>";
                    yield return new WaitForSecondsRealtime(cell.textContents.stop / 1000f);
                }
            }
            else
            {
                SetColor();
                text.text += cell.textContents.content + "</color>";
            }

            index = cell.identifications.jump;

            if ("是".Equals(cell.textContents.link))
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
        private IEnumerator DirectDisplayDialogue()
        {
            inDialogue = true;

            cell = DialogueManager.Instance.dialogueContentCells[index];

            SetColor();
            text.text += cell.textContents.content + "</color>";

            index = cell.identifications.jump;

            if ("是".Equals(cell.textContents.link))
            {
                cell = DialogueManager.Instance.dialogueContentCells[index];
                yield return StartCoroutine(DirectDisplayDialogue());
            }

            DialogueManager.Instance.dialogIndex = index;
            inDialogue = false;
        }
    }
}
