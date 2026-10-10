#nullable enable

using System;
using System.Linq;
using CyanStars.Utils;
using SimpleFileBrowser;
using UnityEngine;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件管理器：向玩家打开文件（夹）选择对话框，并持有平台环境与文件门户
    /// </summary>
    /// <remarks>
    /// <para>初始化时按编译和运行环境创建唯一一个 <see cref="IPlatformEnvironment"/> 实现并注入 <see cref="FilePortal"/>，
    /// 业务逻辑经 <see cref="Environment"/> 与 <see cref="Portal"/> 获取平台路径能力和文件读写能力</para>
    /// <para>选中的文件复制进缓存并创建句柄；选中的文件夹只返回归一化后的路径</para>
    /// </remarks>
    public class FileManager : BaseManager
    {
        /// <summary>
        /// 默认缓存作用域：与具体业务无关的跨平台文件缓存，生命周期同游戏进程
        /// </summary>
        private const string DefaultCacheScope = "CrossPlatform";


        [SerializeField]
        private UISkin fileBrowserSkin = null!;


        /// <summary>
        /// 当前平台环境
        /// </summary>
        public IPlatformEnvironment Environment { get; private set; } = null!;

        /// <summary>
        /// 文件读写业务门户
        /// </summary>
        public FilePortal Portal { get; private set; } = null!;


        public override int Priority { get; }


        public override void OnInit()
        {
            Environment = CreatePlatformEnvironment();
            Portal = new FilePortal(Environment);
            Portal.Init();

            FileBrowser.Skin = fileBrowserSkin;
            FileBrowser.SetExcludedExtensions();

            AddQuickLink(CommonFolders.Downloads, "下载");
            AddQuickLink(CommonFolders.Desktop, "桌面");

            Debug.Log("FileManager Initialized.");
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        /// <summary>
        /// 退出游戏时结束文件门户并删除本次会话的临时目录
        /// </summary>
        private void OnDestroy()
        {
            Portal?.Shutdown();
        }


        #region --- 外部文件的静态工具方法 ---

        /// <summary>
        /// 文件是否存在（支持普通路径和安卓 content:// 路径）
        /// </summary>
        public static bool IsFileExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return FileBrowserHelpers.FileExists(path);
        }

        /// <summary>
        /// 文件夹是否存在（支持普通路径和安卓 content:// 路径）
        /// </summary>
        public static bool IsFolderExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return FileBrowserHelpers.DirectoryExists(path);
        }

        /// <summary>
        /// 取文件（夹）名（支持普通路径和安卓 content:// 路径）
        /// </summary>
        public static string GetFileOrFolderName(string path)
        {
            return string.IsNullOrEmpty(path) ? "" : FileBrowserHelpers.GetFilename(path);
        }

        #endregion

        #region --- 从对话框获取路径 ---

        /// <summary>
        /// 获取单个文件并创建句柄
        /// </summary>
        /// <param name="onSuccess">成功获取的回调，参数为文件句柄</param>
        /// <param name="onCancel">失败后的回调</param>
        /// <param name="title">弹窗标题</param>
        /// <param name="showAllFilesFilter">是否允许玩家选择任意后缀的文件</param>
        /// <param name="filters">依据后缀筛选文件</param>
        /// <param name="defaultFilter">默认后缀过滤器的显示名</param>
        /// <param name="cacheScope">缓存作用域标识，决定缓存副本放在会话临时目录的哪个子目录下</param>
        public void OpenLoadFileHandleBrowser(
            Action<FileHandle>? onSuccess,
            Action? onCancel = null,
            string title = "打开文件",
            bool showAllFilesFilter = false,
            FileTypeFilter[]? filters = null,
            string? defaultFilter = null,
            string cacheScope = DefaultCacheScope
        )
        {
            if (IsBrowserOpen()) return;

            FileBrowser.OnSuccess successWrapper = (paths) =>
            {
                if (paths.Length == 0)
                    return;

                FileHandle? fileHandle = Portal.TryLoadFile(paths[0], cacheScope);
                if (fileHandle == null)
                {
                    Debug.LogError($"无法加载玩家选中的文件，已放弃本次选择：{paths[0]}");
                    onCancel?.Invoke();
                    return;
                }

                onSuccess?.Invoke(fileHandle);
            };

            FileBrowser.OnCancel? cancelWrapper = onCancel != null ? new FileBrowser.OnCancel(onCancel) : null;

            FileBrowser.SetFilters(showAllFilesFilter, ConvertFilters(filters));
            FileBrowser.SetDefaultFilter(defaultFilter);
            FileBrowser.ShowLoadDialog(successWrapper, cancelWrapper,
                FileBrowser.PickMode.Files, false, null, null, title, "选择");
        }

        /// <summary>
        /// 获取要打开的文件夹路径
        /// </summary>
        /// <param name="onSuccess">成功获取的回调，参数为归一化后的文件夹路径</param>
        /// <param name="onCancel">取消后的回调</param>
        /// <param name="title">弹窗标题</param>
        /// <remarks>只返回路径，不复制进缓存</remarks>
        public void OpenLoadFolderPathBrowser(
            Action<string>? onSuccess,
            Action? onCancel = null,
            string title = "打开文件夹"
        )
        {
            ShowFolderPathBrowser(onSuccess, onCancel, title);
        }

        /// <summary>
        /// 获取要保存到的文件夹路径
        /// </summary>
        /// <param name="onSuccess">成功获取的回调，参数为归一化后的文件夹路径</param>
        /// <param name="onCancel">取消后的回调</param>
        /// <param name="title">弹窗标题</param>
        /// <remarks>保存目标是写入位置，不会复制到缓存区，因此安卓上可能仍是 <c>content://</c> 路径</remarks>
        public void OpenSaveFolderPathBrowser(
            Action<string>? onSuccess,
            Action? onCancel = null,
            string title = "保存到文件夹"
        )
        {
            ShowFolderPathBrowser(onSuccess, onCancel, title);
        }

        /// <summary>
        /// 打开文件夹选择对话框并返回归一化后的路径
        /// </summary>
        private void ShowFolderPathBrowser(Action<string>? onSuccess, Action? onCancel, string title)
        {
            if (IsBrowserOpen()) return;

            FileBrowser.OnSuccess successWrapper = (paths) =>
            {
                if (paths.Length == 0)
                    return;

                onSuccess?.Invoke(PathUtil.Normalize(paths[0]));
            };

            FileBrowser.OnCancel? cancelWrapper = onCancel != null ? new FileBrowser.OnCancel(onCancel) : null;

            FileBrowser.ShowLoadDialog(successWrapper, cancelWrapper,
                FileBrowser.PickMode.Folders, false, null, null, title, "选择");
        }

        #endregion


        /// <summary>
        /// 按编译和运行环境创建平台环境
        /// </summary>
        private static IPlatformEnvironment CreatePlatformEnvironment()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Simple File Browser 会在运行时判断安卓 10+ 是否必须走 SAF
            if (FileBrowserHelpers.ShouldUseSAF)
                return new AndroidSafPlatformEnvironment();
