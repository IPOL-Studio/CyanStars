using System;
using CyanStars.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Gameplay.Dialogue
{
    public class DialogueBackGround : Image
    {
        protected override void Start()
        {
            DialogueManager.Instance.OnSwitchDialog += SetImage;
            // GameRoot.Event.AddListener(DialogueEventConst.GalSwitchDialog, SetImage);
        }

        public void SetImage(int index)
        {
            // DialogueEventArgs args = (DialogueEventArgs)e;
            if (DialogueManager.Instance.dialogueContentCells[index].backgrounds.file == "") return;
            sprite = DialogueManager.Instance.spriteDictionary[DialogueManager.Instance.dialogueContentCells[index].backgrounds.file];
        }
    }
}

