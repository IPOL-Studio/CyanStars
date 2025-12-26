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
            // 从对象池创建时先取消既有绑定，然后重新绑定
            Unbind();
            ViewModel?.Dispose();
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

        private void Unbind()
        {
            if (ViewModel == null)
                return;
            itemToggle.onValueChanged.RemoveListener(ViewModel.OnToggleValueChanged);
        }

        protected override void OnDestroy()
        {
            Unbind();
            ViewModel?.Dispose();
        }
    }
}
