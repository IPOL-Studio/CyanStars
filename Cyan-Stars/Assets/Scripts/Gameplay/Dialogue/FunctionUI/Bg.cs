using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Dialogue
{
    public class Bg : Image
    {
        protected override void Start()
        {
            DialogueManager.Instance.OnSwitchDialog += SetImage;
        }

        public void SetImage(int index)
        {
            if (DialogueManager.Instance.dialogueContentCells[index].backgrounds.file == "") return;
            sprite = DialogueManager.Instance.spriteDictionary[DialogueManager.Instance.dialogueContentCells[index].backgrounds.file];
        }
    }
}

