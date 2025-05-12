namespace CyanStars.Gameplay.Chart
{
    public sealed class ChartMetadata
    {
        /// <summary>
        /// 对应的谱面文件
        /// </summary>
        public string FilePath;

        /// <summary>谱面难度</summary>
        /// <remarks>为空时只在编辑器内可见，游戏内不加载；其他难度最多在一个谱包中各有0或1个</remarks>
        public ChartDifficulty? Difficulty;

        /// <summary>谱面定数</summary>
        /// <remarks>内置谱面此值应该是一个 [1, 20] 之间的整数，社区谱随意</remarks>
        public string Level;
    }
}
