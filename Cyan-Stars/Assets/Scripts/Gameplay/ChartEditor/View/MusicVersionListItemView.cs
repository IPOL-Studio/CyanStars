#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class MusicVersionListItemView : BaseView<MusicVersionListItemViewModel>
    {
        [SerializeField]
        private Toggle itemToggle = null!;

        [SerializeField]
        private Image ledImage = null!;

        [SerializeField]
        private TMP_Text titleText = null!;


        public override void Bind(MusicVersionListItemViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ViewModel.IsSelected
                .Subscribe(selected =>
                    {
                        itemToggle.SetIsOnWithoutNotify(selected);
                        ledImage.enabled = selected;
                    }
                )
                .AddTo(this);
            ViewModel.MusicItemTitle
                .Subscribe(text =>
                    {
                        titleText.text = text;
                    }
                )
                .AddTo(this);

            itemToggle.onValueChanged.AddListener(ViewModel.OnToggleValueChanged);
        }

        protected override void OnDestroy()
        {
            if (ViewModel == null)
                return;

            itemToggle.onValueChanged.RemoveListener(ViewModel.OnToggleValueChanged);
            // ISynchronizedView 会自动在 View 卸载时释放对应的 ViewModel，无需手动 ViewModel.Dispose();
        }
    }
}
