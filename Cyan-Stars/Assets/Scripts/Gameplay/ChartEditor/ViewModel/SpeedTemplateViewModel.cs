#nullable enable

using System;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Utils.SpeedTemplate;
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

        /// <summary>
        /// 工厂方法：实例化新的 SpeedTemplateCurveFrameViewModel 子 VM
        /// </summary>
        public SpeedTemplateCurveFrameViewModel NewCurveFrameViewModel()
        {
            return new SpeedTemplateCurveFrameViewModel(Model, this);
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
            BezierCurves bezierCurves = new BezierCurves(
                new BezierPoint(
                    new BezierPointPos(0, 0),
                    new BezierPointPos(0, 0),
                    new BezierPointPos(0, 0)
                )
            )
            {
                new BezierPoint(
                    new BezierPointPos(500, 100),
                    new BezierPointPos(200, 100),
                    new BezierPointPos(800, 100)
                ),
                new BezierPoint(
                    new BezierPointPos(1000, 0),
                    new BezierPointPos(1000, 0),
                    new BezierPointPos(1000, 0)
                )
            };
            Model.ChartData.CurrentValue.SpeedTemplateDatas.Add(new SpeedTemplateData(bezierCurves: bezierCurves));
        }

        public void DeleteSelectedSpeedTemplateData()
        {
            if (selectedSpeedTemplateData.CurrentValue == null)
                throw new Exception("未选中变速模板时不可删除");

            var oldData = selectedSpeedTemplateData.CurrentValue;
            var oldIndex = Model.ChartData.CurrentValue.SpeedTemplateDatas.IndexOf(oldData);

            CommandStack.ExecuteCommand(
                () =>
                {
                    selectedSpeedTemplateData.Value = null;
                    Model.ChartData.CurrentValue.SpeedTemplateDatas.Remove(oldData);
                },
                () =>
                {
                    Model.ChartData.CurrentValue.SpeedTemplateDatas.Insert(oldIndex, oldData);
                    selectedSpeedTemplateData.Value = oldData;
                }
            );
        }

        public void CloneSelectedSpeedTemplateData()
        {
            if (selectedSpeedTemplateData.CurrentValue == null)
                throw new Exception("未选中变速模板时不可删复制");

            var oldData = selectedSpeedTemplateData.CurrentValue;
            var newData = new SpeedTemplateData(oldData.Remark, oldData.Type, oldData.BezierCurves);
            CommandStack.ExecuteCommand(
                () =>
                {
                    Model.ChartData.CurrentValue.SpeedTemplateDatas.Add(newData);
                }
                , () =>
                {
                    Model.ChartData.CurrentValue.SpeedTemplateDatas.Remove(newData);
                }
            );
        }
    }
}
