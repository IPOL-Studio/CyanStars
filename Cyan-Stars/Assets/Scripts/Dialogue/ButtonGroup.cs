using System;
using System.Collections;
using System.Collections.Generic;
using CyanStars.Dialogue;
using CyanStars.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace CyanStars.Dialogue
{
    public class ButtonGroup : MonoBehaviour
    {
        private Button[] branchButton = new Button[5];

        private void Start()
        {
            for (int i = 0; i < 5; i++)
            {
                branchButton[i] = transform.GetChild(i).GetComponent<Button>();
                branchButton[i].gameObject.SetActive(false);
            }
            DialogueManager.Instance.OnCreateBranchUI += DisplayBranchUI;
        }

        /// <summary>
        /// 显示分支UI(最多显示5个)
        /// </summary>
        /// <param name="index"></param>
        public void DisplayBranchUI(int index)
        {
            for (int i = 0; i < 5; i++)
            {
                if (DialogueManager.Instance.dialogueContentCells[index + i].identifications.sign != "&") continue;

                branchButton[i].GetComponentInChildren<TMP_Text>().text =
                    DialogueManager.Instance.dialogueContentCells[index + i].textContents.content;

                branchButton[i].onClick.RemoveAllListeners();
                var i1 = i;
                branchButton[i].onClick.AddListener(() => OnOptionClick(index + i1));

                branchButton[i].gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 关闭分支UI
        /// </summary>
        public void DisableBranchUI()
        {
            for (int i = 0; i < 5; i++)
            {
                branchButton[i].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 分支按钮点击回调
        /// </summary>
        /// <param name="index"></param>
        private void OnOptionClick(int index)
        {
            DisableBranchUI();
            DialogueManager.Instance.dialogIndex = DialogueManager.Instance.dialogueContentCells[index].identifications.jump;
        }
    }
}


