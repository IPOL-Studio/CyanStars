using System;
using CyanStars.Chart.BezierCurve;

namespace CyanStars.Chart
{
    public enum SpeedTemplateType
    {
        /// <summary>相对速度</summary>
        /// <remarks>最终速度 = 谱师设定速度 * 玩家设定速度，用于常规变速</remarks>
        Relative,

        /// <summary>绝对速度</summary>
        /// <remarks>最终速度 = 谱师设定速度，用于谱面表演</remarks>
        Absolute
    }

    [Serializable]
    public class SpeedTemplateData
    {
        /// <summary>变速组名称</summary>
        /// <remarks>方便谱师识别，相当于备注，没有其他作用</remarks>
        public string Remark;

        /// <summary>变速组类型</summary>
        public SpeedTemplateType Type;

        /// <summary>贝塞尔曲线</summary>
        /// <remarks>
        /// 每个点的 x 坐标为相对于 Note 判定时间的提前时间，单位ms（有且仅有一个为 0，其余的 x 值必须是负数）
        /// y 坐标为谱师设定速度
        /// </remarks>
        public BezierCurves BezierCurves;


        public SpeedTemplateData(string remark = "新变速模板", SpeedTemplateType type = SpeedTemplateType.Relative, BezierCurves bezierCurves = null)
        {
            Remark = remark;
            Type = type;
            BezierCurves = bezierCurves ?? new BezierCurves(
                new BezierPoint(
                    new BezierPointPos(0, 1f),
                    new BezierPointPos(0, 1f),
                    new BezierPointPos(0, 1f)
                )
            );
        }
    }
}
