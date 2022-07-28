using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;

namespace CyanStars.Dialogue
{
    public class RightVerticalDrawing : Image, ISetVerticalDrawing
    {
        public RectTransform width;
        private Tween tween;
        protected override void Start()
        {
            width = DialogueManager.Instance.rectTransform;
            DialogueManager.Instance.OnAnimation += SetImage;
            DialogueManager.Instance.OnAnimation += SetAnimation;
            DialogueManager.Instance.OnAnimation += SetXAxisMovement;
            // DialogueManager.Instance.OnSkipAnimation += SkipAnimation;
        }

        public void SetImage(int index)
        {
            if(DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.file == "") return;
            sprite = DialogueManager.Instance.spriteDictionary[DialogueManager.Instance.dialogueContentCells[index]
                .rightVerticalDrawings.file];
        }

        public void SetAnimation(int index)
        {
            if (DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.effect == "") return;
            switch (DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.effect)
            {
                case "抖动":
                    rectTransform.DOShakeAnchorPos(0.3f, new Vector3(5, 5, 5), 50, 180f).OnPlay(() => DialogueManager.Instance.stateCount++).OnComplete(() => DialogueManager.Instance.stateCount--);
                    break;
                case "旋转抖动":
                    rectTransform.DOShakeRotation(0.3f, new Vector3(5, 5, 5), 50, 180f).OnPlay(() => DialogueManager.Instance.stateCount++).OnComplete(() => DialogueManager.Instance.stateCount--);
                    break;
                case "缩放":
                    rectTransform.DOShakeScale(0.3f).OnPlay(() => DialogueManager.Instance.stateCount++).OnComplete(() => DialogueManager.Instance.stateCount--);
                    break;
                default:
                    break;
            }
        }

        public void DOAnchorPosXMove(float xAxisMovement, float duration)
        {
            tween = rectTransform.DOAnchorPosX(-width.sizeDelta.x * xAxisMovement, duration).OnPlay(() => DialogueManager.Instance.stateCount++).OnComplete(() => DialogueManager.Instance.stateCount--);
        }

        public void SkipAnimation(int index)
        {
            if (DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.xAxisMovement < 0) return;
            DOAnchorPosXMove(DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.xAxisMovement, 0);
        }

        public void SetXAxisMovement(int index)
        {
            if (DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.xAxisMovement < 0) return;
            DOAnchorPosXMove(DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.xAxisMovement, 1);
        }
    }
}

