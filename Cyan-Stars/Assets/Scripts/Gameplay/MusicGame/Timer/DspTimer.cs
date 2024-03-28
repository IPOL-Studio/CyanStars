using System;
using AudioSettings = UnityEngine.AudioSettings;

namespace CyanStars.Gameplay.MusicGame
{
    public partial class DspTimer : IMusicGameTimer
    {
        private double delay;

        private double lastDspTime;
        private double pauseDspTime;

        private double finalDspTime;
        private double nonFixedTime;

        private ITimeCompensator timeCompensator;
        private readonly GameTimeCompensationMode CompensationMode;

        public GameTimeSpan Time { get; private set; }

        public MusicGameTimerState State { get; private set; }

        public DspTimer(GameTimeCompensationMode compensationMode = GameTimeCompensationMode.None)
        {
            CompensationMode = compensationMode;
        }

        public bool Pause(MusicGameTimeData data)
        {
            if (State != MusicGameTimerState.Running)
                return false;

            pauseDspTime = AudioSettings.dspTime;
            State = MusicGameTimerState.Pause;
            return true;
        }

        public bool UnPause(MusicGameTimeData data)
        {
            if (State != MusicGameTimerState.Pause)
                return false;

            double dspTime = AudioSettings.dspTime;
            double dt = pauseDspTime - lastDspTime;
            lastDspTime = dspTime;
            finalDspTime += dt;
            timeCompensator?.GetCompensationTime(data);
            State = MusicGameTimerState.Running;
            return true;
        }

        public void Reset()
        {
            if (State == MusicGameTimerState.Running)
                throw new InvalidOperationException("Can't reset game timer when it's running.");

            delay = 0;
            lastDspTime = 0;
            pauseDspTime = 0;
            finalDspTime = 0;
            nonFixedTime = 0;
            Time = default;
            State = MusicGameTimerState.None;
        }

        public void Stop()
        {
            Pause(default);
            Reset();
        }

        public bool Start(MusicGameTimeData data, double delay = 0)
        {
            if (State != MusicGameTimerState.None)
                return false;

            lastDspTime = AudioSettings.dspTime;
            this.delay = delay;
            State = MusicGameTimerState.Running;
            ResetTimeCompensator(data);
            return true;
        }

        public TimerEvaluateData Evaluate(MusicGameTimeData data)
        {
            if (State == MusicGameTimerState.None)
                return default;

            double dspTime = AudioSettings.dspTime;
            double compensationTime = timeCompensator?.GetCompensationTime(data) ?? 0;
            bool isDspTimeChanged = Math.Abs(dspTime - lastDspTime) > 0.00001;
            if (State == MusicGameTimerState.Pause || (timeCompensator is null && !isDspTimeChanged))
            {
                return new TimerEvaluateData
                {
                    Elapsed = Time,
                    LastElapsed = Time,
                };
            }

            if (isDspTimeChanged)
            {
                finalDspTime += dspTime - lastDspTime;
                nonFixedTime = finalDspTime;
                lastDspTime = dspTime;
            }
            else
            {
                nonFixedTime += compensationTime;
            }

            var lastTime = Time;
            var newTime = GameTimeSpan.FromSeconds(nonFixedTime - delay);
            Time = newTime;

            return new TimerEvaluateData
            {
                Elapsed = newTime,
                LastElapsed = lastTime,
            };
        }

        private void ResetTimeCompensator(MusicGameTimeData data)
        {
            timeCompensator = CompensationMode switch
            {
                GameTimeCompensationMode.DeltaTime => new DeltaTimeCompensator(),
                GameTimeCompensationMode.Realtime => new RealtimeDeltaCompensator(data),
                _ => null
            };
        }
    }

    partial class DspTimer
    {
        public enum GameTimeCompensationMode
        {
            None,
            DeltaTime,
            Realtime
        }

        private interface ITimeCompensator
        {
            double GetCompensationTime(MusicGameTimeData data);
        }

        private class DeltaTimeCompensator : ITimeCompensator
        {
            public double GetCompensationTime(MusicGameTimeData data) => data.UnscaledDeltaTime;
        }

        private class RealtimeDeltaCompensator : ITimeCompensator
        {
            private double lastRealtime;

            public RealtimeDeltaCompensator(MusicGameTimeData data)
            {
                lastRealtime = data.RealtimeSinceStartup;
            }

            public double GetCompensationTime(MusicGameTimeData data)
            {
                var deltaTime = data.RealtimeSinceStartup - lastRealtime;
                lastRealtime = data.RealtimeSinceStartup;
                return deltaTime;
            }
        }
    }
}
