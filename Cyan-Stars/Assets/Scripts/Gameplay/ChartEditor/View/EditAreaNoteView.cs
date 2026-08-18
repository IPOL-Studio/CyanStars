#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    /// <summary>
    /// 动态创建。每个编辑区的 Note 持有一个 V。
    /// </summary>
    public class EditAreaNoteView : BaseView<EditAreaNoteViewModel>, IPointerDownHandler
    {
        private RectTransform rect = null!;

        [SerializeField]
        private Image blurImage = null!;

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

            var editAreaView = GetComponentInParent<EditAreaView>();
            if (editAreaView == null)
            {
                Debug.LogError("EditAreaNoteView must be placed under an EditAreaView.");
                return;
            }

            Observable.Merge(
                    targetViewModel.PositionChanged,
                    editAreaView.EditAreaViewModel.BeatZoom.Select(_ => Unit.Default)
                )
                .Subscribe(_ => ApplyPosition(editAreaView))
                .AddTo(this);

            ApplyPosition(editAreaView);
        }

        private void ApplyPosition(EditAreaView editAreaView)
        {
            double zoom = editAreaView.EditAreaViewModel.BeatZoom.CurrentValue;
            double beat = ViewModel.PositionBeat.CurrentValue;
            double endBeat = ViewModel.PositionEndBeat.CurrentValue;

            rect.anchoredPosition = editAreaView.CalculateNoteAnchoredPosition(ViewModel.Data, beat, zoom);

            if (holdTailRect != null && ViewModel.Data is HoldChartNoteData)
            {
                SetHoldLength(editAreaView.CalculateHoldLength(beat, endBeat, zoom));
            }
        }

        /// <summary>
        /// 设置 Hold 音符拖尾的长度
        /// </summary>
        /// <param name="length">拖尾高度，传 0 可隐藏拖尾（供预览音符使用）</param>
        public void SetHoldLength(float length)
        {
            if (holdTailRect != null)
                holdTailRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, length);
        }

        public void SetBlurImageRaycastTarget(bool value) => blurImage.raycastTarget = value;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                ViewModel.OnRightKeyDown();
            }
            else if (eventData.button == PointerEventData.InputButton.Left)
            {
                ViewModel.OnLeftKeyDown();
            }
        }
    }
}
