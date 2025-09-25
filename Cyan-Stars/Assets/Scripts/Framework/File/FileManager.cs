using UnityEngine;
using SimpleFileBrowser;
using System;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件管理器，基于 Unity Simple File Browser 实现
    /// 提供统一的文件和文件夹选择对话框接口
    /// </summary>
    public class FileManager : BaseManager
    {
        // BaseManager 的优先级，可以根据你的框架需求调整
        public override int Priority => 0;

        /// <summary>
        /// 管理器初始化，在此处对 FileBrowser 进行全局配置
        /// </summary>
        public override void OnInit()
        {
            Debug.Log("FileManager Initialized.");

            // 1. 设置文件过滤器 (全局设置，只需一次)
            // 参数1: 是否显示 "All Files (*.*)" 选项
            FileBrowser.SetFilters(true,
                new FileBrowser.Filter("谱面文件", ".json", ".csv"),
                new FileBrowser.Filter("图片", ".jpg", ".png"),
                new FileBrowser.Filter("音频", ".mp3", ".wav", ".ogg"),
                new FileBrowser.Filter("文本文件", ".txt")
            );

            // 2. 设置默认选中的过滤器
            FileBrowser.SetDefaultFilter(".json");

            // 3. 设置需要排除显示的文件扩展名 (默认已排除 .lnk, .tmp)
            FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

            // 4. 添加快速链接 (例如：游戏存档目录、桌面)
            // 这对于让玩家快速定位到常用目录非常有用
            FileBrowser.AddQuickLink("游戏数据目录", Application.persistentDataPath, null);
            FileBrowser.AddQuickLink("桌面", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), null);
        }

        /// <summary>
        /// 每帧更新，此管理器不需要在 Update 中执行逻辑
        /// </summary>
        public override void OnUpdate(float deltaTime)
        {
            // FileBrowser 内部会处理自己的 Update 逻辑，这里无需操作
        }

        #region --- Public API ---

        /// <summary>
        /// 显示加载单个文件的对话框
        /// </summary>
        /// <param name="onSuccess">成功回调，返回选择的文件路径</param>
        /// <param name="onCancel">取消回调</param>
        /// <param name="title">对话框标题</param>
        public void ShowLoadFileDialog(Action<string> onSuccess, Action onCancel = null, string title = "加载文件")
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

            FileBrowser.ShowLoadDialog(successWrapper, cancelWrapper, FileBrowser.PickMode.Files, false, null, null, title, "选择");
        }

        /// <summary>
        /// 显示加载多个文件的对话框
        /// </summary>
        /// <param name="onSuccess">成功回调，返回选择的文件路径数组</param>
        /// <param name="onCancel">取消回调</param>
        /// <param name="title">对话框标题</param>
        public void ShowLoadFilesDialog(Action<string[]> onSuccess, Action onCancel = null, string title = "加载多个文件")
        {
            if (IsBrowserOpen()) return;

            FileBrowser.OnSuccess successWrapper = (paths) =>
            {
                onSuccess?.Invoke(paths);
            };

            FileBrowser.OnCancel cancelWrapper = onCancel != null ? new FileBrowser.OnCancel(onCancel) : null;

            FileBrowser.ShowLoadDialog(successWrapper, cancelWrapper, FileBrowser.PickMode.Files, true, null, null, title, "选择");
        }

        /// <summary>
        /// 显示选择文件夹的对话框
        /// </summary>
        /// <param name="onSuccess">成功回调，返回选择的文件夹路径</param>
        /// <param name="onCancel">取消回调</param>
        /// <param name="title">对话框标题</param>
        public void ShowSelectFolderDialog(Action<string> onSuccess, Action onCancel = null, string title = "选择文件夹")
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

            FileBrowser.ShowLoadDialog(successWrapper, cancelWrapper, FileBrowser.PickMode.Folders, false, null, null, title, "选择");
        }

        /// <summary>
        /// 显示保存文件的对话框
        /// </summary>
        /// <param name="onSuccess">成功回调，返回用户指定的保存路径</param>
        /// <param name="onCancel">取消回调</param>
        /// <param name="defaultFilename">默认文件名</param>
        /// <param name="title">对话框标题</param>
        public void ShowSaveFileDialog(Action<string> onSuccess, Action onCancel = null, string defaultFilename = "NewFile.txt", string title = "保存文件")
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

            FileBrowser.ShowSaveDialog(successWrapper, cancelWrapper, FileBrowser.PickMode.Files, false, null, defaultFilename, title, "保存");
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
