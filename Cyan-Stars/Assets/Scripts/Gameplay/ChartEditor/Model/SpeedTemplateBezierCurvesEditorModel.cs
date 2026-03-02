#nullable enable

using System.Linq;
using CyanStars.Chart.BezierCurve;
using ObservableCollections;
using R3;

public class SpeedTemplateBezierCurvesEditorModel
{
    // 原始列表数据（用于复用校验逻辑）
    private readonly BezierCurves OriginCurves;

    private readonly ObservableList<ReactiveProperty<BezierPoint>> points = new ObservableList<ReactiveProperty<BezierPoint>>();

    /// <summary>
    /// 暴露给制谱器的可观察贝塞尔曲线列表，校验通过后才更新
    /// </summary>
    public IReadOnlyObservableList<ReadOnlyReactiveProperty<BezierPoint>> Points =>
        new ObservableList<ReadOnlyReactiveProperty<BezierPoint>>(
            points.Select(rp => rp.ToReadOnlyReactiveProperty())
        );

    /// <summary>
    /// 构造函数
    /// </summary>
    public SpeedTemplateBezierCurvesEditorModel(BezierCurves originCurves)
    {
        OriginCurves = originCurves;

        foreach (var point in originCurves.Points)
        {
            points.Add(new ReactiveProperty<BezierPoint>(point));
        }
    }

    /// <summary>
    /// 将可观察数据转为标准列表，用于序列化
    /// </summary>
    public BezierCurves ToBezierCurves()
    {
        return OriginCurves;
    }


    /// <summary>
    /// 尝试添加一个点
    /// </summary>
    public bool TryAddPoint(ReactiveProperty<BezierPoint> newPoint)
    {
        if (OriginCurves.TryAdd(newPoint.CurrentValue, out int index))
        {
            points.Insert(index, newPoint);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 尝试更新/移动一个点
    /// </summary>
    public bool TryUpdatePoint(ReactiveProperty<BezierPoint> oldPoint, ReactiveProperty<BezierPoint> newPoint)
    {
        if (OriginCurves.TryReplace(oldPoint.CurrentValue, newPoint.CurrentValue))
        {
            int index = points.IndexOf(oldPoint);
            points[index] = newPoint;
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// 移除一个点
    /// </summary>
    public bool TryRemovePoint(ReactiveProperty<BezierPoint> oldPoint)
    {
        if (OriginCurves.Remove(oldPoint.CurrentValue))
        {
            points.Remove(oldPoint);
            return true;
        }
        else
        {
            return false;
        }
    }
}
