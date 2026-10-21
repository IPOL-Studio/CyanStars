#nullable enable

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件夹传输方向
    /// </summary>
    public enum FolderTransferDirection
    {
        /// <summary>
        /// 外部位置到应用自有目录
        /// </summary>
        Import,

        /// <summary>
        /// 应用自有目录到外部位置
        /// </summary>
        Export,
    }
}
