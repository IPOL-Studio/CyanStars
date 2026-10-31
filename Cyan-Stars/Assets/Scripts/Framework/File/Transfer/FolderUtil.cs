#nullable enable

using System;
using System.IO;
using CyanStars.Framework.File.Platform;
using CyanStars.Utils;
using SimpleFileBrowser;
using UnityEngine;
using IOFile = System.IO.File;

namespace CyanStars.Framework.File.Transfer
{
    /// <summary>
    /// 整目录复制工具
    /// </summary>
    /// <remarks>本地路径使用 <see cref="System.IO"/>；SAF 路径由 <see cref="FileBrowserHelpers"/> 处理</remarks>
    internal static class FolderUtil
    {
        /// <summary>
        /// 复制时允许的最大递归深度
        /// </summary>
        private const int MaxCopyDepth = 16;


        /// <summary>
        /// 复制文件夹，目标已存在时追加序号后缀（仅本地路径）
        /// </summary>
        internal static bool TryCopyFolder(string sourceFolderPath, string targetFolderPath, out string copiedFolderPath)
        {
            return TryCopyFolder(
                null,
                sourceFolderPath,
                targetFolderPath,
                FolderCopyMode.UniqueName,
                out copiedFolderPath
            );
        }

        /// <summary>
        /// 复制文件夹
        /// </summary>
        /// <param name="environment">平台环境；为 null 时按本地路径处理</param>
        /// <param name="sourceFolderPath">源文件夹绝对路径</param>
        /// <param name="targetFolderPath">目标文件夹绝对路径</param>
        /// <param name="mode">目标已存在时的处理方式</param>
        /// <param name="copiedFolderPath">实际复制到的路径；失败时为空字符串</param>
        /// <returns>是否复制成功</returns>
        /// <remarks><see cref="FolderCopyMode.ReplaceExisting"/> 会先删除已有目标；复制失败时旧内容不会恢复。目标包含源目录时拒绝</remarks>
        public static bool TryCopyFolder(
            IPlatformEnvironment? environment,
            string sourceFolderPath,
            string targetFolderPath,
            FolderCopyMode mode,
            out string copiedFolderPath)
        {
            copiedFolderPath = "";

            if (string.IsNullOrEmpty(sourceFolderPath) || !FileBrowserHelpers.DirectoryExists(sourceFolderPath))
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

            if (mode == FolderCopyMode.ReplaceExisting &&
                PathUtil.IsSubPathOf(sourceFolderPath, targetFolderPath))
            {
                Debug.LogError($"覆盖复制文件夹的目标不能包含源目录：{targetFolderPath}");
                return false;
            }

            bool targetUsesSaf = environment != null &&
                !environment.IsLocalPath(targetFolderPath);

            if (targetUsesSaf)
                return TryCopyFolderToSafTarget(sourceFolderPath, targetFolderPath, mode, out copiedFolderPath);

            return TryCopyFolderToLocalTarget(
                environment,
                sourceFolderPath,
                targetFolderPath,
                mode,
                out copiedFolderPath
            );
        }


