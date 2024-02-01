using System;
using System.Diagnostics;

namespace CyanStars.Gameplay.MusicGame
{
    public class StopWatchTimer : IMusicGameTimer
    {
        private Stopwatch sw;

        public double Time { get; private set; }
        public int Milliseconds { get; private set; }

        private double delay;
        private int millisecondsDelay;

        public MusicGameTimerState State
        {
            get
            {
                if (sw.IsRunning)
                    return MusicGameTimerState.Running;
                else if (Time > 0)
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
            Time = 0;
            Milliseconds = 0;
            delay = 0;
            millisecondsDelay = 0;
        }

        public bool Start(double delay = 0)
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

        public bool Pause()
        {
            bool ret = sw.IsRunning;
            sw.Stop();
            return ret;
        }

        public void Stop()
        {
            Pause();
            Reset();
        }

        public TimerEvaluateData Evaluate()
        {
            var state = State;
            if (state == MusicGameTimerState.None)
                return default;

            double time;
            double lastTime;
            int milliseconds;
            int lastMilliseconds;

            if (state == MusicGameTimerState.Pause || (int)sw.ElapsedMilliseconds - Milliseconds < 8)
            {
                time = Time - delay;
                milliseconds = Milliseconds - millisecondsDelay;
                lastTime = time;
                lastMilliseconds = milliseconds;
            }
            else
            {
                lastTime = Time;
                lastMilliseconds = Milliseconds;

                var elapsed = sw.Elapsed;
                Time = elapsed.TotalSeconds;
                Milliseconds = (int)elapsed.TotalMilliseconds;

                time = Time - delay;
                milliseconds = Milliseconds - millisecondsDelay;
            }

            return new TimerEvaluateData
            {
                TotalSeconds = time, LastTotalSeconds = lastTime,
                TotalMilliseconds = milliseconds, LastTotalMilliseconds = lastMilliseconds
            };
        }
    }
}
