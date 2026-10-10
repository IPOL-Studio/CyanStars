#nullable enable

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件类型过滤器：按显示名和后缀筛选文件对话框里的可选文件
    /// </summary>
    /// <remarks>框架自有的过滤器类型，业务侧使用它描述需求，由 <see cref="FileManager"/> 转换为文件对话框插件的类型</remarks>
    public readonly struct FileTypeFilter
    {
        /// <summary>
        /// 过滤器在对话框里的显示名
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 允许的文件后缀，带点号，如 <c>.ogg</c>
        /// </summary>
        public string[] Extensions { get; }


        public FileTypeFilter(string displayName, params string[] extensions)
        {
            DisplayName = displayName;
            Extensions = extensions;
        }
    }
}
