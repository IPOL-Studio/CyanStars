using UnityEngine;
using SimpleFileBrowser;
using System;
using System.Threading.Tasks;
using CatAsset.Runtime;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件管理器
    /// </summary>
    /// <remarks>提供统一的文件和文件夹选择对话框接口，基于 Unity Simple File Browser 实现</remarks>
    /// <remarks>提供对玩家文件的资源文件读写，.json 文件反序列化和序列化操作，其中读和反序列化操作将传给 AssetManager 实现</remarks>
    public class FileManager : BaseManager
    {
        [SerializeField]
        private UISkin skin;

        public readonly FileBrowser.Filter ChartFilter = new FileBrowser.Filter("谱面文件", ".json");
        public readonly FileBrowser.Filter SpriteFilter = new FileBrowser.Filter("图片", ".jpg", ".png");
        public readonly FileBrowser.Filter AudioFilter = new FileBrowser.Filter("音频", ".mp3", ".wav", ".ogg");


        public override int Priority { get; }

        /// <summary>
        /// 管理器初始化，在此处对 FileBrowser 进行全局配置
        /// </summary>
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

        #region --- Public API ---

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
