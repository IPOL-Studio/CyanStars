// TODO: 待重构

#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CyanStars.Gameplay.ChartEditor.View
{
    /// <summary>
    /// 动态创建。每个编辑区的 Note 持有一个 V。
    /// </summary>
    public class EditAreaNoteView : BaseView<EditAreaNoteViewModel>, IPointerClickHandler
    {
        private RectTransform rect = null!;

        [SerializeField]
        private RectTransform? holdTailRect; // 仅 Hold 音符需要赋值


        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        public override void Bind(EditAreaNoteViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            targetViewModel.AnchoredPosition
                .Subscribe(pos => rect.anchoredPosition = pos)
                .AddTo(this);

            if (holdTailRect != null)
            {
                targetViewModel.HoldLength
                    .Subscribe(length => holdTailRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, length))
                    .AddTo(this);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                ViewModel.OnRightClick();
            }
            else if (eventData.button == PointerEventData.InputButton.Left)
            {
                ViewModel.OnLeftClick();
            }
        }
    }
}
