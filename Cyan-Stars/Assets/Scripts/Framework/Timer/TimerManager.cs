using System;
using System.Collections.Generic;
using CyanStars.Framework.Utils;

namespace CyanStars.Framework.Timer
{
    /// <summary>
    /// 定时管理器
    /// </summary>
    public class TimerManager : BaseManager
    {
        /// <inheritdoc />
        public override int Priority { get; }

        private readonly Dictionary<Type, ITimer> TimerDict = new Dictionary<Type, ITimer>();

        public UpdateTimer UpdateTimer { get; private set; }

        public override void OnInit()
        {
            UpdateTimer = new UpdateTimer();

            AddTimer(UpdateTimer);
            AddTimer(new UpdateUntilTimer());
            AddTimer(new IntervalTimer());
        }

        public override void OnUpdate(float deltaTime)
        {
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
