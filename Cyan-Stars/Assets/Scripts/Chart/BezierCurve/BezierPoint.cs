#nullable enable

using System;
using UnityEngine;

namespace CyanStars.Chart.BezierCurve
{
    /// <summary>
    /// 贝塞尔点元素
    /// </summary>
    /// <remarks>
    /// 每个 bezierPoint 由三个点组成：
    /// - Position：位置点，曲线必然穿过此点
    /// - LeftControlPoint：左形变控制点，调整本条曲线和上一条曲线的形状
    /// - RightControlPoint：右形变控制点，调整本条曲线和下一条曲线的形状
    /// </remarks>
    public readonly struct BezierPoint : IEquatable<BezierPoint>
    {
        public readonly BezierPointPos PositionPoint;
        public readonly BezierPointPos LeftControlPoint;
        public readonly BezierPointPos RightControlPoint;

        public BezierPoint(BezierPointPos positionPoint, BezierPointPos leftControlPoint, BezierPointPos rightControlPoint)
        {
            if (positionPoint.MsTime < 0 || leftControlPoint.MsTime < 0 || rightControlPoint.MsTime < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(positionPoint), "尚不支持小于 0 的时间");
            }

            if (leftControlPoint.MsTime > positionPoint.MsTime || rightControlPoint.MsTime < positionPoint.MsTime)
            {
                // 显然左手要在身体和右手的左边而右手要在身体和左手的右边
                throw new ArgumentException("左控制点必须在位置点左边，右控制点必须在位置点右边");
            }

            PositionPoint = positionPoint;
            LeftControlPoint = leftControlPoint;
            RightControlPoint = rightControlPoint;
        }

        public bool Equals(BezierPoint other)
        {
            return PositionPoint.Equals(other.PositionPoint) && LeftControlPoint.Equals(other.LeftControlPoint) && RightControlPoint.Equals(other.RightControlPoint);
        }

        public override bool Equals(object? obj)
        {
            return obj is BezierPoint other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PositionPoint, LeftControlPoint, RightControlPoint);
        }

        public static bool operator ==(BezierPoint left, BezierPoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BezierPoint left, BezierPoint right)
        {
            return !left.Equals(right);
        }
    }

    public readonly struct BezierPointPos : IEquatable<BezierPointPos>
    {
        public int MsTime { get; }
        public float Value { get; }

        public BezierPointPos(int msTime, float value)
        {
            MsTime = msTime;
            Value = value;
        }

        public bool Equals(BezierPointPos other)
        {
            return MsTime == other.MsTime && Value.Equals(other.Value);
        }

        public override bool Equals(object? obj)
        {
            return obj is BezierPointPos other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MsTime, Value);
        }

        public static bool operator ==(BezierPointPos left, BezierPointPos right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BezierPointPos left, BezierPointPos right)
        {
            return !left.Equals(right);
        }
    }
}
