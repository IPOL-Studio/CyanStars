using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;


namespace CyanStars.Dialogue
{
    public class VerticalDrawingBase : Image, ISetVerticalDrawing
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
            DialogueManager.Instance.OnSwitchDialog += SetImage;
            DialogueManager.Instance.OnAnimation += SetAnimation;
            DialogueManager.Instance.OnAnimation += SetXAxisMovement;
            DialogueManager.Instance.OnSkipAnimation += SkipAnimation;
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
        /// <param name="index"></param>
        public void SetImage(int index)
        {
            if (DialogueManager.Instance.dialogueContentCells[index].verticalDrawings[verticalDrawingID].file == "" || !DialogueManager.Instance.spriteDictionary.ContainsKey(DialogueManager.Instance.dialogueContentCells[index].verticalDrawings[verticalDrawingID].file))
            {
                color = Color.clear;
                return;
            }
            color = Color.white;
            sprite = DialogueManager.Instance.spriteDictionary[DialogueManager.Instance.dialogueContentCells[index].verticalDrawings[verticalDrawingID].file];
        }

        /// <summary>
        /// 立绘自身的动画
        /// </summary>
        /// <param name="index"></param>
        public void SetAnimation(int index)
        {
            if (DialogueManager.Instance.dialogueContentCells[index].verticalDrawings[verticalDrawingID].effect == "") return;

            float duration = DialogueManager.Instance.dialogueContentCells[index].verticalDrawings[verticalDrawingID].time;
            string curve = DialogueManager.Instance.dialogueContentCells[index].verticalDrawings[verticalDrawingID].curve;

            switch (DialogueManager.Instance.dialogueContentCells[index].verticalDrawings[verticalDrawingID].effect)
            {
                case "抖动":
                    tweener2 = rectTransform.DOShakeAnchorPos(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(DialogueManager.AnimationEase(curve));
                    break;
                case "旋转抖动":
                    tweener2 = rectTransform.DOShakeRotation(duration, new Vector2(5, 5), 50, 180f).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(DialogueManager.AnimationEase(curve));
                    break;
                case "缩放":
                    tweener2 = rectTransform.DOShakeScale(duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(DialogueManager.AnimationEase(curve));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 立绘的水平方向移动
        /// </summary>
        /// <param name="index"></param>
        public void SetXAxisMovement(int index)
        {
            if (DialogueManager.Instance.dialogueContentCells[index].verticalDrawings[verticalDrawingID].xAxisMovement < 0) return;
            float xAxisMovement = DialogueManager.Instance.dialogueContentCells[index].verticalDrawings[verticalDrawingID].xAxisMovement;
            float duration = DialogueManager.Instance.dialogueContentCells[index].verticalDrawings[verticalDrawingID].time;
            string curve = DialogueManager.Instance.dialogueContentCells[index].verticalDrawings[verticalDrawingID].curve;
            tweener = rectTransform.DOAnchorPosX(fatherRectTransform.sizeDelta.x * xAxisMovement, duration).OnPlay(AddStateCount).OnComplete(SubStateCount).SetEase(DialogueManager.AnimationEase(curve));
        }

        /// <summary>
        /// 停止并立即完成所有动画
        /// </summary>
        /// <param name="index"></param>
        public void SkipAnimation(int index)
        {
            tweener2.Kill(true);
            rectTransform.DOScale(new Vector3(1, 1, 1), 0.1f);
            rectTransform.DOSizeDelta(new Vector2(width, height), 0.1f);

            if (DialogueManager.Instance.dialogueContentCells[index].verticalDrawings[verticalDrawingID].xAxisMovement < 0)
            {
                return;
            }
            else
            {
                tweener.Kill(true);
                rectTransform.anchoredPosition = new Vector2(fatherRectTransform.sizeDelta.x * DialogueManager.Instance.dialogueContentCells[index].verticalDrawings[verticalDrawingID].xAxisMovement, rectTransform.anchoredPosition.y);
            }
        }
    }
}

