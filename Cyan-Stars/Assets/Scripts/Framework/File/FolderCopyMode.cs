#nullable enable

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件夹复制模式
    /// </summary>
    internal enum FolderCopyMode
    {
        /// <summary>
        /// 目标已存在时失败
        /// </summary>
        FailIfExists,

        /// <summary>
        /// 目标已存在时先删除再复制
        /// </summary>
        ReplaceExisting,

        /// <summary>
        /// 目标已存在时追加序号后缀
        /// </summary>
        UniqueName,
    }
}
