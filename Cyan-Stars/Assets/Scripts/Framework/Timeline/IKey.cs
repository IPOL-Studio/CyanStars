namespace CyanStars.Framework.Timeline
{
    public interface IKey
    {
        IKeyableClip Owner { get; }

        float Time { get; }

        void OnExecute(float currentTime);
    }

    public interface IKey<out TClip> : IKey where TClip : IClip, IKeyableClip
    {
        new TClip Owner { get; }
    }
}
