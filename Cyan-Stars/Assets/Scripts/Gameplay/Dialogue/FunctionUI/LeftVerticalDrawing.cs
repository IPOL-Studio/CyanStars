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
            DialogueManager.Instance.OnAnimation += SetImage;
            DialogueManager.Instance.OnAnimation += SetAnimation;
            DialogueManager.Instance.OnAnimation += SetXAxisMovement;
            // DialogueManager.Instance.OnSkipAnimation += SkipAnimation;
        }

        private void AddStateCount()
        {
            DialogueManager.Instance.stateCount++;
        }

        private void SubStateCount()
        {
            DialogueManager.Instance.stateCount--;
        }

        public void SetImage(int index)
        {
            if(DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.file == "") return;
            sprite = DialogueManager.Instance.spriteDictionary[DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.file];
        }

        public void SetAnimation(int index)
        {
            float duration = DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.time;
            if (DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.effect == "") return;
            switch (DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.effect)
            {
                case "抖动":
                    switch (DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.curve)
                    {
                        case "线性":
                            rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.Linear);
                            break;
                        case "三次方加速":
                            rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InCubic);
                            break;
                        case "三次方减速":
                            rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.OutCubic);
                            break;
                        case "三次方加速减速":
                            rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InOutCubic);
                            break;
                        case "指数加速":
                            rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InExpo);
                            break;
                        case "指数减速":
                            rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.OutExpo);
                            break;
                        case "指数加速减速":
                            rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InOutExpo);
                            break;
                        case "超范围三次方加速缓动":
                            rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InBack);
                            break;
                        case "超范围三次方减速缓动":
                            rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.OutBack);
                            break;
                        case "超范围三次方加速减速缓动":
                            rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InOutBack);
                            break;
                        case "指数衰减加速反弹缓动":
                            rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InBounce);
                            break;
                        case "指数衰减减速反弹缓动":
                            rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.OutBounce);
                            break;
                        case "指数衰减加速减速反弹缓动":
                            rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InOutBounce);
                            break;
                        default:
                            rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.Linear);
                            break;
                    }
                    break;
                case "旋转抖动":
                    switch (DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.curve)
                    {
                        case "线性":
                            rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.Linear);
                            break;
                        case "三次方加速":
                            rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InCubic);
                            break;
                        case "三次方减速":
                            rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.OutCubic);
                            break;
                        case "三次方加速减速":
                            rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InOutCubic);
                            break;
                        case "指数加速":
                            rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InExpo);
                            break;
                        case "指数减速":
                            rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.OutExpo);
                            break;
                        case "指数加速减速":
                            rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InOutExpo);
                            break;
                        case "超范围三次方加速缓动":
                            rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InBack);
                            break;
                        case "超范围三次方减速缓动":
                            rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.OutBack);
                            break;
                        case "超范围三次方加速减速缓动":
                            rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InOutBack);
                            break;
                        case "指数衰减加速反弹缓动":
                            rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InBounce);
                            break;
                        case "指数衰减减速反弹缓动":
                            rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.OutBounce);
                            break;
                        case "指数衰减加速减速反弹缓动":
                            rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InOutBounce);
                            break;
                        default:
                            rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.Linear);
                            break;
                    }
                    break;
                case "缩放":
                    switch (DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.curve)
                    {
                        case "线性":
                            rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.Linear);
                            break;
                        case "三次方加速":
                            rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InCubic);
                            break;
                        case "三次方减速":
                            rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.OutCubic);
                            break;
                        case "三次方加速减速":
                            rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InOutCubic);
                            break;
                        case "指数加速":
                            rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InExpo);
                            break;
                        case "指数减速":
                            rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.OutExpo);
                            break;
                        case "指数加速减速":
                            rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InOutExpo);
                            break;
                        case "超范围三次方加速缓动":
                            rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InBack);
                            break;
                        case "超范围三次方减速缓动":
                            rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.OutBack);
                            break;
                        case "超范围三次方加速减速缓动":
                            rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InOutBack);
                            break;
                        case "指数衰减加速反弹缓动":
                            rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InBounce);
                            break;
                        case "指数衰减减速反弹缓动":
                            rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.OutBounce);
                            break;
                        case "指数衰减加速减速反弹缓动":
                            rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InOutBounce);
                            break;
                        default:
                            rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.Linear);
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        public void DoAnchorPosXMove(float xAxisMovement, float duration, string curve)
        {
            switch (curve)
            {
                case "线性":
                    rectTransform.DOAnchorPosX(width.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.Linear);
                    break;
                case "三次方加速":
                    rectTransform.DOAnchorPosX(width.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InCubic);
                    break;
                case "三次方减速":
                    rectTransform.DOAnchorPosX(width.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.OutCubic);
                    break;
                case "三次方加速减速":
                    rectTransform.DOAnchorPosX(width.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InOutCubic);
                    break;
                case "指数加速":
                    rectTransform.DOAnchorPosX(width.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InExpo);
                    break;
                case "指数减速":
                    rectTransform.DOAnchorPosX(width.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.OutExpo);
                    break;
                case "指数加速减速":
                    rectTransform.DOAnchorPosX(width.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InOutExpo);
                    break;
                case "超范围三次方加速缓动":
                    rectTransform.DOAnchorPosX(width.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InBack);
                    break;
                case "超范围三次方减速缓动":
                    rectTransform.DOAnchorPosX(width.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.OutBack);
                    break;
                case "超范围三次方加速减速缓动":
                    rectTransform.DOAnchorPosX(width.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InOutBack);
                    break;
                case "指数衰减加速反弹缓动":
                    rectTransform.DOAnchorPosX(width.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InBounce);
                    break;
                case "指数衰减减速反弹缓动":
                    rectTransform.DOAnchorPosX(width.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.OutBounce);
                    break;
                case "指数衰减加速减速反弹缓动":
                    rectTransform.DOAnchorPosX(width.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.InOutBounce);
                    break;
                default:
                    rectTransform.DOAnchorPosX(width.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(Ease.Linear);
                    break;
            }
        }

        public void SkipAnimation(int index)
        {
            if (DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.xAxisMovement < 0) return;
            DoAnchorPosXMove(DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.xAxisMovement, 0,
                DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.curve);
        }

        public void SetXAxisMovement(int index)
        {
            if (DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.xAxisMovement < 0) return;
            DoAnchorPosXMove(DialogueManager.Instance.dialogueContentCells[index].leftVerticalDrawings.xAxisMovement,
                DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.time,
                DialogueManager.Instance.dialogueContentCells[index].rightVerticalDrawings.curve);
        }
    }
}