#endif
            return new DiskPlatformEnvironment();
        }

        /// <summary>
        /// 把自有过滤器转换为文件对话框插件的过滤器
        /// </summary>
        private static FileBrowser.Filter[]? ConvertFilters(FileTypeFilter[]? filters)
        {
            return filters
                ?.Select(filter => new FileBrowser.Filter(filter.DisplayName, filter.Extensions))
                .ToArray();
        }

        /// <summary>
        /// 检查文件浏览器是否已经打开，并打印警告
        /// </summary>
        private bool IsBrowserOpen()
        {
            if (FileBrowser.IsOpen)
            {
                Debug.LogWarning("无法打开新的文件对话框，因为已有对话框处于打开状态。");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 把常用文件夹加进文件对话框的侧边栏
        /// </summary>
        /// <param name="folder">常用文件夹枚举实例</param>
        /// <param name="displayName">侧边栏显示的名称</param>
        /// <remarks>当前平台上没有这个文件夹时只打日志，不影响其它入口</remarks>
        private void AddQuickLink(CommonFolders folder, string displayName)
        {
            string? folderPath = Portal.GetCommonFolderPath(folder);
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogWarning($"当前平台上没有可用的常用文件夹，已跳过侧边栏入口：{folder}");
                return;
            }

            FileBrowser.AddQuickLink(displayName, folderPath, null);
        }
    }
}
