#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class EditAreaNoteView : BaseView<EditAreaNoteViewModel>
    {
        private RectTransform rect = null!;
        private Button noteButton = null!;

        [SerializeField]
        private RectTransform? holdTailRect; // 仅 Hold 音符需要赋值


        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            noteButton = GetComponent<Button>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        public override void Bind(EditAreaNoteViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            noteButton.onClick.AddListener(ViewModel.SelectedNote);

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

        protected override void OnDestroy()
        {
            noteButton.onClick.RemoveListener(ViewModel.SelectedNote);
        }
    }
}
