#nullable enable

using System;
using UnityEngine;

namespace CyanStars.Framework.SmoothDspTimer
{
    public class SmoothDspTimer
    {
        /* DspTimerManager 计算逻辑：
         * Manager 时间以 AudioSetting.dspTime 为准
         * 但是 dspTime 不保证每帧更新，而下游需要每帧获取时间变化量
         * 因此在 dspTime 没有刷新的间隔内，暂且采用 Time.unscaledDeltaTime 作为差值，牺牲一些精度来换取每帧更新
         * 一旦 dspTime 发生更新，计算当前累计时间和 dspTime 的误差，并在后续的每帧更新中逐步抹平这些误差时间
         */

        /// <summary>
        /// 阻尼系数，[0,1]
        /// </summary>
        /// <remarks>
        /// 每帧抹除当前积累误差的几倍，就像半衰期一样
        /// </remarks>
        private const double ErrorTimeDamping = 0.1; // TODO: 改为每秒抹除多少误差，以排除不同帧率的影响

        /// <summary>
        /// 最大误差时间 (s)，[0,+∞)
        /// </summary>
        /// <remarks>
        /// 若 currentTime 延后超过此误差值，则强制跳转到 dspTime；
        /// 若 currentTime 提前超过此误差值，则停止 currentTime 更新直到误差小于范围
        /// </remarks>
        private const double MaxErrorTime = 1;

        /// <summary>
        /// 此 manager 实例记录的，经过矫正后的时间 (s)
        /// </summary>
        /// <remarks>
        /// 每帧更新，用于在两个 dspTime 的间隔之间平滑差值；保证时间正向更新，不会在 dspTime 变化时发生时光倒流
        /// </remarks>
        private double currentTime = AudioSettings.dspTime;

        /// <summary>
        /// dspTime 上次更新时的值 (s)
        /// </summary>
        /// <remarks>
        /// dspTime 更准确，但不保证每帧更新
        /// </remarks>
        private double previousDspTime = AudioSettings.dspTime;

        /// <summary>
        /// currentTime 与 previousDspTime 之间的误差时间，currentTime 提前时为负数 (s)
        /// </summary>
        /// <remarks>用于纠正 currentTime 的误差</remarks>
        private double errorTime = 0;

        /// <summary>
        /// 由调用者每帧驱动，返回平滑后的 deltaDspTime
        /// </summary>
        public double OnUpdate()
        {
            double smoothDeltaDspTime;
            if (Math.Abs(AudioSettings.dspTime - previousDspTime) > 0.000001)
            {
                // dspTime 发生更新
                previousDspTime = AudioSettings.dspTime;
                errorTime = previousDspTime - currentTime;
            }

            if (errorTime > MaxErrorTime)
            {
                // 如果 managerTime 延后较大，则强制向前跳转以纠正误差
                Debug.LogWarning($"{nameof(currentTime)} 误差达到 {errorTime}s，强制跳转时间到 {AudioSettings.dspTime}s。");
                smoothDeltaDspTime = AudioSettings.dspTime - currentTime;
                currentTime = AudioSettings.dspTime;
                errorTime = 0;
            }
            else if (-errorTime > MaxErrorTime)
            {
                // 如果 managerTime 提前较大，则停止此帧更新以纠正误差
                Debug.LogWarning($"{nameof(currentTime)} 误差达到 {errorTime}s，本帧停止更新。");
                smoothDeltaDspTime = 0;
            }
            else
            {
                double correctionTime = errorTime * ErrorTimeDamping;
                correctionTime = Math.Max(correctionTime, -Time.unscaledDeltaTime); // 修正之后的 deltaTime 必须为非负数
                smoothDeltaDspTime = Time.unscaledDeltaTime + correctionTime;
                errorTime -= correctionTime;
                currentTime += smoothDeltaDspTime;
            }

            return smoothDeltaDspTime;
        }
    }
}
