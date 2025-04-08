namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 多演唱版本曲目数据
    /// </summary>
    public class MusicVersionData
    {
        /// <summary>
        /// 曲目文件相对路径
        /// </summary>
        public string MusicFilePath;

        /// <summary>
        /// 在播放前添加多长时间的空白（ms）
        /// </summary>
        /// <remarks>
        /// 即谱面延后时间
        /// </remarks>
        public int Offset;
    }
}
