#nullable enable

using CyanStars.Chart.BezierCurve;
using R3;

namespace CyanStars.Gameplay.ChartEditor.Model
{
    /// <summary>
    /// 用类来包装结构体，以便子 VM 和 V 在修改数据时不会重新创建
    /// </summary>
    public class BezierPointWrapperModel
    {
        public readonly ReadOnlyReactiveProperty<BezierPoint> Point;

        public BezierPointWrapperModel(BezierPoint bezierPoint)
        {
            Point = new ReactiveProperty<BezierPoint>(bezierPoint);
        }
    }
}
