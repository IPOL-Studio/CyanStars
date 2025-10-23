#nullable enable

using UnityEngine;
using SimpleFileBrowser;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CatAsset.Runtime;
using CyanStars.Framework.Utils;
using CyanStars.Framework.Utils.JsonSerialization;
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
        private UISkin skin;

        public readonly FileBrowser.Filter ChartFilter = new FileBrowser.Filter("谱面文件", ".json");
        public readonly FileBrowser.Filter SpriteFilter = new FileBrowser.Filter("图片", ".jpg", ".png");
        public readonly FileBrowser.Filter AudioFilter = new FileBrowser.Filter("音频", ".mp3", ".wav", ".ogg");

        private string TempFolderPath => Path.Combine(Application.temporaryCachePath, "FileManager");

        /// <summary>
        /// 目标路径:缓存文件路径 双向字典
        /// </summary>
        /// <remarks>键、值都不应该重复</remarks>
        private BiDirectionalDictionary<string, string> targetPathToTempPathMap =
            new BiDirectionalDictionary<string, string>();


        public override int Priority { get; }


        public override void OnInit()
        {
            // 设置颜色主题
            FileBrowser.Skin = skin;

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


        #region --- FileBrowser API ---

        /// <summary>
        /// 获取单个文件的路径
        /// </summary>
        /// <param name="onSuccess">成功获取的回调</param>
        /// <param name="onCancel">玩家取消的回调</param>
        /// <param name="title">窗口标题</param>
        /// <param name="showAllFilesFilter">是否允许玩家选择任意后缀的文件</param>
        /// <param name="filters">依据后缀筛选文件</param>
        /// <param name="defaultFilter">默认筛选后缀名</param>
        public void OpenLoadFilePathBrowser(Action<string> onSuccess,
            Action onCancel = null,
            string title = "打开文件",
            bool showAllFilesFilter = false,
            FileBrowser.Filter[] filters = null,
            string defaultFilter = null)
        {
            if (IsBrowserOpen()) return;

            FileBrowser.OnSuccess successWrapper = (paths) =>
            {
                if (paths.Length > 0)
                {
                    onSuccess?.Invoke(paths[0]);
                }
            };

            FileBrowser.OnCancel cancelWrapper = onCancel != null ? new FileBrowser.OnCancel(onCancel) : null;

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
        public void OpenLoadFilePathsBrowser(Action<string[]> onSuccess,
            Action onCancel = null,
            string title = "打开文件",
            bool showAllFilesFilter = false,
            FileBrowser.Filter[] filters = null,
            string defaultFilter = null)
        {
            if (IsBrowserOpen()) return;

            FileBrowser.OnSuccess successWrapper = (paths) => { onSuccess?.Invoke(paths); };

            FileBrowser.OnCancel cancelWrapper = onCancel != null ? new FileBrowser.OnCancel(onCancel) : null;

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
        public void OpenLoadFolderPathBrowser(Action<string> onSuccess,
            Action onCancel = null,
            string title = "打开文件夹")
        {
            if (IsBrowserOpen()) return;

            FileBrowser.OnSuccess successWrapper = (paths) =>
            {
                if (paths.Length > 0)
                {
                    onSuccess?.Invoke(paths[0]);
                }
            };

            FileBrowser.OnCancel cancelWrapper = onCancel != null ? new FileBrowser.OnCancel(onCancel) : null;

            FileBrowser.ShowLoadDialog(successWrapper, cancelWrapper,
                FileBrowser.PickMode.Folders, false, null, null, title, "选择");
        }

        /// <summary>
        /// 获取要保存到的文件夹路径
        /// </summary>
        /// <param name="onSuccess">成功获取的回调</param>
        /// <param name="onCancel">玩家取消的回调</param>
        /// <param name="title">窗口标题</param>
        public void OpenSaveFolderPathBrowser(Action<string> onSuccess,
            Action onCancel = null,
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

            FileBrowser.OnCancel cancelWrapper = onCancel != null ? new FileBrowser.OnCancel(onCancel) : null;

            FileBrowser.ShowLoadDialog(successWrapper, cancelWrapper,
                FileBrowser.PickMode.Folders, false, null, null, title, "选择");
        }

        #endregion

        #region --- Serialization API ---

        /// <summary>
        /// 从指定的绝对路径加载资源（如图片、文本、音频等）
        /// </summary>
        /// <typeparam name="T">要加载的资源类型，如 Texture2D, TextAsset, AudioClip 等</typeparam>
        /// <param name="absolutePath">文件的完整绝对路径</param>
        /// <param name="target">资源加载后要绑定的游戏对象，用于自动管理生命周期</param>
        /// <param name="priority">加载任务的优先级</param>
        /// <returns>加载完成的资源，如果失败则为 null</returns>
        public async Task<T> LoadAssetFromPathAsync<T>(string absolutePath, GameObject target = null,
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
                T asset = await CatAssetManager.LoadAssetAsync<T>(absolutePath, target, priority);
                return asset;
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

        #region --- TempFiles API ---

        /// <summary>
        /// 将文件复制到缓存区
        /// </summary>
        /// <remarks>将会覆盖目标文件！</remarks>
        /// <param name="originalFilePath">原始文件绝对路径（含文件名和后缀）</param>
        /// <param name="targetFilePath">目标绝对路径（含文件名和后缀）</param>
        /// <returns>缓存文件绝对路径（含文件名和后缀）</returns>
        /// <exception cref="ArgumentException">参数为空或 null / 重复的目标路径或文件缓存路径</exception>
        /// <exception cref="FileNotFoundException">原始文件不存在</exception>
        public string TempFile(string originalFilePath, string targetFilePath)
        {
            if (string.IsNullOrEmpty(originalFilePath))
            {
                throw new ArgumentException("原始文件路径不能为空。", nameof(originalFilePath));
            }

            if (string.IsNullOrEmpty(targetFilePath))
            {
                throw new ArgumentException("目标文件路径不能为空。", nameof(targetFilePath));
            }

            if (!System.IO.File.Exists(originalFilePath))
            {
                throw new FileNotFoundException($"原始文件未找到: {originalFilePath}", originalFilePath);
            }

            // 复制文件到缓存区，缓存区文件格式为 [文件名].[7位GUID].[拓展名]
            string shortGuid = Guid.NewGuid().ToString("N").Substring(0, 7);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFilePath);
            string extension = Path.GetExtension(originalFilePath);
            string tempFileName = $"{fileNameWithoutExt}.{shortGuid}{extension}";
            string tempFilePath = Path.Combine(TempFolderPath, tempFileName);

            Directory.CreateDirectory(TempFolderPath);
            System.IO.File.Copy(originalFilePath, tempFilePath, true);

            targetPathToTempPathMap.Add(targetFilePath, tempFilePath);
            return tempFilePath;
        }

        /// <summary>
        /// 将缓存区的全部文件移动到目标路径，然后清空缓存区文件
        /// </summary>
        /// <remarks>将会覆盖目标文件！</remarks>
        public void SaveAllFiles()
        {
            try
            {
                if (targetPathToTempPathMap.Count == 0)
                {
                    Debug.Log("没有任何暂存内容要保存");
                    return;
                }

                BiDirectionalDictionary<string, string> newMap =
                    new BiDirectionalDictionary<string, string>(targetPathToTempPathMap);
                foreach (var pair in newMap)
                {
                    string? folderPath = Path.GetDirectoryName(pair.Key);
                    if (string.IsNullOrEmpty(folderPath))
                    {
                        Debug.LogError("获取路径出错！");
                        return;
                    }

                    // 如果文件夹路径不存在，创建路径
                    Directory.CreateDirectory(folderPath);

                    // 如果目标文件存在，删除并移动临时文件
                    if (System.IO.File.Exists(pair.Key))
                    {
                        System.IO.File.Delete(pair.Key);
                    }

                    //TODO: 如果删除文件后异常导致无法移动，将导致数据丢失，考虑先移动后重命名或升级 .net 版本后的 Move() 方法重载
                    System.IO.File.Move(pair.Value, pair.Key);

                    // 从映射表中移除键值对
                    targetPathToTempPathMap.RemoveByKey(pair.Key);
                }
            }
            finally
            {
                if (targetPathToTempPathMap.Count == 0 && Directory.Exists(TempFolderPath))
                {
                    Directory.Delete(TempFolderPath, true);
                }
            }
        }

        /// <summary>
        /// 取消保存并清空缓存文件
        /// </summary>
        public void ClearTempFiles()
        {
            targetPathToTempPathMap.Clear();
            if (Directory.Exists(TempFolderPath))
            {
                Directory.Delete(TempFolderPath, true);
            }
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
    }
}
