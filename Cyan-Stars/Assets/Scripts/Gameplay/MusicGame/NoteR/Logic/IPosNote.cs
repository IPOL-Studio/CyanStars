namespace CyanStars.Gameplay.MusicGame
{
    public interface IPosNote
    {
        // 除 Break 以外，其他四种 Note 均有常规的 Pos，以 Note 左端点计算，介于 [0, 0.8]
        public float Pos { get; set; }
    }
}
