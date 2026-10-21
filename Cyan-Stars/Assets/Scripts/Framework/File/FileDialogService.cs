#nullable enable

using System;
using System.Linq;
using CyanStars.Utils;
using SimpleFileBrowser;
using UnityEngine;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件对话框服务：负责文件（夹）选择对话框的展示与筛选器转换
    /// </summary>
    internal sealed class FileDialogService
    {
        private readonly IPlatformEnvironment Environment;


        public FileDialogService(IPlatformEnvironment environment)
        {
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }


        /// <summary>
        /// 初始化对话框皮肤与快捷入口
        /// </summary>
        public void Initialize(UISkin skin)
        {
            FileBrowser.Skin = skin;
            FileBrowser.SetExcludedExtensions();
            AddQuickLink(CommonFolders.Downloads, "下载");
            AddQuickLink(CommonFolders.Desktop, "桌面");
        }

        /// <summary>
        /// 打开文件选择对话框
        /// </summary>
        /// <returns>是否成功打开；已有对话框打开时返回 false，并触发 <paramref name="onCancel"/></returns>
        public bool TryOpenLoadFilePathBrowser(
            Action<string>? onSuccess,
            Action? onCancel = null,
            string title = "打开文件",
            bool showAllFilesFilter = false,
            FileTypeFilter[]? filters = null,
            string? defaultFilter = null)
        {
            if (!TryBeginDialog(onCancel))
                return false;

            FileBrowser.SetFilters(showAllFilesFilter, ConvertFilters(filters));
            FileBrowser.SetDefaultFilter(defaultFilter);

            return TryShowDialog(
                () => FileBrowser.ShowLoadDialog(
                    WrapOnSuccess(onSuccess, onCancel),
                    WrapOnCancel(onCancel),
                    FileBrowser.PickMode.Files,
                    false,
                    null,
                    null,
                    title,
                    "选择"
                ),
                onCancel
            );
        }

        /// <summary>
        /// 打开文件夹选择对话框
        /// </summary>
        /// <returns>是否成功打开；已有对话框打开时返回 false，并触发 <paramref name="onCancel"/></returns>
        public bool TryOpenLoadFolderPathBrowser(
            Action<string>? onSuccess,
            Action? onCancel = null,
            string title = "打开文件夹")
        {
            if (!TryBeginDialog(onCancel))
                return false;

            return TryShowDialog(
                () => FileBrowser.ShowLoadDialog(
                    WrapOnSuccess(onSuccess, onCancel),
                    WrapOnCancel(onCancel),
                    FileBrowser.PickMode.Folders,
                    false,
                    null,
                    null,
                    title,
                    "选择"
                ),
                onCancel
            );
        }

        /// <summary>
        /// 打开文件夹保存对话框
        /// </summary>
        /// <returns>是否成功打开；已有对话框打开时返回 false，并触发 <paramref name="onCancel"/></returns>
        public bool TryOpenSaveFolderPathBrowser(
            Action<string>? onSuccess,
            Action? onCancel = null,
            string title = "保存到")
        {
            if (!TryBeginDialog(onCancel))
                return false;

            return TryShowDialog(
                () => FileBrowser.ShowSaveDialog(
                    WrapOnSuccess(onSuccess, onCancel),
                    WrapOnCancel(onCancel),
                    FileBrowser.PickMode.Folders,
                    false,
                    null,
                    null,
                    title,
                    "选择"
                ),
                onCancel
            );
        }


        /// <summary>
        /// 包装成功回调：归一化玩家选中的首个路径后透传；未选中任何路径时视为取消
        /// </summary>
        private static FileBrowser.OnSuccess WrapOnSuccess(Action<string>? onSuccess, Action? onCancel)
        {
            return paths =>
            {
                if (paths.Length == 0)
                {
                    onCancel?.Invoke();
                    return;
                }

                onSuccess?.Invoke(PathUtil.Normalize(paths[0]));
            };
        }

        private static FileBrowser.OnCancel? WrapOnCancel(Action? onCancel)
        {
            return onCancel != null ? new FileBrowser.OnCancel(onCancel) : null;
        }

        private static bool TryBeginDialog(Action? onCancel)
        {
            if (!FileBrowser.IsOpen)
                return true;

            Debug.LogWarning("无法打开新的文件对话框，因为已有对话框处于打开状态。");
            onCancel?.Invoke();
            return false;
        }

        private static bool TryShowDialog(Func<bool> showDialog, Action? onCancel)
        {
            if (showDialog())
                return true;

            onCancel?.Invoke();
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
            string? folderPath = Environment.GetCommonFolderPath(folder);
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogWarning($"当前平台上没有可用的常用文件夹，已跳过侧边栏入口：{folder}");
                return;
            }

            FileBrowser.AddQuickLink(displayName, folderPath, null);
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
    }
}
