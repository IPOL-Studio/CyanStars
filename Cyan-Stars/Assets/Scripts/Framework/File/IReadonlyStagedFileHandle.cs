#nullable enable

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 暂存文件的只读句柄
    /// </summary>
    /// <remarks>
    /// <para>读取和修改都针对 <see cref="StagedFilePath"/> 这份暂存副本；改目标路径、丢弃暂存副本等写操作在
    /// <see cref="TempFileStore"/> 上</para>
    /// <para>业务代码只应依赖本接口</para>
    /// </remarks>
    public interface IReadonlyStagedFileHandle
    {
        /// <summary>
        /// 暂存副本的绝对路径
        /// </summary>
        /// <remarks>只要 <see cref="State"/> 是 <see cref="StagedFileState.Staged"/>，这个路径就一定能读到内容</remarks>
        public string StagedFilePath { get; }

        /// <summary>
        /// 副本来源的外部文件路径
        /// </summary>
        public string OriginFilePath { get; }

        /// <summary>
        /// 保存到应用数据时要写到的绝对路径（含后缀名）
        /// </summary>
        /// <remarks>为 null 表示尚未指定写入位置，本次保存不会写这个文件</remarks>
        public string? TargetFilePath { get; }

        /// <summary>
        /// 生命周期状态
        /// </summary>
        public StagedFileState State { get; }
    }
}
