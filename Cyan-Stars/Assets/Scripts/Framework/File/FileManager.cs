#nullable enable

using UnityEngine;
using SimpleFileBrowser;
using System;
using CyanStars.Utils;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件管理器：文件（夹）选择对话框与外部文件读写
    /// </summary>
    /// <remarks>
    /// <para>选中的「文件」和读取用的「文件夹」返回的是可直接交给 <see cref="System.IO"/> 的普通绝对路径：安卓上通过 SAF 选中的内容会先复制进 <see cref="fileManagerStaging"/> 再返回，不带 <c>content://</c> 或 <c>file://</c> 这类 scheme。</para>
    /// <para>保存目标文件夹是写入目标，不会复制到缓存区，返回系统给出的路径，安卓上可能仍是 <c>content://</c>；需要暂存文件时不要用本类，应为每个作用域建一个 <see cref="TempFileStore"/>。</para>
    /// </remarks>
    public class FileManager : BaseManager
    {
        /// <summary>
        /// 安卓 SAF 的 content URI 前缀
        /// </summary>
        private const string ContentUriPrefix = "content://";

        [SerializeField]
        private UISkin fileBrowserSkin = null!;


        public override int Priority { get; }


        /// <summary>
        /// FileManager 使用的缓存区
        /// </summary>
        /// <remarks>玩家选中的文件会先复制到这里，再由各作用域自己的 <see cref="TempFileStore"/> 接手</remarks>
        private TempFileStore fileManagerStaging = null!;


        public readonly FileBrowser.Filter ChartFilter = new FileBrowser.Filter("谱面文件", ".json");
        public readonly FileBrowser.Filter SpriteFilter = new FileBrowser.Filter("图片", ".png");
        public readonly FileBrowser.Filter AudioFilter = new FileBrowser.Filter("音频", ".ogg");


        public override void OnInit()
        {
            // 清理上次运行残留的缓存区
            TempFileStore.DeleteAllTempCaches();

            // 创建缓存区作用域
            fileManagerStaging = new TempFileStore(nameof(FileManager));

            // 设置文件选择器弹窗的颜色主题
            FileBrowser.Skin = fileBrowserSkin;

            // 显示所有后缀的文件，包括默认排除的 .lnk 和 .tmp
            FileBrowser.SetExcludedExtensions();

            // 添加侧边栏快速链接
            FileBrowser.AddQuickLink("游戏数据目录", Application.persistentDataPath, null);
            FileBrowser.AddQuickLink("桌面", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), null);

            Debug.Log("FileManager Initialized.");
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        /// <summary>
        /// 销毁时丢弃整个缓存区
        /// </summary>
        public void OnDestroy()
        {
            fileManagerStaging.Discard();
            TempFileStore.DeleteAllTempCaches();
        }


        #region --- 外部文件（含安卓 content: 路径）的静态工具方法 ---

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

        /// <summary>
        /// 复制文件（夹），不做任何覆盖检查，失败时抛出异常
        /// </summary>
        /// <param name="sourcePath">源文件（夹）绝对路径，可以是普通路径或安卓 content:// 路径</param>
        /// <param name="destinationPath">目标文件（夹）绝对路径</param>
        /// <param name="isFolder">源路径是否为文件夹</param>
        internal static void CopyFileOrFolderUnchecked(string sourcePath, string destinationPath, bool isFolder)
        {
            if (isFolder)
            {
                FileBrowserHelpers.CopyDirectory(sourcePath, destinationPath);
                return;
            }

            FileBrowserHelpers.CopyFile(sourcePath, destinationPath);
        }

        /// <summary>
        /// 复制文件（夹）
        /// </summary>
        /// <param name="sourcePath">源文件（夹）绝对路径，可以是普通路径或安卓 content:// 路径</param>
        /// <param name="destinationPath">目标文件（夹）绝对路径</param>
        /// <param name="isFolder">源路径是否为文件夹</param>
        /// <param name="overwrite">允许覆盖目标路径原有的文件（夹）</param>
        /// <returns>是否复制成功</returns>
        /// <remarks>底层复制是覆盖式的，不允许覆盖时由本方法先检查目标是否存在</remarks>
        public static bool CopyFileOrFolder(
            string sourcePath,
            string destinationPath,
            bool isFolder,
            bool overwrite
        )
        {
            if (!overwrite)
            {
                bool destinationExists = isFolder
                    ? FileBrowserHelpers.DirectoryExists(destinationPath)
                    : FileBrowserHelpers.FileExists(destinationPath);

                if (destinationExists)
                {
                    Debug.LogWarning($"目标路径已存在，按要求不覆盖：{destinationPath}");
                    return false;
                }
            }

            try
            {
                CopyFileOrFolderUnchecked(sourcePath, destinationPath, isFolder);
            }
            catch (Exception e)
            {
                Debug.LogError($"复制文件（夹）时出错：{e.Message}");
                return false;
            }

            return true;
        }

        #endregion

        #region --- FileBrowser API 用于在运行时向玩家打开文件管理器 UI  ---

        /// <summary>
        /// 获取单个文件的路径，安卓文件会先复制进缓存区，然后返回缓存区文件路径
        /// </summary>
        /// <param name="onSuccess">成功获取的回调，参数为普通绝对路径</param>
        /// <param name="onCancel">玩家取消的回调</param>
        /// <param name="title">窗口标题</param>
        /// <param name="showAllFilesFilter">是否允许玩家选择任意后缀的文件</param>
        /// <param name="filters">依据后缀筛选文件</param>
        /// <param name="defaultFilter">默认筛选后缀名</param>
        public void OpenLoadFilePathBrowser(
            Action<string>? onSuccess,
            Action? onCancel = null,
            string title = "打开文件",
            bool showAllFilesFilter = false,
            FileBrowser.Filter[]? filters = null,
            string? defaultFilter = null
        )
        {
            if (IsBrowserOpen()) return;

            FileBrowser.OnSuccess successWrapper = (paths) =>
            {
                if (paths.Length == 0)
                    return;

                // TODO: 改为异步任务执行
                if (!TryResolvePlainPaths(paths, "文件", out string[] resolvedPaths))
                    return;

                onSuccess?.Invoke(resolvedPaths[0]);
            };

            FileBrowser.OnCancel? cancelWrapper = onCancel != null ? new FileBrowser.OnCancel(onCancel) : null;

            FileBrowser.SetFilters(showAllFilesFilter, filters);
            FileBrowser.SetDefaultFilter(defaultFilter);
            FileBrowser.ShowLoadDialog(successWrapper, cancelWrapper,
                FileBrowser.PickMode.Files, false, null, null, title, "选择");
        }

        /// <summary>
        /// 获取多个文件的路径，安卓文件会先复制进缓存区，然后返回缓存区文件路径
        /// </summary>
        /// <param name="onSuccess">成功获取的回调，参数为普通绝对路径数组</param>
        /// <param name="onCancel">玩家取消的回调</param>
        /// <param name="title">窗口标题</param>
        /// <param name="showAllFilesFilter">是否允许玩家选择任意后缀的文件</param>
        /// <param name="filters">依据后缀筛选文件</param>
        /// <param name="defaultFilter">默认筛选后缀名</param>
        public void OpenLoadFilePathsBrowser(
            Action<string[]>? onSuccess,
            Action? onCancel = null,
            string title = "打开文件",
            bool showAllFilesFilter = false,
            FileBrowser.Filter[]? filters = null,
            string? defaultFilter = null
        )
        {
            if (IsBrowserOpen()) return;

            FileBrowser.OnSuccess successWrapper = (paths) =>
            {
                if (paths.Length == 0)
                    return;

                // 与单选入口保持一致，批量选择同样转换 content:// 路径
                if (!TryResolvePlainPaths(paths, "文件", out string[] resolvedPaths))
                    return;

                onSuccess?.Invoke(resolvedPaths);
            };

            FileBrowser.OnCancel? cancelWrapper = onCancel != null ? new FileBrowser.OnCancel(onCancel) : null;

            FileBrowser.SetFilters(showAllFilesFilter, filters);
            FileBrowser.SetDefaultFilter(defaultFilter);
            FileBrowser.ShowLoadDialog(successWrapper, cancelWrapper,
                FileBrowser.PickMode.Files, true, null, null, title, "选择");
        }

        /// <summary>
        /// 获取要加载的文件夹路径，安卓文件会先复制进缓存区，然后返回缓存区文件路径
        /// </summary>
        /// <param name="onSuccess">成功获取的回调，参数为普通绝对路径</param>
        /// <param name="onCancel">玩家取消的回调</param>
        /// <param name="title">窗口标题</param>
        public void OpenLoadFolderPathBrowser(
            Action<string>? onSuccess,
            Action? onCancel = null,
            string title = "打开文件夹"
        )
        {
            if (IsBrowserOpen()) return;

            FileBrowser.OnSuccess successWrapper = (paths) =>
            {
                if (paths.Length == 0)
                    return;

                if (!TryResolvePlainPaths(paths, "文件夹", out string[] resolvedPaths))
                    return;

                onSuccess?.Invoke(resolvedPaths[0]);
            };

            FileBrowser.OnCancel? cancelWrapper = onCancel != null ? new FileBrowser.OnCancel(onCancel) : null;

            FileBrowser.ShowLoadDialog(successWrapper, cancelWrapper,
                FileBrowser.PickMode.Folders, false, null, null, title, "选择");
        }

        /// <summary>
        /// 获取要保存到的文件夹路径
        /// </summary>
        /// <param name="onSuccess">成功获取的回调，参数为归一化后的文件夹路径</param>
        /// <param name="onCancel">玩家取消的回调</param>
        /// <param name="title">窗口标题</param>
        /// <remarks>返回值只过一遍 <see cref="PathUtil.Normalize"/>，不会复制到缓存区，安卓上可能仍是 <c>content://</c>。</remarks>
        public void OpenSaveFolderPathBrowser(
            Action<string>? onSuccess,
            Action? onCancel = null,
            string title = "保存到文件夹"
        )
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
        /// 把文件对话框返回的文件（夹）路径统一成普通绝对路径；若是安卓 content:// 路径则先复制到缓存区，再返回缓存区可读写的路径
        /// </summary>
        /// <param name="paths">对话框返回的路径</param>
        /// <param name="description">日志里用的操作对象描述，例如「文件」「文件夹」</param>
        /// <param name="resolvedPaths">解析结果；失败时为空数组</param>
        /// <returns>是否全部解析成功</returns>
        /// <remarks>不做部分成功：任一路径解析失败就整批放弃，返回 false 并只记日志，不回调调用方。</remarks>
        private bool TryResolvePlainPaths(string[] paths, string description, out string[] resolvedPaths)
        {
            try
            {
                var resolved = new string[paths.Length];
                for (int i = 0; i < paths.Length; i++)
                    resolved[i] = ResolvePlainPath(paths[i]);

                resolvedPaths = resolved;
                return true;
            }
            catch (Exception e)
            {
                resolvedPaths = Array.Empty<string>();
                Debug.LogError($"处理玩家选中的{description}时出错，已放弃本次选择：{e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 把文件对话框返回的文件（夹）路径统一成普通绝对路径；若是安卓 content:// 路径则先复制到缓存区，再返回缓存区可读写的路径
        /// </summary>
        /// <param name="path">对话框返回的路径，可能是普通路径、content:// 或 file:// 路径</param>
        /// <returns>可直接交给 <see cref="System.IO"/> 使用的普通绝对路径</returns>
        /// <exception cref="Exception">复制到缓存区失败</exception>
        /// <remarks>
        /// <para>安卓上从 SAF 选中的内容是 <c>content://</c> 路径，会先复制进缓存区再返回；其它路径过一遍 <see cref="PathUtil.Normalize"/> 后原样返回，不产出 <c>file://</c> 路径。</para>
        /// <para>本方法会抛异常，调用方应改用 <see cref="TryResolvePlainPaths"/>。</para>
        /// </remarks>
        private string ResolvePlainPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("传入路径为空", nameof(path));

            if (!IsContentUri(path))
                return PathUtil.Normalize(path);

            if (IsFileExists(path))
                return CopyContentFileToStaging(path);
            if (IsFolderExists(path))
                return CopyContentFolderToStaging(path);

            throw new ArgumentException("传入路径不指向任何文件或文件夹");
        }

        /// <summary>
        /// 把安卓的 content:// 文件复制到缓存区，返回复制后的普通绝对路径
        /// </summary>
        private string CopyContentFileToStaging(string contentUri)
        {
            // 检查 uri 合法性
            if (!IsContentUri(contentUri))
                throw new ArgumentException($"传入的路径不是 contentUri：{contentUri}", nameof(contentUri));

            try
            {
                string destinationPath = fileManagerStaging.CopyInFile(contentUri);

                // 检查是否拷贝成功
                if (!IsFileExists(destinationPath))
                    throw new Exception("文件复制失败，目标文件未生成。");

                return destinationPath;
            }
            catch (Exception e)
            {
                Debug.LogError("复制文件时发生异常: " + e.Message);
                throw;
            }
        }

        /// <summary>
        /// 把安卓的 content:// 文件夹复制到缓存区，返回复制后的普通绝对路径
        /// </summary>
        private string CopyContentFolderToStaging(string contentUri)
        {
            // 检查 uri 合法性
            if (!IsContentUri(contentUri))
                throw new ArgumentException($"传入的路径不是有效的 contentUri：{contentUri}", nameof(contentUri));

            try
            {
                string destinationFolderPath = fileManagerStaging.CopyInFolder(contentUri);

                // 检查是否拷贝成功
                if (!IsFolderExists(destinationFolderPath))
                    throw new Exception("文件夹复制失败，目标目录未生成。");

                return destinationFolderPath;
            }
            catch (Exception e)
            {
                Debug.LogError("复制文件夹时发生异常: " + e.Message);
                throw;
            }
        }

        private static bool IsContentUri(string path)
        {
            return path.StartsWith(ContentUriPrefix, StringComparison.Ordinal);
        }

        #endregion

    }
}
