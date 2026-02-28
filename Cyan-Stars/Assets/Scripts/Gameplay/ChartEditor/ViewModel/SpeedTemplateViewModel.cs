#nullable enable

using System;
using CyanStars.Chart;
using CyanStars.Chart.BezierCurve;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using ObservableCollections;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class SpeedTemplateViewModel : BaseViewModel
    {
        private readonly ReactiveProperty<SpeedTemplateDataEditorModel?> selectedSpeedTemplateData;
        public ReadOnlyReactiveProperty<SpeedTemplateDataEditorModel?> SelectedSpeedTemplateData => selectedSpeedTemplateData;

        public readonly ISynchronizedView<SpeedTemplateDataEditorModel, SpeedTemplateListItemViewModel> SpeedTemplateDatas;


        public SpeedTemplateViewModel(ChartEditorModel model)
            : base(model)
        {
            selectedSpeedTemplateData = new ReactiveProperty<SpeedTemplateDataEditorModel?>(null);

            SpeedTemplateDatas = Model.ChartData.CurrentValue.SpeedTemplateDatas
                .CreateView(data =>
                    new SpeedTemplateListItemViewModel(Model, this, data)
                )
                .AddTo(base.Disposables);
            SpeedTemplateDatas.ObserveChanged()
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
            foreach (var vm in SpeedTemplateDatas)
            {
                vm.SetIndex(index);
                index++;
            }
        }


        public void SelectSpeedTemplateData(SpeedTemplateDataEditorModel? data)
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
            Model.ChartData.CurrentValue.SpeedTemplateDatas.Add(
                new SpeedTemplateDataEditorModel(
                    new SpeedTemplateData(bezierCurves: bezierCurves)
                )
            );
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
            var newSpeedTemplateData = new SpeedTemplateData(oldData.Remark.CurrentValue, oldData.Type.CurrentValue, oldData.BezierCurves.ToBezierCurves());
            var newData = new SpeedTemplateDataEditorModel(newSpeedTemplateData);

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
