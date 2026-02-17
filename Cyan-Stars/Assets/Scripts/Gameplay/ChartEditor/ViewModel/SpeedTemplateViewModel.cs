#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Model;
using ObservableCollections;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class SpeedTemplateViewModel : BaseViewModel
    {
        /// <summary>
        /// 选中的变速模板内部的属性值发生变化（备注/变速类型/贝塞尔曲线组）
        /// </summary>
        public readonly Subject<SpeedTemplateData> SelectedSpeedTemplateDataPropertyUpdatedSubject = new();


        private readonly ReactiveProperty<SpeedTemplateData?> selectedSpeedTemplateData;
        public ReadOnlyReactiveProperty<SpeedTemplateData?> SelectedSpeedTemplateData => selectedSpeedTemplateData;

        public readonly ISynchronizedView<SpeedTemplateData, SpeedTemplateListItemViewModel> SpeedTemplateData;


        public SpeedTemplateViewModel(ChartEditorModel model)
            : base(model)
        {
            selectedSpeedTemplateData = new ReactiveProperty<SpeedTemplateData?>(null);

            SpeedTemplateData = Model.ChartData.CurrentValue.SpeedTemplateDatas
                .CreateView(data =>
                    new SpeedTemplateListItemViewModel(Model, this, data)
                )
                .AddTo(base.Disposables);
            SpeedTemplateData.ObserveChanged()
                .Subscribe(e => RefreshAllIndices())
                .AddTo(base.Disposables);
        }

        private void RefreshAllIndices()
        {
            int index = 0;
            foreach (var vm in SpeedTemplateData)
            {
                vm.SetIndex(index);
                index++;
            }
        }


        public void SelectSpeedTemplateData(SpeedTemplateData? data)
        {
            if (selectedSpeedTemplateData.CurrentValue != data)
                selectedSpeedTemplateData.Value = data;
        }

        public void AddSpeedTemplateData()
        {
            Model.ChartData.CurrentValue.SpeedTemplateDatas.Add(new SpeedTemplateData());
        }
    }
}
