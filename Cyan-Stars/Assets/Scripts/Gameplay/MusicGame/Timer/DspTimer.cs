using System;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class DspTimer : IMusicGameTimer
    {
        private readonly bool UseGameTimeCompensation;

        private double delay;

        private double lastDspTime;
        private double pauseDspTime;

        private double finalDspTime;
        private double nonFixedTime;

        public double Time { get; private set; }
        public int Milliseconds => (int)(Time * 1000);

        public MusicGameTimerState State { get; private set; }

        public DspTimer(bool useGameTimeCompensation = false)
        {
            UseGameTimeCompensation = useGameTimeCompensation;
        }

        public bool Pause()
        {
            if (State != MusicGameTimerState.Running)
                return false;

            pauseDspTime = AudioSettings.dspTime;
            State = MusicGameTimerState.Pause;
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
            Time = 0;
            State = MusicGameTimerState.None;
        }

        public void Stop()
        {
            Pause();
            Reset();
        }

        public bool Start(double delay = 0)
        {
            if (State == MusicGameTimerState.None)
            {
                lastDspTime = AudioSettings.dspTime;
                this.delay = delay;
            }
            else if (State == MusicGameTimerState.Pause)
            {
                this.delay += delay;
                double dspTime = AudioSettings.dspTime;
                double dt = pauseDspTime - lastDspTime;
                lastDspTime = dspTime;
                finalDspTime += dt;
            }
            else
            {
                return false;
            }

            State = MusicGameTimerState.Running;
            return true;
        }

        public TimerEvaluateData Evaluate()
        {
            if (State == MusicGameTimerState.None)
                return default;

            double dspTime = AudioSettings.dspTime;
            bool isDspTimeChanged = Math.Abs(dspTime - lastDspTime) > 0.00001;
            if (State == MusicGameTimerState.Pause || (!UseGameTimeCompensation && !isDspTimeChanged))
            {
                double time = Time;
                int ms = Milliseconds;
                return new TimerEvaluateData
                {
                    TotalSeconds = time, LastTotalSeconds = time,
                    TotalMilliseconds = ms, LastTotalMilliseconds = ms
                };
            }

            double lastTime = Time;
            int lastMilliseconds = Milliseconds;

            if (isDspTimeChanged)
            {
                finalDspTime += dspTime - lastDspTime;
                nonFixedTime = finalDspTime;
                lastDspTime = dspTime;
            }
            else
            {
                nonFixedTime += UnityEngine.Time.unscaledDeltaTime;
            }

            Time = nonFixedTime - delay;

            return new TimerEvaluateData
            {
                TotalSeconds = Time, LastTotalSeconds = lastTime,
                TotalMilliseconds = Milliseconds, LastTotalMilliseconds = lastMilliseconds
            };
        }
    }
}
