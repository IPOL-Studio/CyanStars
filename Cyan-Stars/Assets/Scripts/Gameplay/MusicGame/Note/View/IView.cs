namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 视图层接口
    /// </summary>
    public interface IView
    {
        void OnUpdate(float viewDistance);
        void DestroySelf(bool autoMove = true);
        void CreateEffectObj(float w);
        void DestroyEffectObj();
    }
}
