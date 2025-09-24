using System;
using System.Threading.Tasks;
using SFB;
using UnityEngine;

// 引入编辑器 API 命名空间
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_ANDROID || UNITY_IOS
using NativeFilePickerNamespace;
#endif


namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件管理器，提供跨平台的文件读取和保存对话框功能。
    /// </summary>
    public partial class FileManager : BaseManager
    {
        public override int Priority { get; }

        public override void OnInit()
        {
            Debug.Log("FileManager Initialized.");
        }

        public override void OnUpdate(float deltaTime)
        {
            // 文件管理器通常不需要在 Update 中执行操作
        }

        #region Public API

        /// <summary>
        /// 异步打开文件选择对话框，让用户选择一个或多个文件。
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="extensions">文件扩展名过滤器</param>
        /// <param name="multiselect">是否允许选择多个文件</param>
        /// <returns>返回一个包含所有选中文件完整路径的数组。如果用户取消操作，则返回 null。</returns>
        public Task<string[]> PickFileAsync(string title, ExtensionFilter[] extensions, bool multiselect = false)
        {
#if UNITY_EDITOR
            // Unity 编辑器环境
            string path = EditorUtility.OpenFilePanel(title, "", GetFilterSpecFromExtensions(extensions));
            return Task.FromResult(!string.IsNullOrEmpty(path) ? new[] { path } : null);
#elif UNITY_STANDALONE
            // 桌面平台 (Windows, macOS, Linux)
            string[] paths = StandaloneFileBrowser.OpenFilePanel(title, "", extensions, multiselect);
            return Task.FromResult(paths != null && paths.Length > 0 ? paths : null);
#elif UNITY_ANDROID || UNITY_IOS
            // 移动平台 (Android, iOS)
            // 注意：这里需要你根据所用的原生插件进行修改
            // 大多数移动端插件不支持复杂的 ExtensionFilter，而是使用 MIME 类型
            // 这里我们只处理单选文件的简单情况
            if (multiselect)
            {
                Debug.LogWarning("Mobile platform often doesn't support multi-selection in a simple way. Falling back to single selection.");
            }

            // 示例：将扩展名转换为 MIME 类型
            // string[] mimeTypes = ConvertExtensionsToMimeTypes(extensions);
            // return NativeFilePicker.PickFileAsync(mimeTypes); // 假设你的插件有这样的异步方法

            Debug.LogWarning("NativeFilePicker logic is not implemented. Please replace with your actual plugin API.");
            return Task.FromResult<string[]>(null); // 返回 null 作为占位符
#else
            // 其他不支持的平台
            Debug.LogWarning("File picking is not supported on this platform.");
            return Task.FromResult<string[]>(null);
#endif
        }

        /// <summary>
        /// 异步打开保存文件对话框，让用户选择一个保存路径。
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="defaultName">默认文件名</param>
        /// <param name="extensions">文件扩展名过滤器</param>
        /// <returns>返回用户选择的完整保存路径。如果用户取消操作，则返回 null。</returns>
        public Task<string> SaveFileAsync(string title, string defaultName, ExtensionFilter[] extensions)
        {
#if UNITY_EDITOR
            // Unity 编辑器环境
            string extension = extensions != null && extensions.Length > 0 ? extensions[0].Extensions[0] : "";
            string path = EditorUtility.SaveFilePanel(title, "", defaultName, extension);
            return Task.FromResult(!string.IsNullOrEmpty(path) ? path : null);
#elif UNITY_STANDALONE
            // 桌面平台
            string path = StandaloneFileBrowser.SaveFilePanel(title, "", defaultName, extensions);
            return Task.FromResult(!string.IsNullOrEmpty(path) ? path : null);
#elif UNITY_ANDROID || UNITY_IOS
            // 移动平台
            // 在移动端直接保存到任意路径通常是受限的或不推荐的。
            // 通常的做法是保存到应用的持久化目录，然后提供“导出”或“分享”功能。
            // 因此，这里我们不实现 SaveFilePanel 功能。
            Debug.LogError("Saving to an arbitrary path is not supported on mobile platforms. Consider saving to Application.persistentDataPath instead.");
            return Task.FromResult<string>(null);
#else
            // 其他不支持的平台
            Debug.LogWarning("File saving is not supported on this platform.");
            return Task.FromResult<string>(null);
#endif
        }

        /// <summary>
        /// 方便的组合方法：打开文件选择器并直接读取文件内容。
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="extensions">文件扩展名过滤器</param>
        /// <returns>返回文件的字节数组。如果操作失败或被取消，则返回 null。</returns>
        public async Task<byte[]> PickAndReadAllBytesAsync(string title, ExtensionFilter[] extensions)
        {
            var paths = await PickFileAsync(title, extensions, false);
            if (paths == null || paths.Length == 0)
            {
                return null; // 用户取消
            }

            string path = paths[0];
            try
            {
                // 在 .NET Standard 2.0 中，File.ReadAllBytesAsync 不可用，使用同步版本
                // 如果你的项目环境支持 .NET Standard 2.1+ 或 .NET 5+，可以使用 await File.ReadAllBytesAsync(path);
                return System.IO.File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read file at {path}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 方便的组合方法：打开保存文件对话框并直接写入数据。
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="defaultName">默认文件名</param>
        /// <param name="extensions">文件扩展名过滤器</param>
        /// <param name="data">要写入的字节数据</param>
        /// <returns>如果写入成功，返回 true；否则返回 false。</returns>
        public async Task<bool> PickAndSaveFileAsync(string title, string defaultName, ExtensionFilter[] extensions,
            byte[] data)
        {
            string path = await SaveFileAsync(title, defaultName, extensions);
            if (string.IsNullOrEmpty(path))
            {
                return false; // 用户取消
            }

            try
            {
                // 在 .NET Standard 2.0 中，File.WriteAllBytesAsync 不可用，使用同步版本
                // 如果你的项目环境支持 .NET Standard 2.1+ 或 .NET 5+，可以使用 await File.WriteAllBytesAsync(path, data);
                System.IO.File.WriteAllBytes(path, data);
                Debug.Log($"File successfully saved to {path}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save file to {path}: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Private Helpers

#if UNITY_EDITOR
        /// <summary>
        /// 将 SFB 的 ExtensionFilter 转换为 Unity Editor API 需要的格式。
        /// 例如: new ExtensionFilter("Image Files", "png", "jpg") -> "Image Files,png,jpg"
        /// </summary>
        private string GetFilterSpecFromExtensions(ExtensionFilter[] extensions)
        {
            if (extensions == null || extensions.Length == 0) return "";

            // EditorUtility.OpenFilePanel/SaveFilePanel 的过滤器格式比较简单，
            // 它不支持像 Windows 那样显示多个过滤器，所以我们只用第一个。
            var filter = extensions[0];
            return string.Join(",", filter.Extensions);
        }
#endif

        #endregion
    }
}
