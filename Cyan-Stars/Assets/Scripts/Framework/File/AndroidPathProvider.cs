#nullable enable

using System;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 安卓的路径提供者：外部存储走 SAF 路径，应用数据目录仍用普通绝对路径
    /// </summary>
    /// <remarks>
    /// <para>安卓 10 及以后只能通过 Storage Access Framework 访问外部存储，
    /// 因此常用文件夹给的是 <c>content://</c> 路径</para>
    /// </remarks>
    public sealed class AndroidPathProvider : IPathProvider
    {
        public string? GetCommonFolderPath(CommonFolders folder)
        {
            return folder switch
            {
                // 系统下载目录的根，Simple File Browser 会把它当作 SAF 入口打开
                CommonFolders.Downloads => "content://com.android.externalstorage.documents/root/downloads",
                CommonFolders.Desktop => null,
                _ => throw new ArgumentOutOfRangeException(nameof(folder), folder, null)
            };
        }
    }
}
