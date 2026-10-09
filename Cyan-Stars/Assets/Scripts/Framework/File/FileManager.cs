#nullable enable

using System;
using CyanStars.Utils;
using SimpleFileBrowser;
using UnityEngine;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件管理器：向玩家打开文件（夹）选择对话框
    /// </summary>
    /// <remarks>选中的文件复制进缓存并创建句柄；选中的文件夹只返回归一化后的路径</remarks>
    public class FileManager : BaseManager
    {
        [SerializeField]
        private UISkin fileBrowserSkin = null!;


        public override int Priority { get; }


        // TODO: 业务逻辑与文件选择器插件混用过滤器，后续考虑分离成自有过滤器
        public readonly FileBrowser.Filter ChartFilter = new FileBrowser.Filter("谱面文件", ".json");
        public readonly FileBrowser.Filter SpriteFilter = new FileBrowser.Filter("图片", ".png");
        public readonly FileBrowser.Filter AudioFilter = new FileBrowser.Filter("音频", ".ogg");


        public override void OnInit()
        {
            PlatformFilePortal.Init();

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
        /// 退出游戏时删除本次会话的临时目录
        /// </summary>
        public void OnDestroy()
        {
            PlatformFilePortal.Shutdown();
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
        /// <param name="defaultFilter">默认后缀过滤器</param>
        /// <param name="cacheKind">需要复制进缓存时放在哪一层缓存下</param>
        public void OpenLoadFileHandleBrowser(
            Action<FileHandle>? onSuccess,
            Action? onCancel = null,
            string title = "打开文件",
            bool showAllFilesFilter = false,
            FileBrowser.Filter[]? filters = null,
            string? defaultFilter = null,
            FileCacheKind cacheKind = FileCacheKind.CrossPlatform
        )
        {
            if (IsBrowserOpen()) return;

            FileBrowser.OnSuccess successWrapper = (paths) =>
            {
                if (paths.Length == 0)
                    return;

                FileHandle? fileHandle = PlatformFilePortal.TryLoadFile(paths[0], cacheKind);
                if (fileHandle == null)
                {
                    Debug.LogError($"无法加载玩家选中的文件，已放弃本次选择：{paths[0]}");
                    onCancel?.Invoke();
                    return;
                }

                onSuccess?.Invoke(fileHandle);
            };

            FileBrowser.OnCancel? cancelWrapper = onCancel != null ? new FileBrowser.OnCancel(onCancel) : null;

            FileBrowser.SetFilters(showAllFilesFilter, filters);
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
        private static void AddQuickLink(CommonFolders folder, string displayName)
        {
            string? folderPath = PlatformFilePortal.GetCommonFolderPath(folder);
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogWarning($"当前平台上没有可用的常用文件夹，已跳过侧边栏入口：{folder}");
                return;
            }

            FileBrowser.AddQuickLink(displayName, folderPath, null);
        }
    }
}
