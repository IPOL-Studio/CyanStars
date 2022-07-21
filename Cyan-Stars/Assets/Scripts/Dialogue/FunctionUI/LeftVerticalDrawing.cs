using System;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;

namespace CyanStars.Dialogue
{
    public class LeftVerticalDrawing : Image, ISetVerticalDrawing
    {
        public RectTransform width;
        protected override void Start()
        {
            width = DialogueManager.Instance.rectTransform;
            DialogueManager.Instance.switchDialog += SetImage;
            DialogueManager.Instance.switchDialog += SetAnimation;
            DialogueManager.Instance.switchDialog += SetXAxisMovement;
        }

        public void SetImage(int index)
        {
            if(DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.file == "") return;
            sprite = DialogueManager.Instance.spriteDictionary[DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.file];
        }

        public void SetAnimation(int index)
        {
            if (DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.effect == "") return;
            switch (DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.effect)
            {
                case "抖动":
                    rectTransform.DOShakePosition(1, new Vector3(5, 5, 5), 50, 180f);
                    break;
                case "旋转抖动":
                    rectTransform.DOShakeRotation(1, new Vector3(5, 5, 5), 50, 180f);
                    break;
                case "缩放":
                    rectTransform.DOShakeScale(1, 1);
                    break;
                default:
                    break;
            }
        }

        public void SetXAxisMovement(int index)
        {
            if (DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.xAxisMovement < 0) return;
            rectTransform.DOAnchorPosX(
                width.sizeDelta.x * DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.xAxisMovement, 1);
        }
    }
}

