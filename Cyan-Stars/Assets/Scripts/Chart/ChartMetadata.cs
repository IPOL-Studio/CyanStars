using Newtonsoft.Json;

namespace CyanStars.Chart
{
    public sealed class ChartMetaData
    {
        /// <summary>
        /// 对应的谱面文件
        /// </summary>
        public string FilePath;

        /// <summary>谱面难度</summary>
        /// <remarks>为空时只在制谱器内可见，游戏内不加载</remarks>
        public ChartDifficulty? Difficulty;

        /// <summary>
        /// 提供的谱面哈希，用于和缓存哈希对比并展示历史成绩
        /// </summary>
        /// <remarks>制谱器保存、首次导入谱包、音游流程加载谱面时会重算一次这里的哈希，首次导入和加载谱面时还会修改缓存的哈希</remarks>
        public string ChartHash;

        /// <summary>
        /// 构造函数
        /// </summary>
        [JsonConstructor]
        public ChartMetaData(string filePath, ChartDifficulty? difficulty = null, string chartHash = null)
        {
            FilePath = filePath;
            Difficulty = difficulty;
            ChartHash = chartHash;
        }
    }
}
