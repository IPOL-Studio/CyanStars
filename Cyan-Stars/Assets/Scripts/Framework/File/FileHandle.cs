#nullable enable

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件句柄：记录文件来源、当前可读写、将保存的路径地址
    /// </summary>
    /// <remarks>可读写路径始终指向会话缓存里的一份副本，句柄的生命周期与这份副本对齐</remarks>
    public sealed class FileHandle
    {
        /// <summary>
        /// 生命周期状态
        /// </summary>
        public FileHandleState State { get; private set; } = FileHandleState.Available;

        /// <summary>
        /// 来源路径：加载句柄时传入的原始路径；新建的句柄为 null
        /// </summary>
        public string? SourcePath { get; }

        /// <summary>
        /// 来源文件（夹）的名称
        /// </summary>
        /// <remarks>缓存副本可能带防重名后缀，业务侧要用名字作目标路径时以本属性为准</remarks>
        public string EntryName { get; }

        /// <summary>
        /// 当前可读写的路径：会话缓存里的一份副本
        /// </summary>
        public string ReadablePath { get; }

        /// <summary>
        /// 保存时要写到的目标路径（可能是普通绝对路径或安卓 <c>content://</c> 路径）
        /// </summary>
        /// <remarks>
        /// 为 null 表示尚未指定保存位置，本次保存不会写这个文件
        /// </remarks>
        public string? TargetPath { get; private set; }


        /// <summary>
        /// 创建一个可用状态的句柄
        /// </summary>
        /// <param name="sourcePath">来源路径；新建的句柄传 null</param>
        /// <param name="entryName">来源文件（夹）的名称；新建的句柄传请求的文件名</param>
        /// <param name="readablePath">会话缓存里的可读写路径</param>
        internal FileHandle(string? sourcePath, string entryName, string readablePath)
        {
            SourcePath = sourcePath;
            EntryName = entryName;
            ReadablePath = readablePath;
        }

        /// <summary>
        /// 更新目标路径
        /// </summary>
        /// <param name="targetPath">传 null 表示解除目标路径</param>
        /// <remarks>只做状态迁移，可用性与路径合法性校验由 <see cref="FilePortal"/> 统一负责</remarks>
        internal void SetTargetPath(string? targetPath)
        {
            TargetPath = targetPath;
        }

        /// <summary>
        /// 标记为已释放
        /// </summary>
        internal void MarkReleased()
        {
            State = FileHandleState.Released;
            TargetPath = null;
        }
    }
}
