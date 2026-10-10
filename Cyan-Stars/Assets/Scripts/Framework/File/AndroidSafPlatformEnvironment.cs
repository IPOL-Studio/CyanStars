#nullable enable

using System;
using CyanStars.Utils;
using SimpleFileBrowser;
using UnityEngine;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 安卓启用 Storage Access Framework 时的平台环境
    /// </summary>
    /// <remarks>
    /// <para>安卓 10 及以后只能通过 SAF 访问外部存储，因此常用文件夹给的是 <c>content://</c> 路径，
    /// 外部文件夹的整目录导入导出不可用；应用数据目录仍是普通绝对路径</para>
    /// </remarks>
    public sealed class AndroidSafPlatformEnvironment : IPlatformEnvironment
    {
        public string PersistentDataRoot => PathUtil.Normalize(Application.persistentDataPath);

        public string TemporaryCacheRoot => PathUtil.Normalize(Application.temporaryCachePath);

        // SAF 选中的目录是 content:// 路径，无法按普通路径做整目录复制
        public bool CanImportExternalFolder => false;

        public bool CanExportFolder => false;


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

        public bool RequiresStorageAccessFramework(string path)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            // Simple File Browser 会在运行时判断具体路径是否必须走 SAF
            return FileBrowserHelpers.ShouldUseSAFForPath(path);
#else
            // 本实现只会在安卓真机启用 SAF 时被选用，其它环境不会走到这里
            return false;
#endif
        }
    }
}
