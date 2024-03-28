using System;

namespace CyanStars.Gameplay.MusicGame
{
    public class RealtimeSliceStartupTimer : IMusicGameTimer
    {
        private double delay;

        private double startRealtime;
        private double lastPauseRealtime;
        private double lastEvaluationRealtime;
        private double totalPauseTime;

        public GameTimeSpan Time { get; private set; }

        public MusicGameTimerState State { get; private set; }

        public RealtimeSliceStartupTimer()
        {
            Reset();
        }

        public bool Pause(MusicGameTimeData data)
        {
            if (State != MusicGameTimerState.Running)
                return false;

            lastPauseRealtime = data.RealtimeSinceStartup;
            State = MusicGameTimerState.Pause;
            return true;
        }

        public bool UnPause(MusicGameTimeData data)
        {
            if (State != MusicGameTimerState.Pause)
                return false;

            totalPauseTime += data.RealtimeSinceStartup - lastPauseRealtime;
            lastPauseRealtime = -double.NaN;
            State = MusicGameTimerState.Running;
            return true;
        }

        public void Reset()
        {
            if (State == MusicGameTimerState.Running)
                throw new InvalidOperationException("Can't reset game timer when it's running.");

            startRealtime = -double.NaN;
            lastPauseRealtime = -double.NaN;
            lastEvaluationRealtime = -double.NaN;
            delay = 0;
            totalPauseTime = 0;
            Time = default;
            State = MusicGameTimerState.None;
        }

        public void Stop()
        {
            State = MusicGameTimerState.None;
            Reset();
        }

        public bool Start(MusicGameTimeData data, double delay = 0)
        {
            if (State != MusicGameTimerState.None)
                return false;

            startRealtime = data.RealtimeSinceStartup;
            lastEvaluationRealtime = data.RealtimeSinceStartup;
            totalPauseTime = 0;
            this.delay = delay;
            State = MusicGameTimerState.Running;
            return true;
        }

        public TimerEvaluateData Evaluate(MusicGameTimeData data)
        {
            if (State == MusicGameTimerState.None)
                return default;

            bool isRealtimeChanged = data.RealtimeSinceStartup - lastEvaluationRealtime > 0.00001;
            if (State == MusicGameTimerState.Pause || !isRealtimeChanged)
            {
                return new TimerEvaluateData
                {
                    Elapsed = Time,
                    LastElapsed = Time,
                };
            }

            var lastTime = Time;

            lastEvaluationRealtime = data.RealtimeSinceStartup;
            var newTime = GameTimeSpan.FromSeconds(data.RealtimeSinceStartup - startRealtime - totalPauseTime - delay);
            Time = newTime;

            return new TimerEvaluateData
            {
                Elapsed = newTime,
                LastElapsed = lastTime,
            };
        }
    }
}
