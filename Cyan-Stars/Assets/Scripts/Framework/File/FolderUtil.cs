#nullable enable

using System;
using System.IO;
using CyanStars.Utils;
using UnityEngine;
using IOFile = System.IO.File;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件夹工具：整目录复制
    /// </summary>
    /// <remarks>仅处理普通本地绝对路径，不支持安卓 <c>content://</c> 路径</remarks>
    public static class FolderUtil
    {
        /// <summary>
        /// 复制时允许的最大递归深度
        /// </summary>
        private const int MaxCopyDepth = 16;


        /// <summary>
        /// 把源文件夹整体复制到目标位置
        /// </summary>
        /// <param name="sourceFolderPath">源文件夹绝对路径</param>
        /// <param name="targetFolderPath">目标文件夹绝对路径</param>
        /// <param name="copiedFolderPath">实际复制到的文件夹绝对路径；复制失败时为空字符串</param>
        /// <returns>是否复制成功</returns>
        /// <remarks>
        /// <para>目标已存在时依次追加 (1)、(2) 等后缀</para>
        /// <para>目标等于或位于源之内时拒绝，避免复制时自我嵌套</para>
        /// <para>失败时清理未复制完成的目录</para>
        /// </remarks>
        public static bool TryCopyFolder(string sourceFolderPath, string targetFolderPath, out string copiedFolderPath)
        {
            copiedFolderPath = "";

            if (string.IsNullOrEmpty(sourceFolderPath) || !Directory.Exists(sourceFolderPath))
            {
                Debug.LogError($"要复制的源文件夹不存在：{sourceFolderPath}");
                return false;
            }

            if (string.IsNullOrEmpty(targetFolderPath))
            {
                Debug.LogError("复制文件夹时目标路径为空。");
                return false;
            }

            if (PathUtil.IsSubPathOf(targetFolderPath, sourceFolderPath))
            {
                Debug.LogError($"复制文件夹的目标不能等于或位于源之内：{targetFolderPath}");
                return false;
            }

            string uniqueTargetPath = CreateUniqueFolderPath(targetFolderPath);

            try
            {
                CopyFolderRecursively(sourceFolderPath, uniqueTargetPath, 1);
                copiedFolderPath = uniqueTargetPath;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"把文件夹 {sourceFolderPath} 复制到 {uniqueTargetPath} 时出错：{e.Message}");
                DeleteFolderIfExists(uniqueTargetPath);
                return false;
            }
        }

        /// <summary>
        /// 目标路径已被占用时，依次追加 (1)、(2) 等后缀取一个可用的路径
        /// </summary>
        private static string CreateUniqueFolderPath(string targetFolderPath)
        {
            if (!Directory.Exists(targetFolderPath))
                return targetFolderPath;

            int counter = 1;
            string uniqueTargetPath;

            do
            {
                uniqueTargetPath = $"{targetFolderPath} ({counter})";
                counter++;
            } while (Directory.Exists(uniqueTargetPath));

            return uniqueTargetPath;
        }

        /// <summary>
        /// 递归复制文件夹内容
        /// </summary>
        /// <param name="sourceFolderPath">源文件夹绝对路径</param>
        /// <param name="targetFolderPath">目标文件夹绝对路径，不存在时会创建</param>
        /// <param name="currentDepth">当前递归深度，从 1 开始计数</param>
        /// <remarks>深度超过上限时抛异常，避免符号链接成环时无限递归</remarks>
        private static void CopyFolderRecursively(string sourceFolderPath, string targetFolderPath, int currentDepth)
        {
            if (currentDepth > MaxCopyDepth)
                throw new Exception("复制文件夹时递归超过最大深度");

            Directory.CreateDirectory(targetFolderPath);

            foreach (string filePath in Directory.EnumerateFiles(sourceFolderPath))
            {
                string targetFilePath = PathUtil.Combine(targetFolderPath, PathUtil.GetName(filePath));
                IOFile.Copy(filePath, targetFilePath, true);
            }

            foreach (string subFolderPath in Directory.EnumerateDirectories(sourceFolderPath))
            {
                string targetSubFolderPath = PathUtil.Combine(targetFolderPath, PathUtil.GetName(subFolderPath));
                CopyFolderRecursively(subFolderPath, targetSubFolderPath, currentDepth + 1);
            }
        }

        /// <summary>
        /// 删除文件夹（存在时）
        /// </summary>
        private static void DeleteFolderIfExists(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                    Directory.Delete(folderPath, true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"删除文件夹 {folderPath} 时出错：{e.Message}");
            }
        }
    }
}
