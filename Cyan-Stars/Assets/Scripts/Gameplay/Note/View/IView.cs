namespace CyanStars.Gameplay.Note
{
    /// <summary>
    /// 视图层接口
    /// </summary>
    public interface IView
    {

        void OnUpdate(float deltaTime);

        void DestroySelf(bool autoMove = true);
        void CreateEffectObj(float w);
        void DestroyEffectObj();
    }
}