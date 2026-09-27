#nullable enable

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 暂存文件的生命周期状态
    /// </summary>
    /// <remarks>
    /// 只有「副本还在」和「副本已随作用域一起丢弃」两种状态。
    /// 副本是否已写进目标路径不影响状态：<see cref="TempFileStore"/> 保存后映射保持不变
    /// </remarks>
    public enum StagedFileState
    {
        /// <summary>已暂存，暂存副本可用</summary>
        Staged,

        /// <summary>已随缓存区一起丢弃，暂存副本已删除</summary>
        Released
    }
}
