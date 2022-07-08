using System.Collections;
using System.Linq;
using CyanStars.Framework.Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Gameplay.Dialogue
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
        protected override void Start()
        {
            text = GetComponentInChildren<TMP_Text>();
            DialogueManager.Instance.switchDialog += SwitchDialog;
        }

        public void SwitchDialog()
        {
            text.text = string.Empty;
            StartCoroutine(Dialogue());
        }

        public void SetColor()
        {
            switch (cell.color)
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
        /// 切换对话的功能协程
        /// </summary>
        /// <returns></returns>
        /// TODO:处理连续点击鼠标的问题
        IEnumerator Dialogue()
        {
            cell = DialogueManager.Instance.dialogueContentCells[DialogueManager.Instance.dialogIndex];

            if (cell.stop != 0)
            {
                foreach (char c in cell.text)
                {
                    SetColor();
                    text.text += c + "</color>";
                    yield return new WaitForSecondsRealtime(cell.stop / 1000f);
                }
            }
            else
            {
                SetColor();
                text.text += cell.text;
            }

            DialogueManager.Instance.dialogIndex = cell.jump;

            if ("是".Equals(cell.link))
            {
                cell = DialogueManager.Instance.dialogueContentCells[DialogueManager.Instance.dialogIndex];
                StartCoroutine(Dialogue());
            }
        }
    }
}
