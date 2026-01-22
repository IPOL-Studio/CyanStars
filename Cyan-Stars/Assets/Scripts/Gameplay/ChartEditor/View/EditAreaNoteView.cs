#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class EditAreaNoteView : BaseView<EditAreaNoteViewModel>
    {
        public RectTransform Rect { get; private set; } = null!;

        [SerializeField]
        private RectTransform? holdTailRect; // 仅 Hold 音符需要赋值

        private void Awake()
        {
            Rect = GetComponent<RectTransform>();
            Rect.anchorMin = new Vector2(0.5f, 0f);
            Rect.anchorMax = new Vector2(0.5f, 0f);
            Rect.pivot = new Vector2(0.5f, 0.5f);
        }

        public override void Bind(EditAreaNoteViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            targetViewModel.AnchoredPosition
                .Subscribe(pos => Rect.anchoredPosition = pos)
                .AddTo(this);

            if (holdTailRect != null)
            {
                targetViewModel.HoldLength
                    .Subscribe(length => holdTailRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, length))
                    .AddTo(this);
            }
        }

        protected override void OnDestroy()
        {
            // View 销毁时 ViewModel 由外部管理（EditAreaView）进行 Dispose
        }
    }
}
