#nullable enable

using System.Collections.Generic;
using CyanStars.Utils;
using UnityEngine;

#if !UNITY_EDITOR && UNITY_ANDROID
using SimpleFileBrowser;
#endif


namespace CyanStars.Framework.File
{
    /// <summary>
    /// 安卓启用 Storage Access Framework 时的平台环境
    /// </summary>
    /// <remarks>
    /// <para>安卓 10 及以后只能通过 SAF 访问外部存储，因此常用文件夹给的是 <c>content://</c> 路径；
    /// 应用数据目录仍是普通绝对路径</para>
    /// </remarks>
    public sealed class AndroidSafPlatformEnvironment : IPlatformEnvironment
    {
        private const string DownloadsFolderPath = "content://com.android.externalstorage.documents/root/downloads";


        public string PersistentDataRoot => PathUtil.Normalize(Application.persistentDataPath);

        public string TemporaryCacheRoot => PathUtil.Normalize(Application.temporaryCachePath);

        public string? GameInstallRoot => null;

        // TODO: SAF 目录的复制已接入 FolderUtil，待真机验证权限与重名处理后开放
        public bool CanImportExternalFolder => false;

        public bool CanExportFolder => false;

        public bool CanCustomizeAppPaths => false;

        public bool CanRestartApplication => false;


        public IReadOnlyList<PlatformFolder> GetCommonFolders()
        {
            // 系统下载目录的根，Simple File Browser 会把它当作 SAF 入口打开
            return new[] { new PlatformFolder("下载", DownloadsFolderPath) };
        }

        public bool IsLocalPath(string path)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            return !FileBrowserHelpers.ShouldUseSAFForPath(path);
#else
            // 本实现只会在安卓真机启用 SAF 时被选用，其它环境不会走到这里
            return true;
#endif
        }
    }
}
