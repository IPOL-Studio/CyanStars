using System;

namespace CyanStars.Gameplay.MusicGame
{
    public class DefaultMusicGameTimer : IMusicGameTimer
    {
        private const int TimerCount = 2;

        private IMusicGameTimer[] timers;
        private TimerEvaluateData[] lastEvaluateDatas;
        private TimerEvaluateData[] tempEvaluateDatas;
        private int[] evaluateDataKeepCounts;

        public double Time { get; private set; }
        public int Milliseconds { get; private set; }
        public MusicGameTimerState State { get; private set; }

        public DefaultMusicGameTimer()
        {
            timers = new IMusicGameTimer[TimerCount]
            {
                new DspTimer(true),
                new StopWatchTimer()
            };

            lastEvaluateDatas = new TimerEvaluateData[TimerCount];
            tempEvaluateDatas = new TimerEvaluateData[TimerCount];
            evaluateDataKeepCounts = new int[TimerCount];
        }

        public void Reset()
        {
            if (State == MusicGameTimerState.Running)
                throw new InvalidOperationException("Can't reset game timer when it's running.");

            Reset_Internal();
        }

        public bool Start(double delay = 0)
        {
            if (State == MusicGameTimerState.Running)
                return false;

            for (int i = 0; i < TimerCount; i++)
            {
                timers[i].Start(delay);
            }

            State = MusicGameTimerState.Running;
            return true;
        }

        public bool Pause()
        {
            if (State != MusicGameTimerState.Running)
                return false;

            for (int i = 0; i < TimerCount; i++)
            {
                timers[i].Pause();
            }

            State = MusicGameTimerState.Pause;
            return true;
        }

        public void Stop()
        {
            if (State == MusicGameTimerState.None)
                return;

            for (int i = 0; i < TimerCount; i++)
            {
                timers[i].Stop();
            }

            Reset_Internal();
        }

        public TimerEvaluateData Evaluate()
        {
            if (State == MusicGameTimerState.None)
                return default;

            if (State == MusicGameTimerState.Pause || !UpdateAllTimer())
            {
                return new TimerEvaluateData
                {
                    TotalSeconds = Time, LastTotalSeconds = Time,
                    TotalMilliseconds = Milliseconds, LastTotalMilliseconds = Milliseconds
                };
            }

            int timeCount = 0;
            for (int i = 0; i < TimerCount; i++)
            {
                timeCount += evaluateDataKeepCounts[i];
            }

            double lastTime = Time;
            int lastMilliseconds = Milliseconds;

            double time = 0;
            int milliseconds = 0;

            for (int i = 0; i < TimerCount; i++)
            {
                double rate = 1.0 - (double)evaluateDataKeepCounts[i] / timeCount;
                // UnityEngine.Debug.Log($"Timer[{i}] keepCount: {evaluateDataKeepCounts[i]}, rate: {rate}.");
                time += tempEvaluateDatas[i].TotalSeconds * rate;
                milliseconds += (int)(tempEvaluateDatas[i].TotalMilliseconds * rate);
            }

            Time = time;
            Milliseconds = milliseconds;

            for (int i = 0; i < TimerCount; i++)
            {
                lastEvaluateDatas[i] = tempEvaluateDatas[i];
            }

            return new TimerEvaluateData
            {
                TotalSeconds = time, LastTotalSeconds = lastTime,
                TotalMilliseconds = milliseconds, LastTotalMilliseconds = lastMilliseconds
            };
        }

        /// <returns> Is any timer value has been valid update </returns>
        private bool UpdateAllTimer()
        {
            for (int i = 0; i < TimerCount; i++)
            {
                tempEvaluateDatas[i] = timers[i].Evaluate();
                // UnityEngine.Debug.Log($"Timer[{i}]: {tempEvaluateDatas[i].TotalSeconds}.");
            }

            bool isAnyUpdated = false;
            for (int i = 0; i < TimerCount; i++)
            {
                if (IsModified(tempEvaluateDatas[i], lastEvaluateDatas[i]))
                {
                    evaluateDataKeepCounts[i] = 1;
                    isAnyUpdated = true;
                }
                else
                {
                    evaluateDataKeepCounts[i]++;
                }
            }

            return isAnyUpdated;
        }

        private bool IsModified(TimerEvaluateData newData, TimerEvaluateData oldData)
        {
            return newData.TotalMilliseconds > oldData.TotalMilliseconds;
        }

        private void Reset_Internal()
        {
            for (int i = 0; i < TimerCount; i++)
            {
                timers[i].Reset();
                lastEvaluateDatas[i] = default;
                tempEvaluateDatas[i] = default;
                evaluateDataKeepCounts[i] = 0;
            }

            Time = 0;
            Milliseconds = 0;
            State = MusicGameTimerState.None;
        }
    }
}
