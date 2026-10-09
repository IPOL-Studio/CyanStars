namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件句柄的生命周期状态
    /// </summary>
    public enum FileHandleState
    {
        /// <summary>句柄可用，<see cref="FileHandle.ReadablePath"/> 指向的内容可读</summary>
        Available,

        /// <summary>已经释放，句柄上的缓存文件已删除，不可再使用</summary>
        Released
    }
}
