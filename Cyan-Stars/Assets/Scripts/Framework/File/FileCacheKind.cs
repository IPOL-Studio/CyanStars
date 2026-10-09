namespace CyanStars.Framework.File
{
    /// <summary>
    /// 会话临时目录里缓存的用途
    /// </summary>
    public enum FileCacheKind
    {
        /// <summary>跨平台文件缓存，生命周期同游戏进程</summary>
        CrossPlatform,

        /// <summary>制谱器可撤销文件缓存，生命周期为每次进入和退出制谱器期间</summary>
        ChartEditor
    }
}
