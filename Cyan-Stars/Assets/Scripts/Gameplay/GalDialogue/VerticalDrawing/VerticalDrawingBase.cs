using System;
using CyanStars.Framework;
using UnityEngine.UI;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;


namespace CyanStars.Gameplay.Dialogue
{
    public class VerticalDrawingBase : Image
    {
        private RectTransform fatherRectTransform;
        public int verticalDrawingID;
        private float width;
        private float height;
        private Tweener tweener;
        private Tweener tweener2;

        protected override void Start()
        {
            fatherRectTransform = DialogueManager.Instance.rectTransform;
            var rect = rectTransform.rect;
            width = rect.width;
            height = rect.height;
            // DialogueManager.Instance.OnSwitchDialog += SetImage;
            // DialogueManager.Instance.OnAnimation += SetAnimation;
            // DialogueManager.Instance.OnAnimation += SetXAxisMovement;
            // DialogueManager.Instance.OnSkipAnimation += SkipAnimation;
            GameRoot.Event.AddListener(DialogueEventConst.GalSwitchDialog, SetImage);
            GameRoot.Event.AddListener(DialogueEventConst.GalSkipAnimation, SetAnimation);
            GameRoot.Event.AddListener(DialogueEventConst.GalSkipAnimation, SetXAxisMovement);
            GameRoot.Event.AddListener(DialogueEventConst.GalSkipAnimation, SkipAnimation);
        }

        /// <summary>
        /// 增加动画状态计数
        /// </summary>
        private void AddStateCount()
        {
            DialogueManager.Instance.stateCount++;
        }

        /// <summary>
        /// 减少动画状态计数
        /// </summary>
        private void SubStateCount()
        {
            DialogueManager.Instance.stateCount--;
        }

        /// <summary>
        /// 切换立绘（如果表中的立绘为空或不存在则将颜色清除）
        /// </summary>
        private void SetImage(object sender, EventArgs e)
        {
            DialogueEventArgs args = (DialogueEventArgs)e;
            if (DialogueManager.Instance.dialogueContentCells[args.index].verticalDrawings[verticalDrawingID].file == "" || !DialogueManager.Instance.spriteDictionary.ContainsKey(DialogueManager.Instance.dialogueContentCells[args.index].verticalDrawings[verticalDrawingID].file))
            {
                color = Color.clear;
                return;
            }
            color = Color.white;
            sprite = DialogueManager.Instance.spriteDictionary[DialogueManager.Instance.dialogueContentCells[args.index].verticalDrawings[verticalDrawingID].file];
        }

        /// <summary>
        /// 立绘自身的动画
        /// </summary>
        public void SetAnimation(object sender, EventArgs e)
        {
            DialogueEventArgs args = (DialogueEventArgs)e;
            if (DialogueManager.Instance.dialogueContentCells[args.index].verticalDrawings[verticalDrawingID].effect == "") return;

            float duration = DialogueManager.Instance.dialogueContentCells[args.index].verticalDrawings[verticalDrawingID].time;
            string curve = DialogueManager.Instance.dialogueContentCells[args.index].verticalDrawings[verticalDrawingID].curve;

            switch (DialogueManager.Instance.dialogueContentCells[args.index].verticalDrawings[verticalDrawingID].effect)
            {
                case "Shake":
                    tweener2 = rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(DialogueManager.AnimationEase(curve));
                    break;
                case "ShakeRotation":
                    tweener2 = rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(DialogueManager.AnimationEase(curve));
                    break;
                case "ShakeScale":
                    tweener2 = rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(DialogueManager.AnimationEase(curve));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 停止并立即完成所有动画
        /// </summary>
        public void SkipAnimation(object sender, EventArgs e)
        {
            DialogueEventArgs args = (DialogueEventArgs)e;
            tweener2.Kill(true);
            rectTransform.DOScale(new Vector3(1, 1, 1), 0.1f);
            rectTransform.DOSizeDelta(new Vector2(width, height), 0.1f);

            if (DialogueManager.Instance.dialogueContentCells[args.index].verticalDrawings[verticalDrawingID].xAxisMovement < 0)
            {
                return;
            }
            else
            {
                tweener.Kill(true);
                rectTransform.anchoredPosition = new Vector2(fatherRectTransform.sizeDelta.x * DialogueManager.Instance.dialogueContentCells[args.index].verticalDrawings[verticalDrawingID].xAxisMovement, rectTransform.anchoredPosition.y);
            }
        }

        /// <summary>
        /// 立绘的水平方向移动
        /// </summary>
        private void SetXAxisMovement(object sender, EventArgs e)
        {
            DialogueEventArgs args = (DialogueEventArgs)e;
            if (DialogueManager.Instance.dialogueContentCells[args.index].verticalDrawings[verticalDrawingID].xAxisMovement < 0) return;
            float xAxisMovement = DialogueManager.Instance.dialogueContentCells[args.index].verticalDrawings[verticalDrawingID].xAxisMovement;
            float duration = DialogueManager.Instance.dialogueContentCells[args.index].verticalDrawings[verticalDrawingID].time;
            string curve = DialogueManager.Instance.dialogueContentCells[args.index].verticalDrawings[verticalDrawingID].curve;
            tweener = rectTransform.DOAnchorPosX(fatherRectTransform.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(DialogueManager.AnimationEase(curve));
        }
    }
}

