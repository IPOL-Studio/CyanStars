#nullable enable

using UnityEngine;
using SimpleFileBrowser;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CatAsset.Runtime;
using CyanStars.Utils;
using CyanStars.Utils.JsonSerialization;
using Newtonsoft.Json;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件管理器，用于在运行时处理 外部数据 ←---→ 用户数据
    /// </summary>
    /// <remarks>
    /// <para>提供统一的文件和文件夹选择对话框功能，基于 Unity Simple File Browser 实现</para>
    /// <para>提供对玩家文件的资源文件读写，.json 文件反序列化和序列化功能，其中读和反序列化操作将传给 AssetManager 实现</para>
    /// <para>提供 读取外部文件-复制到缓存区-复制到用户数据 功能，可根据 GUID 获取文件当前路径</para>
    /// </remarks>
    public class FileManager : BaseManager
    {
        [SerializeField]
        private UISkin fileBrowserSkin = null!;


        public override int Priority { get; }


        private static string TempFolderPath =>
            PathUtil.Combine(Application.temporaryCachePath, nameof(FileManager));


        public readonly FileBrowser.Filter ChartFilter = new FileBrowser.Filter("谱面文件", ".json");
        public readonly FileBrowser.Filter SpriteFilter = new FileBrowser.Filter("图片", ".png");
        public readonly FileBrowser.Filter AudioFilter = new FileBrowser.Filter("音频", ".ogg");


        public override void OnInit()
        {
            // 删除上次游戏的临时文件
            DeleteTempSessionFolder();

            // 设置颜色主题
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


        private void DeleteTempSessionFolder()
        {
            // 清理旧的缓存路径
            if (Directory.Exists(Application.temporaryCachePath))
            {
                try
                {
                    Directory.Delete(Application.temporaryCachePath, true);
                    Debug.Log($"已清除缓存文件夹：{Application.temporaryCachePath}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"在删除缓存文件夹时捕获了异常：{e.Message}");
                }
            }
        }


        #region --- FileBrowser API 用于在运行时向玩家打开文件管理器 UI  ---

        /// <summary>
        /// 获取单个文件的路径
        /// </summary>
        /// <param name="onSuccess">成功获取的回调</param>
        /// <param name="onCancel">玩家取消的回调</param>
        /// <param name="title">窗口标题</param>
        /// <param name="showAllFilesFilter">是否允许玩家选择任意后缀的文件</param>
        /// <param name="filters">依据后缀筛选文件</param>
        /// <param name="defaultFilter">默认筛选后缀名</param>
        public void OpenLoadFilePathBrowser(Action<string>? onSuccess,
                                            Action? onCancel = null,
                                            string title = "打开文件",
                                            bool showAllFilesFilter = false,
                                            FileBrowser.Filter[]? filters = null,
                                            string? defaultFilter = null)
        {
            if (IsBrowserOpen()) return;

            FileBrowser.OnSuccess successWrapper = (paths) =>
            {
                if (paths.Length > 0)
                {
                    string path = paths[0];

                    if (path.StartsWith("content://"))
                        path = CopyContentFileToAppData(paths[0]); // TODO: 改为异步任务执行，目前没想好怎么在复制的时候展示 UI 交互，干脆先整个同步卡着

                    onSuccess?.Invoke(path);
                }
            };

            FileBrowser.OnCancel? cancelWrapper = onCancel != null ? new FileBrowser.OnCancel(onCancel) : null;

            FileBrowser.SetFilters(showAllFilesFilter, filters);
            FileBrowser.SetDefaultFilter(defaultFilter);
            FileBrowser.ShowLoadDialog(successWrapper, cancelWrapper,
                FileBrowser.PickMode.Files, false, null, null, title, "选择");
        }

        /// <summary>
        /// 获取多个文件的路径
        /// </summary>
        /// <param name="onSuccess">成功获取的回调，参数为文件路径数组</param>
        /// <param name="onCancel">玩家取消的回调</param>
        /// <param name="title">窗口标题</param>
        /// <param name="showAllFilesFilter">是否允许玩家选择任意后缀的文件</param>
        /// <param name="filters">依据后缀筛选文件</param>
        /// <param name="defaultFilter">默认筛选后缀名</param>
        public void OpenLoadFilePathsBrowser(Action<string[]>? onSuccess,
                                             Action? onCancel = null,
                                             string title = "打开文件",
                                             bool showAllFilesFilter = false,
                                             FileBrowser.Filter[]? filters = null,
                                             string? defaultFilter = null)
        {
            if (IsBrowserOpen()) return;

            FileBrowser.OnSuccess successWrapper = (paths) =>
            {
                onSuccess?.Invoke(paths);
            };

            FileBrowser.OnCancel? cancelWrapper = onCancel != null ? new FileBrowser.OnCancel(onCancel) : null;

            FileBrowser.SetFilters(showAllFilesFilter, filters);
            FileBrowser.SetDefaultFilter(defaultFilter);
            FileBrowser.ShowLoadDialog(successWrapper, cancelWrapper,
                FileBrowser.PickMode.Files, true, null, null, title, "选择");
        }

        /// <summary>
        /// 获取要加载的文件夹路径
        /// </summary>
        /// <param name="onSuccess">成功获取的回调</param>
        /// <param name="onCancel">玩家取消的回调</param>
        /// <param name="title">窗口标题</param>
        public void OpenLoadFolderPathBrowser(Action<string>? onSuccess,
                                              Action? onCancel = null,
                                              string title = "打开文件夹")
        {
            if (IsBrowserOpen()) return;

            FileBrowser.OnSuccess successWrapper = (paths) =>
            {
                if (paths.Length > 0)
                {
                    string path = paths[0];

                    if (path.StartsWith("content://"))
                        path = CopyContentFolderToAppData(path);

                    onSuccess?.Invoke(path);
                }
            };

            FileBrowser.OnCancel? cancelWrapper = onCancel != null ? new FileBrowser.OnCancel(onCancel) : null;

            FileBrowser.ShowLoadDialog(successWrapper, cancelWrapper,
                FileBrowser.PickMode.Folders, false, null, null, title, "选择");
        }

        /// <summary>
        /// 获取要保存到的文件夹路径
        /// </summary>
        /// <param name="onSuccess">成功获取的回调</param>
        /// <param name="onCancel">玩家取消的回调</param>
        /// <param name="title">窗口标题</param>
        public void OpenSaveFolderPathBrowser(Action<string>? onSuccess,
                                              Action? onCancel = null,
                                              string title = "保存到文件夹")
        {
            if (IsBrowserOpen()) return;

            FileBrowser.OnSuccess successWrapper = (paths) =>
            {
                if (paths.Length > 0)
                {
                    onSuccess?.Invoke(paths[0]);
                }
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
        /// 将安卓的 content:// 文件复制到 TempFolderPath，并返回复制后的文件的 file://
        /// </summary>
        private string CopyContentFileToAppData(string contentUri)
        {
            // 检查 uri 合法性
            if (string.IsNullOrEmpty(contentUri) || !contentUri.StartsWith("content://"))
                throw new ArgumentException($"传入的路径不是 contentUri：{contentUri}", nameof(contentUri));

            try
            {
                // 获取文件名
                string fileName = FileBrowserHelpers.GetFilename(contentUri);
                if (string.IsNullOrEmpty(fileName))
                    throw new Exception("无法获取文件名，可能是暂不支持此操作");

                // 拼接并创建目标路径
                string destinationPath = PathUtil.Combine(TempFolderPath, fileName);
                if (!Directory.Exists(TempFolderPath))
                    Directory.CreateDirectory(TempFolderPath);

                // 如果目标文件已存在，先删除它（避免覆盖报错）
                if (System.IO.File.Exists(destinationPath))
                    System.IO.File.Exists(destinationPath);

                // 使用 FileBrowserHelpers 进行拷贝
                FileBrowserHelpers.CopyFile(contentUri, destinationPath);

                // 检查是否拷贝成功
                if (!System.IO.File.Exists(destinationPath))
                    throw new Exception("文件复制失败，目标文件未生成。");


                return "file://" + destinationPath;
            }
            catch (Exception e)
            {
                Debug.LogError("复制文件时发生异常: " + e.Message);
                throw;
            }
        }

        /// <summary>
        /// 将安卓的 content:// 路径复制到 TempFolderPath，并返回复制后的路径
        /// </summary>
        private string CopyContentFolderToAppData(string contentUri)
        {
            // 检查 uri 合法性
            if (string.IsNullOrEmpty(contentUri) || !contentUri.StartsWith("content://"))
                throw new ArgumentException($"传入的路径不是有效的 contentUri：{contentUri}", nameof(contentUri));

            try
            {
                // 获取文件夹名称
                string folderName = FileBrowserHelpers.GetFilename(contentUri);
                if (string.IsNullOrEmpty(folderName))
                    throw new Exception("无法获取文件夹名，可能是暂不支持此操作");

                // 拼接目标路径
                string destinationFolderPath = PathUtil.Combine(TempFolderPath, folderName);

                // 确保父目录存在
                if (!Directory.Exists(TempFolderPath))
                    Directory.CreateDirectory(TempFolderPath);

                // 如果目标文件夹已存在，先删除它
                if (Directory.Exists(destinationFolderPath))
                    Directory.Delete(destinationFolderPath, true);

                // 使用 FileBrowserHelpers 进行文件夹拷贝
                FileBrowserHelpers.CopyDirectory(contentUri, destinationFolderPath);

                // 检查是否拷贝成功
                if (!Directory.Exists(destinationFolderPath))
                    throw new Exception("文件夹复制失败，目标目录未生成。");

                return destinationFolderPath;
            }
            catch (Exception e)
            {
                Debug.LogError("复制文件夹时发生异常: " + e.Message);
                throw;
            }
        }

        #endregion

        #region --- Serialization API 用于在运行时读写 .json 文件 ---

        /// <summary>
        /// 从指定的绝对路径加载资源（如图片、文本、音频等）
        /// </summary>
        /// <typeparam name="T">要加载的资源类型，如 Texture2D, TextAsset, AudioClip 等</typeparam>
        /// <param name="absolutePath">文件的完整绝对路径</param>
        /// <param name="target">资源加载后要绑定的游戏对象，用于自动管理生命周期</param>
        /// <param name="priority">加载任务的优先级</param>
        /// <returns>加载完成的资源，如果失败则为 null</returns>
        public async Task<T> LoadAssetFromPathAsync<T>(string absolutePath, CancellationToken cancellationToken = default,
                                                       TaskPriority priority = TaskPriority.Middle) where T : class
        {
            if (string.IsNullOrEmpty(absolutePath))
            {
                Debug.LogError("LoadAssetFromPathAsync Error: Provided path is null or empty.");
                return null;
            }

            if (!System.IO.File.Exists(absolutePath))
            {
                Debug.LogError($"LoadAssetFromPathAsync Error: File does not exist at path: {absolutePath}");
                return null;
            }

            try
            {
                // 直接将绝对路径传递给 CatAsset。CatAsset 会将其作为外部原生资源处理。
                // CatAsset 内部会负责读取文件的 byte[] 并根据类型 T 进行转换。
                return (await CatAssetManager.LoadAssetAsync<T>(absolutePath, cancellationToken, priority)).Asset;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load asset from path '{absolutePath}' using CatAsset. Exception: {e}");
                return null;
            }
        }


        /// <summary>
        /// 序列化对象为 Json 文件
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="filePath">保存到的路径和文件全名</param>
        /// <returns>是否成功序列化</returns>
        public bool SerializationToJson(object obj, string filePath)
        {
            try
            {
                // 设置序列化格式参数
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None,
                    Formatting = Formatting.Indented,
                    Culture = CultureInfo.InvariantCulture,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    Converters = JsonConverters.Converters
                };

                // 如果目录不存在，创建目录
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(obj, settings);
                System.IO.File.WriteAllText(filePath, json);
                Debug.Log($"序列化完成，文件路径：{filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"序列化时出现异常：{e}");
                return false;
            }
        }

        #endregion
    }
}
