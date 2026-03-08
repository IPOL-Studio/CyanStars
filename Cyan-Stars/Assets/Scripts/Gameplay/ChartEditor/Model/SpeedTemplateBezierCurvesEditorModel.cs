#nullable enable

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
    public IReadOnlyObservableList<ReactiveProperty<BezierPoint>> Points => points;


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

    public int GetPointIndex(ReadOnlyReactiveProperty<BezierPoint> pointWrapper)
    {
        return points.IndexOf((ReactiveProperty<BezierPoint>)pointWrapper);
    }


    /// <summary>
    /// 尝试添加一个点
    /// </summary>
    public bool TryAddPoint(BezierPoint newPoint)
    {
        if (OriginCurves.TryAdd(newPoint, out int index))
        {
            points.Insert(index, new ReactiveProperty<BezierPoint>(newPoint));
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
    public bool TryUpdatePoint(ReadOnlyReactiveProperty<BezierPoint> oldPointWrapper, BezierPoint newPoint)
    {
        if (OriginCurves.TryReplace(oldPointWrapper.CurrentValue, newPoint))
        {
            int index = points.IndexOf((ReactiveProperty<BezierPoint>)oldPointWrapper);
            points[index].Value = newPoint;
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
    public bool TryRemovePoint(ReadOnlyReactiveProperty<BezierPoint> oldPointWrapper)
    {
        if (OriginCurves.Remove(oldPointWrapper.CurrentValue))
        {
            points.Remove((ReactiveProperty<BezierPoint>)oldPointWrapper);
            return true;
        }
        else
        {
            return false;
        }
    }
}
