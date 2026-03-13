using System;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Framework.Timer
{
    /// <summary>
    /// 定时管理器
    /// </summary>
    public class DspTimerManager : BaseManager
    {
        /// <inheritdoc />
        public override int Priority { get; }

        private readonly Dictionary<Type, ITimer> TimerDict = new Dictionary<Type, ITimer>();

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
        private const double ErrorTimeDamping = 0.1;

        /// <summary>
        /// 最大误差时间 (s)，[0,+∞)
        /// </summary>
        /// <remarks>
        /// 若 managerTime 延后超过此误差值，则强制跳转到 dspTime；
        /// 若 managerTime 提前超过此误差值，则停止 managerTime 更新直到误差小于范围
        /// </remarks>
        private const double MaxErrorTime = 1;

        /// <summary>
        /// 此 manager 实例记录的，经过矫正后的时间 (s)
        /// </summary>
        /// <remarks>
        /// 每帧更新，用于在两个 dspTime 的间隔之间平滑差值；保证时间正向更新，不会在 dspTime 变化时发生时光倒流
        /// </remarks>
        private double managerTime;

        /// <summary>
        /// dspTime 上次更新时的值 (s)
        /// </summary>
        /// <remarks>
        /// dspTime 更准确，但不保证每帧更新
        /// </remarks>
        private double previousDspTime;

        /// <summary>
        /// managerTime 与 previousDspTime 之间的误差时间，managerTime 提前时为负数 (s)
        /// </summary>
        /// <remarks>用于纠正 managerTime 的误差</remarks>
        private double errorTime = 0;


        public UpdateTimer UpdateTimer { get; private set; }

        public override void OnInit()
        {
            managerTime = previousDspTime = AudioSettings.dspTime;

            UpdateTimer = new UpdateTimer();
            AddTimer(UpdateTimer);
            AddTimer(new UpdateUntilTimer());
            AddTimer(new IntervalTimer());
        }

        public override void OnUpdate(float _)
        {
            if (Math.Abs(AudioSettings.dspTime - previousDspTime) > 0.000001)
            {
                // dspTime 发生更新
                previousDspTime = AudioSettings.dspTime;
                errorTime = previousDspTime - managerTime;

                // 如果 managerTime 延后较大，则强制向前跳转以纠正误差
                if (errorTime > MaxErrorTime)
                {
                    managerTime = AudioSettings.dspTime;
                    errorTime = 0;
                }
            }

            // 如果 managerTime 提前较大，则停止此帧更新以纠正误差
            double deltaTime;
            if (-errorTime > MaxErrorTime)
            {
                deltaTime = 0;
                errorTime += Time.unscaledDeltaTime;
            }
            else
            {
                double correctionTime = errorTime * ErrorTimeDamping;
                correctionTime = Math.Max(correctionTime, -Time.unscaledDeltaTime); // 修正之后的 deltaTime 必须为非负数
                deltaTime = Time.unscaledDeltaTime + correctionTime;
                errorTime -= correctionTime;
                managerTime += deltaTime;
            }

            foreach (var timer in TimerDict.Values)
            {
                timer?.OnUpdate(deltaTime);
            }
        }

        public void AddTimer<T>(T timer) where T : class, ITimer
        {
            if (timer is null)
            {
                throw new NullReferenceException(nameof(timer));
            }

            TimerDict.Add(typeof(T), timer);
        }

        public T GetTimer<T>() where T : class, ITimer
        {
            return TimerDict.GetValueOrDefault(typeof(T)) as T;
        }
    }
}
