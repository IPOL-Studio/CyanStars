namespace CyanStars.Gameplay.MusicGame
{
    public interface IMusicGameTimer
    {
        GameTimeSpan Time { get; }
        MusicGameTimerState State { get; }

        void Reset();
        bool Start(MusicGameTimeData data, double delay = 0);
        bool Pause(MusicGameTimeData data);
        bool UnPause(MusicGameTimeData data);
        void Stop();
        TimerEvaluateData Evaluate(MusicGameTimeData data);
    }
}
