namespace CyanStars.Framework.File
{
    /// <summary>
    /// 暂存文件的生命周期状态
    /// </summary>
    public enum StagedFileState
    {
        /// <summary>已暂存，暂存副本可用</summary>
        Staged,

        /// <summary>已随缓存区一起丢弃，暂存副本已删除</summary>
        Released
    }
}
