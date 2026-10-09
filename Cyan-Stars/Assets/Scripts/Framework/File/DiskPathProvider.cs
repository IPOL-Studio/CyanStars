#nullable enable

using System;
using System.IO;
using CyanStars.Utils;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 直接使用磁盘路径的路径提供者
    /// </summary>
    /// <remarks>
    /// <para>用于不需要 SAF 的平台：如编辑器、Windows、macOS、Linux</para>
    /// <para>安卓上不启用 SAF 时也回退到本类</para>
    /// </remarks>
    public sealed class DiskPathProvider : IPathProvider
    {
        public string? GetCommonFolderPath(CommonFolders folder)
        {
            return folder switch
            {
                CommonFolders.Downloads => GetDownloadsFolderPath(),
                CommonFolders.Desktop => GetDesktopFolderPath(),
                _ => throw new ArgumentOutOfRangeException(nameof(folder), folder, null)
            };
        }


        /// <summary>
        /// 取下载文件夹路径
        /// </summary>
        /// <returns>文件夹不存在时返回 null</returns>
        /// <remarks>TODO: 用户迁移过系统下载目录时这里取不到，后续改用系统 KnownFolder 接口查询</remarks>
        private static string? GetDownloadsFolderPath()
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads"
            );
            return Directory.Exists(path) ? PathUtil.Normalize(path) : null;
        }

        /// <summary>
        /// 取桌面路径
        /// </summary>
        /// <returns>文件夹不存在时返回 null</returns>
        private static string? GetDesktopFolderPath()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            return string.IsNullOrEmpty(path) || !Directory.Exists(path) ? null : PathUtil.Normalize(path);
        }
    }
}
