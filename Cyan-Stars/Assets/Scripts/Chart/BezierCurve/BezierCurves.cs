#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Chart.BezierCurve
{
    /// <summary>
    /// 用于变速模板的贝塞尔曲线组。对于任意给定的时间(x)，返回唯一的瞬时速度(y)
    /// </summary>
    /// <remarks>整条曲线是由多条首尾相连的三次贝塞尔曲线构成的，整条曲线在 x 轴上连续单调递增的，意味着每个 x 都有且仅有一个 y</remarks>
    public class BezierCurves : ICollection<BezierPoint>
    {
        // 用于插入贝塞尔点时的二分查找比较器
        private static readonly Comparer<BezierPoint> Comparer =
            Comparer<BezierPoint>.Create((x, y) =>
                x.PositionPoint.MsTime.CompareTo(y.PositionPoint.MsTime)
            );

        private readonly List<BezierPoint> points = new();
        public IReadOnlyList<BezierPoint> Points => points;

        public int Count => points.Count;
        public bool IsReadOnly => false;


        public BezierPoint this[int index] => points[index];

        /// <summary>
        /// 实例化贝塞尔曲线组
        /// </summary>
        /// <param name="firstBezierPoint">注意：首个元素的 PositionPoint.MsTime 应该等于 0</param>
        public BezierCurves(BezierPoint firstBezierPoint)
        {
            if (firstBezierPoint.PositionPoint.MsTime != 0)
                throw new ArgumentOutOfRangeException(nameof(firstBezierPoint.PositionPoint.MsTime), "首个元素的 PositionPoint.MsTime 应该等于 0");

            points.Add(firstBezierPoint);
        }

        public IEnumerator<BezierPoint> GetEnumerator()
        {
            return points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 自动在合适的位置插入贝塞尔点元素
        /// </summary>
        /// <exception cref="ArgumentException">元素已存在于列表中、元素位置点与其他位置点 x 值重复、元素控制点 x 值超过前/后一个元素位置点 x 值限制</exception>
        public void Add(BezierPoint item)
        {
            if (!AddValidate(item, out int index))
                throw new ArgumentException("无法添加贝塞尔点元素", nameof(item));

            points.Insert(index, item);
        }

        public void Clear()
        {
            points.Clear();
        }

        public bool Contains(BezierPoint item)
        {
            return points.Contains(item);
        }

        public void CopyTo(BezierPoint[] array, int arrayIndex)
        {
            points.CopyTo(array, arrayIndex);
        }

        public bool Remove(BezierPoint item)
        {
            if (points.IndexOf(item) == 0)
            {
                Debug.LogWarning("首个元素不能被删除，请使用 Replace() 替换之");
                return false;
            }

            return points.Remove(item);
        }


        /// <summary>
        /// 根据传入的时间返回此时的曲线 Value（瞬时速度）
        /// </summary>
        /// <param name="msTime">要查询的时间点，通常应该为正值</param>
        /// <remarks>时间点小于等于 0 时，返回首个贝塞尔点元素的 PositionPoint.Value；超过末个贝塞尔点元素的 PositionPoint.MsTime 时，返回末个贝塞尔点元素的 PositionPoint.Value</remarks>
        /// <returns>对应的曲线值</returns>
        public float EvaluateValue(int msTime)
        {
            if (msTime <= 0)
                return points[0].PositionPoint.Value;

            if (msTime >= points[^1].PositionPoint.MsTime)
                return points[^1].PositionPoint.Value;

            // 二分查找定位 msTime 所在的贝塞尔线段起始索引
            // 需要找到 index，使得 points[index].MsTime <= msTime < points[index + 1].MsTime
            int left = 0;
            int right = points.Count - 1;
            int startIndex = 0;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (points[mid].PositionPoint.MsTime <= msTime)
                {
                    startIndex = mid;
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }


            // 获取当前线段的起止点
            BezierPoint startNode = points[startIndex];
            BezierPoint endNode = points[startIndex + 1];

            // 根据 MsTime 计算贝塞尔插值参数 t
            float t = BezierHelper.FindTForX(
                msTime,
                startNode.PositionPoint.MsTime,
                startNode.RightControlPoint.MsTime,
                endNode.LeftControlPoint.MsTime,
                endNode.PositionPoint.MsTime
            );

            // 根据 t 计算 Value
            float value = (float)BezierHelper.CalculateValueForT(
                t,
                startNode.PositionPoint.Value,
                startNode.RightControlPoint.Value,
                endNode.LeftControlPoint.Value,
                endNode.PositionPoint.Value
            );

            return value;
        }

        /// <exception cref="NotSupportedException">试图删除首个元素</exception>
        public void RemoveAt(int index)
        {
            if (index == 0)
                throw new InvalidOperationException("首个元素不能被删除，请使用 Replace() 替换之");

            points.RemoveAt(index);
        }


        public bool TryReplace(BezierPoint oldItem, BezierPoint newItem)
        {
            int oldItemIndex = points.IndexOf(oldItem);
            return TryReplace(oldItemIndex, newItem);
        }

        public bool TryReplace(int oldItemIndex, BezierPoint newItem)
        {
            if (!ReplaceValidate(oldItemIndex, newItem))
                return false;

            points[oldItemIndex] = newItem;
            return true;
        }


        /// <summary>
        /// 校验给定的贝塞尔点能否插入到列表内
        /// </summary>
        /// <param name="item">要插入的贝塞尔点</param>
        /// <param name="index">如果校验通过，建议插入在此下标处</param>
        /// <returns>是否校验通过？在以下情况时为 false：元素已存在于列表中、元素位置点与其他位置点 x 值重复、元素控制点 x 值超过前/后一个元素位置点 x 值限制</returns>
        private bool AddValidate(BezierPoint item, out int index)
        {
            index = 0;

            if (points.Contains(item))
                return false;

            // 自定义比较器，按照 item.PositionPoint.MsTime 进行比较


            index = points.BinarySearch(item, Comparer);

            if (index >= 0) // 查找到了 Position.x 相同的元素
                return false;

            index = ~index; // 此后 index 代表建议插入的位置下标

            // 交叉校验插入点和相邻点的位置点和控制点
            // 除非插入在列表头部，否则需要校验左侧控制点是否超过上一个元素位置点&位置点是否超过上个元素右侧控制点
            if (index != 0)
            {
                if (!(points[index - 1].PositionPoint.MsTime <= item.LeftControlPoint.MsTime &&
                      points[index - 1].RightControlPoint.MsTime <= item.PositionPoint.MsTime))
                    return false;
            }

            // 除非插入在列表尾部，否则需要校验右侧控制点是否超过下一个元素位置点&位置点是否超过下个元素左侧控制点
            if (index != points.Count)
            {
                if (!(item.RightControlPoint.MsTime <= points[index].PositionPoint.MsTime &&
                      item.PositionPoint.MsTime <= points[index].LeftControlPoint.MsTime))
                    return false;
            }

            return true;
        }

        private bool ReplaceValidate(int oldItemIndex, BezierPoint newItem)
        {
            // 如果替换首个元素，要求 PositionPoint.MsTime == 0
            if (oldItemIndex == 0 && newItem.PositionPoint.MsTime != 0)
                return false;

            // 交叉校验新替换点和相邻点的位置点和控制点
            // 除非替换首个元素，否则需要校验左侧控制点是否超过上一个元素位置点&位置点是否超过上个元素右侧控制点
            if (oldItemIndex != 0)
            {
                if (!(points[oldItemIndex - 1].PositionPoint.MsTime < newItem.LeftControlPoint.MsTime &&
                      points[oldItemIndex - 1].RightControlPoint.MsTime <= newItem.PositionPoint.MsTime))
                    return false;
            }

            // 除非替换末个元素，否则需要校验右侧控制点是否超过下一个元素位置点&位置点是否超过下个元素左侧控制点
            if (oldItemIndex != points.Count - 1)
            {
                if (!(newItem.RightControlPoint.MsTime < points[oldItemIndex + 1].PositionPoint.MsTime &&
                      newItem.PositionPoint.MsTime <= points[oldItemIndex + 1].LeftControlPoint.MsTime))
                    return false;
            }

            return true;
        }
    }
}
