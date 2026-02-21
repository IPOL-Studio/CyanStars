using System;
using System.Collections.Generic;
using CyanStars.Chart.BezierCurve;
using UnityEngine;

namespace CyanStars.Chart
{
    /// <summary>
    /// 加载到游戏内的速度组
    /// </summary>
    public class SpeedTemplate
    {
        private SpeedTemplateData SpeedTemplateData;

        /// <summary>
        /// 缓存的速度列表
        /// </summary>
        /// <remarks>列表从 0 时间差开始，每隔 SampleInterval 个时间向前采样，计算提前一段时间的速度</remarks>
        private readonly List<float> SpeedList;

        /// <summary>
        /// 缓存的距离（视图层时间）列表
        /// </summary>
        /// <remarks>列表从 0 时间差开始，每隔 SampleInterval 个时间向前采样，计算提前一段时间的距离</remarks>
        private readonly List<float> DisplacementList;

        /// <summary>
        /// 缓存的最远端贝塞尔点的最终位移
        /// </summary>
        private readonly double FinalDisplacement;


        /// <summary>
        /// 构造实例并烘焙速度和距离采样点
        /// </summary>
        public SpeedTemplate(SpeedTemplateData speedTemplateData, float playerSpeed = 1f) // TODO: 外部传入玩家速度
        {
            SpeedTemplateData = speedTemplateData;

            SpeedTemplateHelper.Bake(
                speedTemplateData,
                playerSpeed,
                out SpeedList,
                out DisplacementList
            );
            FinalDisplacement = SpeedTemplateHelper.GetFinalDisplacement(speedTemplateData, playerSpeed);
        }

        /// <summary>
        /// 根据相对时间计算音符当前的位移
        /// </summary>
        public float GetDistance(float msTime)
        {
            msTime = msTime * -1; // TODO: 历史遗留问题，逻辑层时间提前时为负数

            if (msTime <= 0)
            {
                // 音符已经超过判定时间，根据 [0] 贝塞尔点线性计算位移
                var speed = SpeedTemplateData.BezierCurves[0].PositionPoint.Value;
                return speed * msTime / 1000f;
            }
            else if (SpeedTemplateData.BezierCurves[^1].PositionPoint.Value <= msTime)
            {
                // 音符还未达到判定时间，且在最远端贝塞尔点之外
                // 线性计算超出贝塞尔点时间部分的位移，然后加上整段位移
                var segmentMsTime = msTime - SpeedTemplateData.BezierCurves[^1].PositionPoint.MsTime;
                var speed = SpeedTemplateData.BezierCurves[^1].PositionPoint.Value;
                return (float)FinalDisplacement + speed * segmentMsTime / 1000f;
            }
            else
            {
                // 音符落在曲线上，直接根据采样结果返回
                // 将音符对齐到最近的采样点上
                const float halfSampleIntervalMsTime = SpeedTemplateHelper.SampleIntervalMsTime / 2f;
                int sampleIndex = (int)((msTime + halfSampleIntervalMsTime) / SpeedTemplateHelper.SampleIntervalMsTime);
                sampleIndex = Math.Min(sampleIndex, DisplacementList.Count - 1);
                return DisplacementList[sampleIndex];
            }
        }
    }
}
