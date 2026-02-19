#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Utils.SpeedTemplate
{
    /// <summary>
    /// 用于变速模板的贝塞尔曲线组。对于任意给定的时间(x)，返回唯一的瞬时速度(y)
    /// </summary>
    /// <remarks>整条曲线是由多条首尾相连的三次贝塞尔曲线构成的，整条曲线在 x 轴上连续单调递增的，意味着每个 x 都有且仅有一个 y</remarks>
    public class BezierCurves : ICollection<BezierPoint>
    {
        // 二分法根据 x 查找 t 时的深度和精度
        private const int MaxIterations = 20; // 查找深度
        private const float Epsilon = 0.0001f; // 查找精度

        // 用于插入贝塞尔点时的二分查找比较器
        private static readonly Comparer<BezierPoint> Comparer =
            Comparer<BezierPoint>.Create((x, y) =>
                x.PositionPoint.MsTime.CompareTo(y.PositionPoint.MsTime)
            );

        private readonly List<BezierPoint> Points = new();

        public int Count => Points.Count;
        public bool IsReadOnly => false;

        /// <remarks>
        /// 不要随意在外部修改坐标点和控制点的 MsTime，可能会导致数组排序异常！涉及到从修改时请先删掉旧元素再添加新元素。
        /// </remarks>
        public BezierPoint this[int index] => Points[index];

        /// <summary>
        /// 实例化贝塞尔曲线组
        /// </summary>
        /// <param name="firstBezierPoint">注意：首个元素的 PositionPoint.MsTime 应该等于 0</param>
        public BezierCurves(BezierPoint firstBezierPoint)
        {
            if (firstBezierPoint.PositionPoint.MsTime != 0)
                throw new ArgumentOutOfRangeException(nameof(firstBezierPoint.PositionPoint.MsTime), "首个元素的 PositionPoint.MsTime 应该等于 0");

            Points.Add(firstBezierPoint);
        }

        public IEnumerator<BezierPoint> GetEnumerator()
        {
            return Points.GetEnumerator();
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

            Points.Insert(index, item);
        }

        public void Clear()
        {
            Points.Clear();
        }

        public bool Contains(BezierPoint item)
        {
            return Points.Contains(item);
        }

        public void CopyTo(BezierPoint[] array, int arrayIndex)
        {
            Points.CopyTo(array, arrayIndex);
        }

        public bool Remove(BezierPoint item)
        {
            if (Points.IndexOf(item) == 0)
            {
                Debug.LogWarning("首个元素不能被删除，请使用 Replace() 替换之");
                return false;
            }

            return Points.Remove(item);
        }


        /// <summary>
        /// 根据传入的时间返回此时的曲线值
        /// </summary>
        /// <param name="msTime">要查询的时间点，通常应该为正值</param>
        /// <remarks>时间点小于等于 0 时，返回首个贝塞尔点元素的 PositionPoint.Value；超过末个贝塞尔点元素的 PositionPoint.MsTime 时，返回末个贝塞尔点元素的 PositionPoint.Value</remarks>
        /// <returns>对应的曲线值</returns>
        public float EvaluateValue(int msTime)
        {
            if (msTime <= 0)
                return Points[0].PositionPoint.Value;

            if (msTime >= Points[^1].PositionPoint.MsTime)
                return Points[^1].PositionPoint.Value;

            // 二分查找定位 msTime 所在的贝塞尔线段起始索引
            // 需要找到 index，使得 Points[index].MsTime <= msTime < Points[index + 1].MsTime
            int left = 0;
            int right = Points.Count - 1;
            int startIndex = 0;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (Points[mid].PositionPoint.MsTime <= msTime)
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
            BezierPoint startNode = Points[startIndex];
            BezierPoint endNode = Points[startIndex + 1];

            // 根据 MsTime 计算贝塞尔插值参数 t
            float t = FindTForX(
                msTime,
                startNode.PositionPoint.MsTime,
                startNode.RightControlPoint.MsTime,
                endNode.LeftControlPoint.MsTime,
                endNode.PositionPoint.MsTime
            );

            // 根据 t 计算 Value
            float value = CalculateValueForT(
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
                throw new NotSupportedException("首个元素不能被删除，请使用 Replace() 替换之");

            Points.RemoveAt(index);
        }


        public bool TryReplace(BezierPoint oldItem, BezierPoint newItem)
        {
            int oldItemIndex = Points.IndexOf(oldItem);
            return TryReplace(oldItemIndex, newItem);
        }

        public bool TryReplace(int oldItemIndex, BezierPoint newItem)
        {
            if (!ReplaceValidate(oldItemIndex, newItem))
                return false;

            Points[oldItemIndex] = newItem;
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

            if (Points.Contains(item))
                return false;

            // 自定义比较器，按照 item.PositionPoint.MsTime 进行比较


            index = Points.BinarySearch(item, Comparer);

            if (index >= 0) // 查找到了 Position.x 相同的元素
                return false;

            index = ~index; // 此后 index 代表建议插入的位置下标

            // 交叉校验插入点和相邻点的位置点和控制点
            // 除非插入在列表头部，否则需要校验左侧控制点是否超过上一个元素位置点&位置点是否超过上个元素右侧控制点
            if (index != 0)
            {
                if (!(Points[index - 1].PositionPoint.MsTime <= item.LeftControlPoint.MsTime &&
                      Points[index - 1].RightControlPoint.MsTime <= item.PositionPoint.MsTime))
                    return false;
            }

            // 除非插入在列表尾部，否则需要校验右侧控制点是否超过下一个元素位置点&位置点是否超过下个元素左侧控制点
            if (index != Points.Count)
            {
                if (!(item.RightControlPoint.MsTime <= Points[index].PositionPoint.MsTime &&
                      item.PositionPoint.MsTime <= Points[index].LeftControlPoint.MsTime))
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
                if (!(Points[oldItemIndex - 1].PositionPoint.MsTime < newItem.LeftControlPoint.MsTime &&
                      Points[oldItemIndex - 1].RightControlPoint.MsTime <= newItem.PositionPoint.MsTime))
                    return false;
            }

            // 除非替换末个元素，否则需要校验右侧控制点是否超过下一个元素位置点&位置点是否超过下个元素左侧控制点
            if (oldItemIndex != Points.Count - 1)
            {
                if (!(newItem.RightControlPoint.MsTime < Points[oldItemIndex + 1].PositionPoint.MsTime &&
                      newItem.PositionPoint.MsTime <= Points[oldItemIndex + 1].LeftControlPoint.MsTime))
                    return false;
            }

            return true;
        }


        /// <summary>
        /// 二分法查找给定 x 值在贝塞尔曲线段上的参数 t
        /// </summary>
        private float FindTForX(float x, float p0X, float p1X, float p2X, float p3X)
        {
            float tLow = 0;
            float tHigh = 1;
            float t;
            float currentX;
            int iterations = 0;

            do
            {
                t = (tLow + tHigh) / 2;
                currentX = CalculateValueForT(t, p0X, p1X, p2X, p3X);

                if (currentX > x)
                {
                    tHigh = t;
                }
                else
                {
                    tLow = t;
                }

                iterations++;
            } while (Mathf.Abs(currentX - x) > Epsilon && iterations < MaxIterations);

            return t;
        }

        /// <summary>
        /// 计算一维三次贝塞尔曲线在 t 处的值
        /// </summary>
        private float CalculateValueForT(float t, float p0, float p1, float p2, float p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            float p = uuu * p0; // (1-t)^3 * P0
            p += 3 * uu * t * p1; // 3 * (1-t)^2 * t * P1
            p += 3 * u * tt * p2; // 3 * (1-t) * t^2 * P2
            p += ttt * p3; // t^3 * P3

            return p;
        }
    }
}
