#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class SpeedTemplateListItemViewModel : BaseViewModel
    {
        private readonly SpeedTemplateViewModel SpeedTemplateViewModel;
        private readonly SpeedTemplateDataEditorModel SpeedTemplateData;


        private readonly ReactiveProperty<int> itemIndex;

        /// <summary>
        /// 当前变速组 item 在列表中的下标
        /// </summary>
        public ReadOnlyReactiveProperty<int> ItemIndex => itemIndex;

        public readonly ReadOnlyReactiveProperty<bool> IsSelected;


        public SpeedTemplateListItemViewModel(
            ChartEditorModel model,
            SpeedTemplateViewModel speedTemplateViewModel,
            SpeedTemplateDataEditorModel speedTemplateData
        )
            : base(model)
        {
            SpeedTemplateViewModel = speedTemplateViewModel;
            SpeedTemplateData = speedTemplateData;

            itemIndex = new ReactiveProperty<int>();


            IsSelected = SpeedTemplateViewModel.SelectedSpeedTemplateData
                .Select(selectedData => selectedData == SpeedTemplateData)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
        }

        /// <summary>
        /// 由父 VM 调用，刷新元素的 index
        /// </summary>
        public void SetIndex(int index)
        {
            if (itemIndex.Value != index)
                itemIndex.Value = index;
        }

        public void OnClick()
        {
            SpeedTemplateViewModel.SelectSpeedTemplateData(SpeedTemplateData);
        }
    }
}
