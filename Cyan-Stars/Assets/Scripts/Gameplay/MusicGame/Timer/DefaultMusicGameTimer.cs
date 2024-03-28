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

        public GameTimeSpan Time { get; private set; }
        public MusicGameTimerState State { get; private set; }

        public DefaultMusicGameTimer()
        {
            timers = new IMusicGameTimer[TimerCount]
            {
                new RealtimeSliceStartupTimer(),
                new DspTimer(DspTimer.GameTimeCompensationMode.Realtime),
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

        public bool Start(MusicGameTimeData data, double delay = 0)
        {
            if (State == MusicGameTimerState.Running)
                return false;

            for (int i = 0; i < TimerCount; i++)
            {
                timers[i].Start(data, delay);
            }

            State = MusicGameTimerState.Running;
            return true;
        }

        public bool Pause(MusicGameTimeData data)
        {
            if (State != MusicGameTimerState.Running)
                return false;

            for (int i = 0; i < TimerCount; i++)
            {
                timers[i].Pause(data);
            }

            State = MusicGameTimerState.Pause;
            return true;
        }

        public bool UnPause(MusicGameTimeData data)
        {
            if (State != MusicGameTimerState.Pause)
                return false;

            for (int i = 0; i < TimerCount; i++)
            {
                timers[i].UnPause(data);
            }

            State = MusicGameTimerState.Running;
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

        public TimerEvaluateData Evaluate(MusicGameTimeData data)
        {
            if (State == MusicGameTimerState.None)
                return default;

            if (State == MusicGameTimerState.Pause || !UpdateAllTimer(data))
            {
                return new TimerEvaluateData
                {
                    Elapsed = Time,
                    LastElapsed = Time
                };
            }

            int timeCount = 0;
            for (int i = 0; i < TimerCount; i++)
            {
                timeCount += evaluateDataKeepCounts[i];
            }

            var lastTime = Time;

            double seconds = 0;
            int milliseconds = 0;

            for (int i = 0; i < TimerCount; i++)
            {
                double rate = 1.0 - (double)evaluateDataKeepCounts[i] / timeCount;
                // UnityEngine.Debug.Log($"Timer[{i}] keepCount: {evaluateDataKeepCounts[i]}, rate: {rate}.");
                seconds += tempEvaluateDatas[i].Elapsed.TotalSeconds * rate;
                milliseconds += (int)(tempEvaluateDatas[i].Elapsed.TotalMilliseconds * rate);
            }

            var newTime = new GameTimeSpan(seconds, milliseconds);
            Time = newTime;

            for (int i = 0; i < TimerCount; i++)
            {
                lastEvaluateDatas[i] = tempEvaluateDatas[i];
            }

            return new TimerEvaluateData
            {
                Elapsed = newTime,
                LastElapsed = lastTime
            };
        }

        /// <returns> Is any timer value has been valid update </returns>
        private bool UpdateAllTimer(MusicGameTimeData data)
        {
            for (int i = 0; i < TimerCount; i++)
            {
                tempEvaluateDatas[i] = timers[i].Evaluate(data);
                //UnityEngine.Debug.Log($"Timer[{i}]: {tempEvaluateDatas[i].Elapsed.TotalSeconds}.");
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
            return newData.Elapsed.TotalMilliseconds > oldData.Elapsed.TotalMilliseconds;
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

            Time = default;
            State = MusicGameTimerState.None;
        }
    }
}
