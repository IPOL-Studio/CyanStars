using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Dialogue
{
    public class Bg : Image, ISetVerticalDrawing
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

        public void SetAnimation(int index)
        {

        }

        public void SkipAnimation(int index)
        {

        }

        public void DOAnchorPosXMove(float xAxisMovement, float duration)
        {

        }
    }
}