        private static bool TryCopyFolderToLocalTarget(
            IPlatformEnvironment? environment,
            string sourceFolderPath,
            string targetFolderPath,
            FolderCopyMode mode,
            out string copiedFolderPath)
        {
            copiedFolderPath = "";

            string resolvedTargetPath;
            switch (mode)
            {
                case FolderCopyMode.UniqueName:
                    resolvedTargetPath = CreateUniqueFolderPath(targetFolderPath);
                    break;

                case FolderCopyMode.FailIfExists:
                    if (Directory.Exists(targetFolderPath) || IOFile.Exists(targetFolderPath))
                    {
                        Debug.LogWarning($"目标路径已存在，按要求不覆盖：{targetFolderPath}");
                        return false;
                    }

                    resolvedTargetPath = targetFolderPath;
                    break;

                case FolderCopyMode.ReplaceExisting:
                    if (IOFile.Exists(targetFolderPath))
                    {
                        Debug.LogError($"目标路径是一个文件，无法作为文件夹复制目标：{targetFolderPath}");
                        return false;
                    }

                    resolvedTargetPath = targetFolderPath;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            try
            {
                if (mode == FolderCopyMode.ReplaceExisting && Directory.Exists(resolvedTargetPath))
                    Directory.Delete(resolvedTargetPath, true);

                bool sourceUsesSaf = environment != null &&
                    !environment.IsLocalPath(sourceFolderPath);

                if (sourceUsesSaf)
                    FileBrowserHelpers.CopyDirectory(sourceFolderPath, resolvedTargetPath);
                else
                    CopyFolderRecursively(sourceFolderPath, resolvedTargetPath, 1);

                copiedFolderPath = PathUtil.Normalize(resolvedTargetPath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"把文件夹 {sourceFolderPath} 复制到 {resolvedTargetPath} 时出错：{e.Message}");
                DeleteFolderIfExists(resolvedTargetPath);
                return false;
            }
        }

        private static bool TryCopyFolderToSafTarget(
            string sourceFolderPath,
            string targetFolderPath,
            FolderCopyMode mode,
            out string copiedFolderPath)
        {
            copiedFolderPath = "";
            string createdFolderPath = "";

            try
            {
                if (!TrySplitFolderPath(targetFolderPath, out string parentPath, out string targetName))
                {
                    Debug.LogError($"无法解析目标路径的父目录或名称：{targetFolderPath}");
                    return false;
                }

                // 插件对空目录和枚举失败都返回 null：按无同名项继续，真正的失败会在创建与复制阶段暴露
                FileSystemEntry[] entries = FileBrowserHelpers.GetEntriesInDirectory(parentPath, false)
                    ?? Array.Empty<FileSystemEntry>();

                FileSystemEntry? existingEntry = FindEntryByName(entries, targetName);

                if (existingEntry != null)
                {
                    switch (mode)
                    {
                        case FolderCopyMode.FailIfExists:
                            Debug.LogWarning($"目标路径已存在，按要求不覆盖：{targetFolderPath}");
                            return false;

                        case FolderCopyMode.ReplaceExisting:
                            if (!existingEntry.Value.IsDirectory)
                            {
                                Debug.LogError($"目标路径是一个文件，无法作为文件夹复制目标：{targetFolderPath}");
                                return false;
                            }

                            FileBrowserHelpers.DeleteDirectory(existingEntry.Value.Path);
                            break;

                        case FolderCopyMode.UniqueName:
                            targetName = CreateUniqueSafFolderName(entries, targetName);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                    }
                }

                createdFolderPath = FileBrowserHelpers.CreateFolderInDirectory(parentPath, targetName);
                if (string.IsNullOrEmpty(createdFolderPath))
                {
                    Debug.LogError($"无法创建目标文件夹：{targetFolderPath}");
                    return false;
                }

                FileBrowserHelpers.CopyDirectory(sourceFolderPath, createdFolderPath);
                copiedFolderPath = createdFolderPath;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"把文件夹 {sourceFolderPath} 复制到 {targetFolderPath} 时出错：{e.Message}");
                DeleteSafFolderIfExists(createdFolderPath);
                return false;
            }
        }


        /// <summary>
        /// 拆分文件夹路径的父目录与名称
        /// </summary>
        private static bool TrySplitFolderPath(string folderPath, out string parentPath, out string folderName)
        {
            string normalizedPath = PathUtil.Normalize(folderPath);
            int separatorIndex = normalizedPath.LastIndexOf('/');

            if (separatorIndex <= 0 || separatorIndex == normalizedPath.Length - 1)
            {
                parentPath = "";
                folderName = "";
                return false;
            }

            parentPath = normalizedPath[..separatorIndex];
            folderName = normalizedPath[(separatorIndex + 1)..];
            return true;
        }

        private static FileSystemEntry? FindEntryByName(FileSystemEntry[] entries, string name)
        {
            foreach (FileSystemEntry entry in entries)
            {
                if (string.Equals(entry.Name, name, StringComparison.Ordinal))
                    return entry;
            }

            return null;
        }

        private static string CreateUniqueSafFolderName(FileSystemEntry[] entries, string baseName)
        {
            for (int counter = 1; ; counter++)
            {
                string candidateName = $"{baseName} ({counter})";
                if (FindEntryByName(entries, candidateName) == null)
                    return candidateName;
            }
        }

        private static void DeleteSafFolderIfExists(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return;

            try
            {
                FileBrowserHelpers.DeleteDirectory(folderPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"删除文件夹 {folderPath} 时出错：{e.Message}");
            }
        }

        /// <summary>
        /// 目标路径已被占用（文件夹或同名文件）时，依次追加 (1)、(2) 等后缀取一个可用的路径
        /// </summary>
        private static string CreateUniqueFolderPath(string targetFolderPath)
        {
            if (!Directory.Exists(targetFolderPath) && !IOFile.Exists(targetFolderPath))
                return targetFolderPath;

            int counter = 1;
            string uniqueTargetPath;
            do
            {
                uniqueTargetPath = $"{targetFolderPath} ({counter})";
                counter++;
            } while (Directory.Exists(uniqueTargetPath) || IOFile.Exists(uniqueTargetPath));

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
