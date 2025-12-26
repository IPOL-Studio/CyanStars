namespace CyanStars.Chart
{
    public sealed class ChartMetaData
    {
        /// <summary>
        /// 对应的谱面文件
        /// </summary>
        public string FilePath;

        /// <summary>谱面难度</summary>
        /// <remarks>为空时只在制谱器内可见，游戏内不加载；其他难度最多在一个谱包中各有0或1个</remarks>
        public ChartDifficulty? Difficulty;

        /// <summary>谱面定数</summary>
        /// <remarks>内置谱面此值应该是一个 [1, 20] 之间的整数，社区谱随意</remarks>
        public string Level;

        /// <summary>
        /// 提供的谱面哈希，用于和缓存哈希对比并展示历史成绩
        /// </summary>
        /// <remarks>制谱器保存、首次导入谱包、音游流程加载谱面时会重算一次这里的哈希，首次导入和加载谱面时还会修改缓存的哈希</remarks>
        public string ChartHash;

        public ChartMetaData(string filePath, ChartDifficulty? difficulty = null, string level = null,
            string chartHash = null)
        {
            FilePath = filePath;
            Difficulty = difficulty;
            Level = level ?? "";
            ChartHash = chartHash;
        }
    }
}
