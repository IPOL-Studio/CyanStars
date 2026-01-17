#nullable enable

using UnityEngine;
using SimpleFileBrowser;
using System;
using System.Collections.Generic;
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


        public readonly FileBrowser.Filter ChartFilter = new FileBrowser.Filter("谱面文件", ".json");
        public readonly FileBrowser.Filter SpriteFilter = new FileBrowser.Filter("图片", ".jpg", ".png");
        public readonly FileBrowser.Filter AudioFilter = new FileBrowser.Filter("音频", ".ogg");

        private static string TempFolderPath => PathUtil.Combine(Application.persistentDataPath, "TempSession", "FileManager");
        private readonly List<TempFileHandler> TempFileHandlers = new();
        private readonly Dictionary<string, TempFileHandler> TempPathToHandlerMap = new(); // 缓存路径->句柄 映射表，一定是齐全的
        private readonly Dictionary<string, TempFileHandler> TargetPathToHandlerMap = new(); // 目标路径->句柄 映射表，不一定齐全（文件缓存了但没有指定映射路径，用于制谱器可撤销操作时缓存文件）


        public override void OnInit()
        {
            // 清理旧的缓存路径
            try
            {
                Directory.Delete(TempFolderPath, true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"在删除缓存文件时捕获了异常：{e.Message}");
            }

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
                    onSuccess?.Invoke(paths[0]);
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
                    onSuccess?.Invoke(paths[0]);
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

        #region --- TempFiles API 用于文件缓存 ---

        public enum TempFileHandlerState
        {
            Saved = 0, // 文件副本已保存到目标路径
            Temped = 1, // 文件副本已保存到缓存路径，但未保存/未能成功保存到目标路径
            Unavailable = 2 // 句柄已释放/实例化出错，缓存文件可能不存在，相关属性可能不再有效
        }

        /// <summary>
        /// 可缓存的文件的句柄（只读接口）
        /// </summary>
        public interface IReadonlyTempFileHandler
        {
            public TempFileHandlerState State { get; }
            public string OriginFilePath { get; }
            public string? TempFilePath { get; }
            public string? TargetFilePath { get; }
        }

        /// <summary>
        /// 可缓存的文件的句柄
        /// </summary>
        public class TempFileHandler : IReadonlyTempFileHandler, IDisposable
        {
            public TempFileHandlerState State { get; private set; }
            public string OriginFilePath { get; }
            public string? TempFilePath { get; }

            private string? targetFilePath;

            public string? TargetFilePath
            {
                get => targetFilePath;
                set
                {
                    if (string.IsNullOrEmpty(value)) // 将空字符串视为 null
                        value = null;
                    if (targetFilePath == value)
                        return;
                    State = TempFileHandlerState.Temped;
                    targetFilePath = value;
                }
            }

            /// <summary>
            /// 缓存文件并实例化句柄
            /// </summary>
            /// <param name="originFilePath">要保存的原始文件绝对路径</param>
            /// <param name="tempFolder">缓存文件夹绝对路径</param>
            /// <param name="targetFilePath">目标文件绝对路径，后续可以修改</param>
            public TempFileHandler(string originFilePath, string tempFolder, string? targetFilePath)
            {
                OriginFilePath = originFilePath;
                TargetFilePath = targetFilePath;

                if (string.IsNullOrEmpty(originFilePath) || !System.IO.File.Exists(originFilePath))
                {
                    Debug.LogWarning("给定的原始文件不存在");
                    State = TempFileHandlerState.Unavailable;
                    TempFilePath = null;
                    return;
                }

                try
                {
                    // 创建路径
                    if (!Directory.Exists(tempFolder))
                        Directory.CreateDirectory(tempFolder);

                    // 生成一个不重复的 7 位 GUID，然后复制文件到缓存区，缓存区文件格式为 [文件名].[7位GUID].[拓展名]
                    string tempFilePath;
                    while (true)
                    {
                        string shortGuid = Guid.NewGuid().ToString("N")[..7];
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originFilePath);
                        string extension = Path.GetExtension(originFilePath);
                        string tempFileName = $"{fileNameWithoutExt}.{shortGuid}{extension}";
                        tempFilePath = PathUtil.Combine(tempFolder, tempFileName);

                        if (!System.IO.File.Exists(tempFilePath))
                            break;
                    }

                    System.IO.File.Copy(originFilePath, tempFilePath);

                    State = TempFileHandlerState.Temped;
                    TempFilePath = tempFilePath;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"未能成功复制文件到临时目录：{e.Message}");
                    throw;
                }
            }

            public bool Save(bool releaseAfterSucceedSave = false)
            {
                if (string.IsNullOrEmpty(TempFilePath) || string.IsNullOrEmpty(TargetFilePath))
                {
                    Debug.LogError("临时文件或目标文件路径为空，无法保存！");
                    return false;
                }

                if (State == TempFileHandlerState.Unavailable)
                {
                    Debug.LogError("句柄已卸载，无法保存！");
                    return false;
                }

                // 覆盖目标文件路径
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(TargetFilePath)); // 如果目标文件夹路径不存在，创建路径
                    System.IO.File.Copy(TempFilePath, TargetFilePath!, true);
                }
                catch (Exception e)
                {
                    Debug.LogError($"覆盖目标文件时出错：{e.Message}");
                    return false;
                }

                State = TempFileHandlerState.Saved;

                if (releaseAfterSucceedSave)
                    Dispose();

                return true;
            }

            public void Dispose()
            {
                if (State == TempFileHandlerState.Unavailable)
                {
                    // 已经释放，无需再次释放
                    return;
                }

                if (string.IsNullOrEmpty(TempFilePath))
                {
                    Debug.LogError("临时文件或目标文件路径为空，无法释放！");
                    return;
                }

                try
                {
                    // 删除临时文件
                    System.IO.File.Delete(TempFilePath);
                }
                catch (Exception e)
                {
                    Debug.LogError($"删除临时文件时出错：{e.Message}");
                    return;
                }

                State = TempFileHandlerState.Unavailable;
            }
        }

        /// <summary>
        /// 将原始文件复制到缓存区
        /// </summary>
        /// <param name="originFilePath">原始文件的绝对路径（含后缀名）</param>
        /// <param name="targetFilePath">目标文件的绝对路径（含后缀名）</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">originFilePath 为空</exception>
        /// <exception cref="Exception">在缓存文件时出错</exception>
        public IReadonlyTempFileHandler TempFile(string originFilePath, string? targetFilePath = null)
        {
            if (string.IsNullOrEmpty(originFilePath))
                throw new ArgumentNullException(nameof(originFilePath));
            if (string.IsNullOrEmpty(targetFilePath))
                targetFilePath = null;

            var handler = new TempFileHandler(originFilePath, TempFolderPath, targetFilePath);
            if (handler.State == TempFileHandlerState.Unavailable || handler.TempFilePath == null)
                throw new Exception("缓存文件时出错");

            TempPathToHandlerMap[handler.TempFilePath] = handler;

            if (handler.TargetFilePath != null)
                TargetPathToHandlerMap[handler.TargetFilePath] = handler;

            return handler;
        }

        /// <summary>
        /// 更新句柄的目标文件路径和映射表记录
        /// </summary>
        /// <param name="handler">句柄</param>
        /// <param name="targetFilePath">绝对目标路径</param>
        /// <remarks>如果传入的路径不为 null，且原先有其他指向此目标路径的句柄，原有句柄的目标路径将被设为 null</remarks>
        public void UpdateTargetFilePath(TempFileHandler handler, string? targetFilePath)
        {
            if (handler.State == TempFileHandlerState.Unavailable)
            {
                Debug.LogWarning("句柄已卸载，无法更新其目标文件路径！");
                return;
            }

            if (handler.TargetFilePath == targetFilePath)
                return;

            if (targetFilePath != null && string.IsNullOrEmpty(targetFilePath))
                targetFilePath = null;

            // 如果有其他句柄指向此新路径，应该先更新其他句柄
            if (targetFilePath != null && TargetPathToHandlerMap.TryGetValue(targetFilePath, out TempFileHandler? otherHandler))
                otherHandler.TargetFilePath = null;

            // 清除旧路径映射
            if (!string.IsNullOrEmpty(handler.TargetFilePath))
                TargetPathToHandlerMap.Remove(handler.TargetFilePath);

            // 更新句柄路径和映射表
            handler.TargetFilePath = targetFilePath;
            if (targetFilePath != null)
                TargetPathToHandlerMap[targetFilePath] = handler;
        }

        // /// <summary>
        // /// 将缓存区的全部文件移动到目标路径，然后清空缓存区文件
        // /// </summary>
        // /// <param name="deleteTempFileAfterSucceedSave">保存成功后是否删除临时文件并从映射表中移除？目标文件为 null 时视为失败，失败时永不删除</param>
        // /// <remarks>将会覆盖目标文件！</remarks>
        // public void SaveAllFiles(bool deleteTempFileAfterSucceedSave = false)
        // {
        //     if (TempTargetFileMap.Count == 0)
        //     {
        //         Debug.Log("没有任何暂存内容要保存");
        //         return;
        //     }
        //
        //     Dictionary<string, string?> tempMap = new Dictionary<string, string?>(TempTargetFileMap); // 确保内部元素删除不影响 foreach
        //     int successCount = 0;
        //     int failCount = 0;
        //     int skipCount = 0;
        //     foreach (var kvp in tempMap)
        //     {
        //         if (kvp.Value == null)
        //         {
        //             skipCount++;
        //             continue;
        //         }
        //
        //
        //         if (SaveFile(kvp.Key, deleteTempFileAfterSucceedSave, out _))
        //             successCount++;
        //         else
        //             failCount++;
        //     }
        //
        //     Debug.Log($"批量保存完成，成功{successCount}个，失败{failCount}个，未指定保存路径而跳过{skipCount}个。是否删除成功保存的缓存文件：{deleteTempFileAfterSucceedSave}。");
        // }
        //
        // /// <summary>
        // /// 将指定的临时文件保存
        // /// </summary>
        // /// <param name="tempFilePath">临时文件绝地路径</param>
        // /// <param name="deleteTempFileAfterSucceedSave">保存成功后是否删除临时文件并从映射表中移除？目标文件为 null 时视为失败，失败时永不删除</param>
        // /// <param name="toggleFilePath">目标文件路径</param>
        // /// <returns>是否成功保存</returns>
        // public bool SaveFile(string tempFilePath, bool deleteTempFileAfterSucceedSave, out string? toggleFilePath)
        // {
        //     toggleFilePath = null;
        //
        //     if (string.IsNullOrEmpty(tempFilePath))
        //     {
        //         Debug.LogError("临时文件路径为空！");
        //         return false;
        //     }
        //
        //     if (!TempTargetFileMap.ContainsKey(tempFilePath))
        //     {
        //         Debug.LogError("无法获取目标文件路径，传入的不是在映射表中的临时文件路径或临时文件已删除？");
        //         return false;
        //     }
        //
        //     string? toggleFolderPath = Path.GetDirectoryName(toggleFilePath);
        //     if (string.IsNullOrEmpty(toggleFolderPath))
        //     {
        //         Debug.LogWarning($"{tempFilePath}对应的目标文件路径为空，将跳过保存...");
        //         return false;
        //     }
        //
        //     // 覆盖目标文件路径
        //     try
        //     {
        //         Directory.CreateDirectory(toggleFolderPath); // 如果目标文件夹路径不存在，创建路径
        //         System.IO.File.Copy(tempFilePath, toggleFilePath!, true);
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogError($"覆盖目标文件时出错：{e.Message}");
        //         return false;
        //     }
        //
        //     // 删除临时文件并从映射表中移除
        //     if (deleteTempFileAfterSucceedSave)
        //     {
        //         try
        //         {
        //             // 删除文件并从映射表中移除键值对
        //             System.IO.File.Delete(tempFilePath);
        //             TempTargetFileMap.Remove(tempFilePath);
        //         }
        //         catch (Exception e)
        //         {
        //             Debug.LogError($"删除临时文件时出错：{e.Message}");
        //             return false;
        //         }
        //
        //         // 完成删除后，如果映射表为空，则清理临时文件路径并舍弃不在映射表中的其他文件
        //         if (TempTargetFileMap.Count == 0 && Directory.Exists(TempFolderPath))
        //         {
        //             Directory.Delete(TempFolderPath, true);
        //         }
        //     }
        //
        //     return true;
        // }
        //
        // /// <summary>
        // /// 取消保存并清空缓存文件
        // /// </summary>
        // public void ClearAllTempFiles()
        // {
        //     TempTargetFileMap.Clear();
        //     if (Directory.Exists(TempFolderPath))
        //     {
        //         Directory.Delete(TempFolderPath, true);
        //     }
        // }
        //
        // /// <summary>
        // /// 尝试取消临时文件的映射并删除文件
        // /// </summary>
        // /// <param name="tempFilePath">临时文件绝对路径</param>
        // /// <returns>是否找到并移除了文件</returns>
        // public bool TryClearTempFile(string tempFilePath)
        // {
        //     if (!TempTargetFileMap.ContainsKey(tempFilePath))
        //     {
        //         return false;
        //     }
        //
        //     System.IO.File.Delete(tempFilePath);
        //     TempTargetFileMap.Remove(tempFilePath);
        //
        //     if (TempTargetFileMap.Count == 0 && Directory.Exists(TempFolderPath))
        //     {
        //         Directory.Delete(TempFolderPath, true);
        //     }
        //
        //     return true;
        // }

        #endregion
    }
}
