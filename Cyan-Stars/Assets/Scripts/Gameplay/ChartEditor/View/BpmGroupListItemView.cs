#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    /// <summary>
    /// 动态生成。关闭精简模式后 BpmGroup 弹窗左侧的 ListItem。每个 Item 持有一个 View。
    /// </summary>
    public class BpmGroupListItemView : BaseView<BpmGroupListItemViewModel>
    {
        [SerializeField]
        private Toggle itemToggle = null!;

        [SerializeField]
        private RawImage ledRawImage = null!;

        [SerializeField]
        private TMP_Text itemNumberText = null!;

        [SerializeField]
        private TMP_Text beatAndTimeText = null!;


        public override void Bind(BpmGroupListItemViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            itemNumberText.text = $"#{(ViewModel.ItemIndex + 1).ToString()}";
            ViewModel.IsSelected
                .Subscribe(isSelected => ledRawImage.enabled = isSelected)
                .AddTo(this);
            ViewModel.BeatAndTimeString
                .Subscribe(text => beatAndTimeText.text = text)
                .AddTo(this);

            itemToggle
                .OnValueChangedAsObservable()
                .Subscribe(ViewModel.OnToggleChanged)
                .AddTo(this);
        }

        protected override void OnDestroy()
        {
        }
    }
}
