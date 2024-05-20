using System;
using System.Diagnostics;

namespace CyanStars.Gameplay.MusicGame
{
    public class StopWatchTimer : IMusicGameTimer
    {
        private Stopwatch sw;

        public GameTimeSpan Time { get; private set; }

        private GameTimeSpan rawTime;

        private double delay;
        private int millisecondsDelay;

        public MusicGameTimerState State
        {
            get
            {
                if (sw.IsRunning)
                    return MusicGameTimerState.Running;
                else if (Time.TotalSeconds > 0)
                    return MusicGameTimerState.Pause;
                else
                    return MusicGameTimerState.None;
            }
        }

        public StopWatchTimer()
        {
            sw = new Stopwatch();
        }

        public void Reset()
        {
            if (State == MusicGameTimerState.Running)
                throw new InvalidOperationException("Can't reset game timer when it's running.");

            sw.Reset();
            Time = default;
            delay = 0;
            millisecondsDelay = 0;
        }

        public bool Start(MusicGameTimeData data, double delay = 0)
        {
            if (!sw.IsRunning)
            {
                this.delay += delay;
                this.millisecondsDelay = (int)(this.delay * 1000);
                sw.Start();
                return true;
            }
            return false;
        }

        public bool Pause(MusicGameTimeData data)
        {
            bool ret = sw.IsRunning;
            sw.Stop();
            return ret;
        }

        public bool UnPause(MusicGameTimeData data)
        {
            if (State != MusicGameTimerState.Pause)
                return false;

            sw.Start();
            return true;
        }

        public void Stop()
        {
            sw.Stop();
            Reset();
        }

        public TimerEvaluateData Evaluate(MusicGameTimeData data)
        {
            var state = State;
            if (state == MusicGameTimerState.None)
                return default;

            var elapsed = sw.Elapsed;

            if (state == MusicGameTimerState.Pause || elapsed.TotalMilliseconds - rawTime.TotalMilliseconds < 8)
            {
                return new TimerEvaluateData
                {
                    Elapsed = Time,
                    LastElapsed = Time
                };
            }

            var newRawTime = GameTimeSpan.FromTimeSpan(elapsed);
            var lastTime = Time;
            var newTime = new GameTimeSpan(newRawTime.TotalSeconds - delay, newRawTime.TotalMilliseconds - millisecondsDelay);

            rawTime = newRawTime;
            Time = newTime;

            return new TimerEvaluateData
            {
                Elapsed = newTime,
                LastElapsed = lastTime
            };
        }
    }
}
