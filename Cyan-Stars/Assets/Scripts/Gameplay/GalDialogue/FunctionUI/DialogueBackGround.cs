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
            // DialogueManager.Instance.OnSwitchDialog += SetImage;
            GameRoot.Event.AddListener(DialogueEventConst.GalSwitchDialog, SetImage);
        }

        public void SetImage(object sender, EventArgs e)
        {
            DialogueEventArgs args = (DialogueEventArgs)e;
            if (DialogueManager.Instance.dialogueContentCells[args.index].backgrounds.file == "") return;
            sprite = DialogueManager.Instance.spriteDictionary[DialogueManager.Instance.dialogueContentCells[args.index].backgrounds.file];
        }
    }
}

