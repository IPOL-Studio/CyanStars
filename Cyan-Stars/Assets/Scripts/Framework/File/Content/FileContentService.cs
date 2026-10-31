#nullable enable

using System;
using SimpleFileBrowser;
using UnityEngine;

namespace CyanStars.Framework.File.Content
{
    /// <summary>
    /// 文件内容服务：提供不创建会话暂存副本的内容读取操作
    /// </summary>
    internal static class FileContentService
    {
        /// <summary>
        /// 尝试读取文件文本
        /// </summary>
        /// <param name="sourcePath">来源路径，可以是普通绝对路径或安卓 <c>content://</c> 路径</param>
        /// <param name="text">读取到的文本；失败时为空字符串</param>
        /// <returns>是否读取成功</returns>
        /// <remarks>普通本地路径直接读取；安卓 SAF 路径由 Simple File Browser 处理，不创建会话暂存副本</remarks>
        public static bool TryReadTextFromPath(string sourcePath, out string text)
        {
            text = "";

            if (string.IsNullOrEmpty(sourcePath))
            {
                Debug.LogError("读取文本时来源路径为空。");
                return false;
            }

            try
            {
                text = FileBrowserHelpers.ReadTextFromFile(sourcePath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"读取文件 {sourcePath} 时出错：{e.Message}");
                return false;
            }
        }
    }
}
