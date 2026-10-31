#nullable enable

using System;

namespace CyanStars.Framework.File.Platform
{
    /// <summary>
    /// 当前平台可用的常用文件夹
    /// </summary>
    public sealed class PlatformFolder
    {
        /// <summary>
        /// 文件选择对话框中的显示名称
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 普通绝对路径或平台外部存储 URI
        /// </summary>
        public string Path { get; }


        /// <summary>
        /// 创建一个常用文件夹描述
        /// </summary>
        /// <param name="displayName">文件选择对话框中的显示名称</param>
        /// <param name="path">普通绝对路径或平台外部存储 URI</param>
        /// <exception cref="ArgumentException">显示名称或路径为空时抛出</exception>
        public PlatformFolder(string displayName, string path)
        {
            if (string.IsNullOrEmpty(displayName))
                throw new ArgumentException("常用文件夹显示名称为空", nameof(displayName));

            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("常用文件夹路径为空", nameof(path));

            DisplayName = displayName;
            Path = path;
        }
    }
}
