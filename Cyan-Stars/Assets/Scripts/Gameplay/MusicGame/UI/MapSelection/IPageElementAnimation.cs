namespace CyanStars.Gameplay.MusicGame
{
    public interface IPageElementAnimation
    {
        void OnEnter(MapSelectionPageChangeArgs args);
        void OnExit(MapSelectionPageChangeArgs args);
    }
}
