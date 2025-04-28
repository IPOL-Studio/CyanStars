using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 贝塞尔曲线
    /// </summary>
    /// <remarks>一整条曲线，由首尾相连的多个贝塞尔区间组成，即一个变速组</remarks>
    public class BezierCurve
    {
        public List<CubicBezierCurve> CubicBeziers = new List<CubicBezierCurve>();


        /// <summary>
        /// 排序列表，并舍弃不连续的曲线
        /// </summary>
        /// <exception cref="InvalidOperationException">未找到起始曲线或找到多个起始曲线（起始曲线：Time = 0 的曲线）</exception>
        private void Sort()
        {
            // 搜索 P0.Time=0 的曲线，作为第一条曲线，未找到或找到多条则抛异常
            // 获取这条曲线的 P3 坐标，搜索下一个 P0 等于这个坐标的曲线，作为下一条曲线，重复
            // 如果搜索时出现任何重复，抛异常
            // 如果搜索结束，舍弃掉未连接的曲线

            List<CubicBezierCurve> sortedList = new List<CubicBezierCurve>();
            List<CubicBezierCurve> remainingCurves = new List<CubicBezierCurve>(CubicBeziers);

            // 查找起始曲线（P0.Time == 0）
            CubicBezierCurve startCurve = remainingCurves.FirstOrDefault(c => c.P0.Time == 0);
            if (startCurve == null)
            {
                throw new InvalidOperationException("No starting curve with P0.Time = 0.");
            }
            if (remainingCurves.Count(c => c.P0.Time == 0) > 1)
            {
                throw new InvalidOperationException("Multiple curves with P0.Time = 0.");
            }

            sortedList.Add(startCurve);
            remainingCurves.Remove(startCurve);

            CubicBezierCurve current = startCurve;
            while (true)
            {
                // 根据右侧曲线的 P3 作为左侧曲线（待查找曲线）的 P0
                BezierControlPoint targetP0 = current.P3;
                CubicBezierCurve nextCurve = remainingCurves.FirstOrDefault(c =>
                    c.P0.Time == targetP0.Time &&
                    Mathf.Approximately(c.P0.Value, targetP0.Value));

                if (nextCurve == null)
                {
                    // 未找到下一条曲线，结束排序，未被排序的曲线将被舍弃
                    break;
                }

                if (sortedList.Contains(nextCurve))
                {
                    // 检测到循环引用
                    throw new InvalidOperationException("Circular reference detected.");
                }

                // 将这条曲线加入排序列表，并从待查找曲线列表中移除，继续查找下一条曲线
                sortedList.Add(nextCurve);
                remainingCurves.Remove(nextCurve);
                current = nextCurve;
            }

            CubicBeziers.Clear();
            CubicBeziers.AddRange(sortedList);
        }

        /// <summary>
        /// 根据传入的时间，返回瞬时速度
        /// </summary>
        /// <param name="time">相对于判定点时间（ms），必须为负数或 0</param>
        /// <returns>贝塞尔曲线上的瞬时速度，如果时间小于最前一个曲线，返回最前一个曲线的速度</returns>
        /// <exception cref="ArgumentOutOfRangeException">time 大于 0 时将会抛出异常</exception>
        public float GetSpeed(int time)
        {
            Sort();

            if (time > 0)
            {
                // 过线后返回判定时瞬时速度
                return CubicBeziers[0].P0.Value;
            }

            if (time <= CubicBeziers[CubicBeziers.Count - 1].P3.Time)
            {
                // 超出最左侧曲线的时间时，直接返回最左侧曲线 P3 的速度
                return CubicBeziers[CubicBeziers.Count - 1].P3.Value;
            }

            foreach (CubicBezierCurve cubic in CubicBeziers)
            {
                if (cubic.P3.Time <= time && time <= cubic.P0.Time)
                {
                    // time 落在这个曲线里
                    return cubic.Calculate(time);
                }
            }

            throw new ArgumentException("Time not in any curve range."); // But how?
        }
    }

    /// <summary>
    /// 贝塞尔曲线区间
    /// </summary>
    /// <remarks>一个不“折返”的三次贝塞尔曲线</remarks>
    public class CubicBezierCurve
    {
        /// <summary>
        /// 二分法求值时的最大迭代次数，达到此次数即停止迭代
        /// </summary>
        private const int MaxIter = 100;

        /// <summary>
        /// 二分法求值时的精度，达到此精度即停止迭代
        /// </summary>
        private const float Epsilon = 0.0001f;

        // 所有 Time 必须小于等于 0（即距离判定点的时间，单位 ms），且通过以下限制来确保整条曲线从右往左绘制，不出现“折返”（即每个 x 总是有且仅有一个对应的 y 值）
        // P3.Time < P0.Time
        // P3.Time < P1.Time <= P0.Time
        // P3.Time <= P2.Time < P1.Time
        // TODO: 优化限制条件：目前通过 P1 P2 的 Time 范围来确保一定不会出现折返（更严格），但排除了某些控制点出界但未发生折返的情况，后续可以通过对三次曲线求导为二次斜率来限制折返（更宽松但依然不会有折返）
        public BezierControlPoint P0; // 最右侧的位置点，如果是第一条曲线，P0.Time=0
        public BezierControlPoint P1; // 控制点
        public BezierControlPoint P2; // 控制点
        public BezierControlPoint P3; // 最左侧的位置点，也是下一组曲线的 P0

        /// <summary>
        /// 在单个曲线区间上计算时间对应的速度
        /// </summary>
        /// <remarks>如非必要，请调用 BezierCurve 类的 GetSpeed()</remarks>
        public float Calculate(int time)
        {
            if (time > 0)
            {
                throw new ArgumentOutOfRangeException(nameof(time), "Time must be negative or zero.");
            }

            // 通过二分法求 time（即 x） 对应的近似 t 值
            // 关于 t 值的更多信息，请自行查找贝塞尔曲线的相关知识
            float targetX = time;
            float tLow = 0f, tHigh = 1f;

            float t = 0.5f;
            for (int i = 0; i < MaxIter; i++)
            {
                t = (tLow + tHigh) / 2;
                float xMid = CalculateX(t);

                if (Mathf.Abs(xMid - targetX) < Epsilon)
                {
                    break;
                }

                if (xMid > targetX)
                {
                    tLow = t;
                }
                else
                {
                    tHigh = t;
                }
            }

            // 通过 t 值计算 y 值
            return CalculateY(t);
        }

        private float CalculateX(float t)
        {
            float u = 1f - t;
            return u * u * u * P0.Time + 3 * u * u * t * P1.Time + 3 * u * t * t * P2.Time + t * t * t * P3.Time;
        }

        private float CalculateY(float t)
        {
            float u = 1f - t;
            return u * u * u * P0.Value + 3 * u * u * t * P1.Value + 3 * u * t * t * P2.Value +
                   t * t * t * P3.Value;
        }
    }

    /// <summary>
    /// 贝塞尔控制点
    /// </summary>
    /// <remarks>
    /// 在三次贝塞尔曲线中，曲线会穿过首尾控制点，另外两个点将控制曲线形变
    /// </remarks>
    public class BezierControlPoint
    {
        public int Time;
        public float Value;
    }
}
