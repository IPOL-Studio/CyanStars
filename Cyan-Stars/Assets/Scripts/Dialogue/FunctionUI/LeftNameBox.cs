using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Dialogue
{
    public class LeftNameBox : Image
    {
        private TMP_Text text;

        protected override void Start()
        {
            text = GetComponentInChildren<TMP_Text>();
            DialogueManager.Instance.switchDialog += SetName;
        }

        public void SetName(int index)
        {
            if(DialogueManager.Instance.dialogueContentCells[index]
                .textContents.name == "") return;
            text.text = DialogueManager.Instance.dialogueContentCells[index].textContents.name;
        }
    }
}

