#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
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
            Unbind(); // 从对象池创建时先取消既有订阅
            base.Bind(targetViewModel);

            itemToggle.isOn = ViewModel.IsSelected.Value;
            ledImage.enabled = ViewModel.IsSelected.Value;
            titleText.text = ViewModel.MusicItemTitle.Value;

            ViewModel.IsSelected.OnValueChanged += SetSelectStatus;
            ViewModel.MusicItemTitle.OnValueChanged += SetTitleText;

            itemToggle.onValueChanged.AddListener(ViewModel.OnToggleValueChanged);
        }

        private void SetSelectStatus(bool isSelected)
        {
            itemToggle.isOn = isSelected;
            ledImage.enabled = isSelected;
        }


        private void SetTitleText(string text)
        {
            titleText.text = text;
        }

        private void Unbind()
        {
            if (ViewModel == null)
                return;
            ViewModel.IsSelected.OnValueChanged -= SetSelectStatus;
            ViewModel.MusicItemTitle.OnValueChanged -= SetTitleText;
            itemToggle.onValueChanged.RemoveListener(ViewModel.OnToggleValueChanged);
        }

        protected override void OnDestroy()
        {
            Unbind();
        }
    }
}
