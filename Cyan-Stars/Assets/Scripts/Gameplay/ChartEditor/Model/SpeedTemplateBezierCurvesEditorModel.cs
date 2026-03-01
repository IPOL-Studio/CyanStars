#nullable enable

using CyanStars.Chart.BezierCurve;
using CyanStars.Gameplay.ChartEditor.Model;
using ObservableCollections;

public class SpeedTemplateBezierCurvesEditorModel
{
    // 原始列表数据（用于复用校验逻辑）
    private readonly BezierCurves OriginCurves;

    private readonly ObservableList<BezierPointWrapperModel> points = new ObservableList<BezierPointWrapperModel>();

    /// <summary>
    /// 暴露给制谱器的可观察贝塞尔曲线列表，校验通过后才更新
    /// </summary>
    public IReadOnlyObservableList<BezierPointWrapperModel> Points => points;

    /// <summary>
    /// 构造函数
    /// </summary>
    public SpeedTemplateBezierCurvesEditorModel(BezierCurves originCurves)
    {
        OriginCurves = originCurves;
        foreach (var point in originCurves.Points)
        {
            points.Add(new BezierPointWrapperModel(point));
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
    public bool TryAddPoint(BezierPointWrapperModel newPoint)
    {
        if (OriginCurves.TryAdd(newPoint.Point.CurrentValue, out int index))
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
    public bool TryUpdatePoint(BezierPointWrapperModel oldPoint, BezierPointWrapperModel newPoint)
    {
        if (OriginCurves.TryReplace(oldPoint.Point.CurrentValue, newPoint.Point.CurrentValue))
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
    public bool TryRemovePoint(BezierPointWrapperModel oldPoint)
    {
        if (OriginCurves.Remove(oldPoint.Point.CurrentValue))
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
