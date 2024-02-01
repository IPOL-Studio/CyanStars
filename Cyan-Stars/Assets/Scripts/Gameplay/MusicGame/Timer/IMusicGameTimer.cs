namespace CyanStars.Gameplay.MusicGame
{
    public interface IMusicGameTimer
    {
        double Time { get; }
        int Milliseconds { get; }
        MusicGameTimerState State { get; }

        void Reset();
        bool Start(double delay = 0);
        bool Pause();
        void Stop();
        TimerEvaluateData Evaluate();
    }
}
