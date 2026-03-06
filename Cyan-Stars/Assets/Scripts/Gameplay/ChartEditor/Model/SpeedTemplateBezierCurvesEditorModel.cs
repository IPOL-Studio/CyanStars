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

    // 当在列表末尾新增或删除元素时可能会变化，需要对 wrapper 进行观察
    private readonly ReactiveProperty<ReadOnlyReactiveProperty<BezierPoint>> lastPointWrapper = new();
    public ReadOnlyReactiveProperty<ReadOnlyReactiveProperty<BezierPoint>> LastPointWrapper => lastPointWrapper;


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

        lastPointWrapper.Value = points[^1];
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
    public bool TryAddPoint(BezierPoint newPoint)
    {
        if (OriginCurves.TryAdd(newPoint, out int index))
        {
            points.Insert(index, new ReactiveProperty<BezierPoint>(newPoint));
            lastPointWrapper.Value = points[^1];
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
            lastPointWrapper.Value = points[^1];
            return true;
        }
        else
        {
            return false;
        }
    }
}
