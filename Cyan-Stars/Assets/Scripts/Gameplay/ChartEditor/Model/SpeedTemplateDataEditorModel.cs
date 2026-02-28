#nullable enable

using System;
using CyanStars.Chart;
using CyanStars.Chart.BezierCurve;
using R3;

namespace CyanStars.Gameplay.ChartEditor.Model
{
    [Serializable]
    public class SpeedTemplateDataEditorModel
    {
        /// <summary>变速组名称</summary>
        /// <remarks>方便谱师识别，相当于备注，没有其他作用</remarks>
        public ReactiveProperty<string> Remark;

        /// <summary>变速组类型</summary>
        public ReactiveProperty<SpeedTemplateType> Type;

        /// <summary>贝塞尔曲线</summary>
        /// <remarks>
        /// 每个点的 x 坐标为相对于 Note 判定时间的提前时间，单位ms（有且仅有一个为 0，其余的 x 值必须是正数）
        /// y 坐标为谱师设定速度
        /// </remarks>
        public readonly SpeedTemplateBezierCurvesEditorModel BezierCurves;


        /// <summary>
        /// 构造函数：将 SpeedTemplateData 转为制谱器内的可观察实例
        /// </summary>
        public SpeedTemplateDataEditorModel(SpeedTemplateData speedTemplateData)
        {
            Remark = new ReactiveProperty<string>(speedTemplateData.Remark);
            Type = new ReactiveProperty<SpeedTemplateType>(speedTemplateData.Type);
            BezierCurves = new SpeedTemplateBezierCurvesEditorModel(speedTemplateData.BezierCurves);
        }


        /// <summary>
        /// 将可观察对象转为纯数据对象
        /// </summary>
        public SpeedTemplateData ToSpeedTemplateData()
        {
            var remark = Remark.CurrentValue;
            var type = Type.CurrentValue;
            var bezierCurves = BezierCurves.ToBezierCurves();
            return new SpeedTemplateData(remark, type, bezierCurves);
        }
    }
}
