#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using CyanStars.Utils;
using UnityEngine;
using IOFile = System.IO.File;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 应用路径校验器：检查路径能否作为指定用途的目录
    /// </summary>
    /// <remarks>校验有副作用：通过前会在目标位置实际创建目录并写入探针文件，以确认可写</remarks>
    internal static class AppPathValidator
    {
        /// <summary>
        /// 校验路径；通过时 <paramref name="normalizedPath"/> 为归一化后的绝对路径，失败时 <paramref name="error"/> 为原因
        /// </summary>
        public static bool TryValidate(
            AppPathKind kind,
            string rawPath,
            string persistentDataRoot,
            string? gameInstallRoot,
            string? sessionFolderPath,
            IReadOnlyDictionary<AppPathKind, string> otherPaths,
            out string normalizedPath,
            out string error)
        {
            normalizedPath = "";
            error = "";

            if (string.IsNullOrWhiteSpace(rawPath))
            {
                error = "路径不能为空。";
                return false;
            }

            if (rawPath.Contains("://", StringComparison.Ordinal))
            {
                error = "只能选择本地文件夹。";
                return false;
            }

            if (!Path.IsPathRooted(rawPath))
            {
                error = "请选择绝对路径。";
                return false;
            }

            try
            {
                normalizedPath = PathUtil.Normalize(Path.GetFullPath(rawPath));
            }
            catch (Exception e)
            {
                error = $"路径格式不正确：{e.Message}";
                return false;
            }

            string? pathRoot = Path.GetPathRoot(normalizedPath);
            if (!string.IsNullOrEmpty(pathRoot) && PathUtil.PathEquals(normalizedPath, pathRoot))
            {
                error = "不能选择磁盘根目录，请选择它下面的文件夹。";
                return false;
            }

            if (PathUtil.PathEquals(normalizedPath, persistentDataRoot))
            {
                error = "不能直接使用应用数据根目录，请选择它下面的子目录。";
                return false;
            }

            if (kind == AppPathKind.PlayerChartPacks &&
                PathUtil.IsSubPathOf(persistentDataRoot, normalizedPath))
            {
                error = "不能选择应用数据目录的父目录，请选择其他位置。";
                return false;
            }

            if (!string.IsNullOrEmpty(gameInstallRoot) &&
                (PathUtil.IsSubPathOf(normalizedPath, gameInstallRoot) ||
                PathUtil.IsSubPathOf(gameInstallRoot, normalizedPath)))
            {
                error = "不能选择游戏安装目录或其父目录，请选择其他位置。";
                return false;
            }

            if (!string.IsNullOrEmpty(sessionFolderPath) &&
                PathUtil.IsSubPathOf(normalizedPath, sessionFolderPath))
            {
                error = "不能选择当前会话的临时目录。";
                return false;
            }

            foreach (KeyValuePair<AppPathKind, string> pair in otherPaths)
            {
                if (PathUtil.IsSubPathOf(normalizedPath, pair.Value) ||
                    PathUtil.IsSubPathOf(pair.Value, normalizedPath))
                {
                    error = $"不能与{pair.Key.GetDisplayName()}相同或互相包含。";
                    return false;
                }
            }

            if (kind == AppPathKind.TemporaryCacheRoot &&
                PathUtil.IsSubPathOf(normalizedPath, persistentDataRoot))
            {
                error = "临时文件目录不能放在应用数据目录内部。";
                return false;
            }

            if (IOFile.Exists(normalizedPath))
            {
                error = "目标路径是一个文件，请选择文件夹。";
                return false;
            }

            return TryEnsureDirectoryWritable(normalizedPath, out error);
        }


        /// <summary>
        /// 确认目录存在且可写：必要时创建目录，写入并删除一个探针文件
        /// </summary>
        private static bool TryEnsureDirectoryWritable(string folderPath, out string error)
        {
            error = "";
            string probeFilePath = PathUtil.Combine(folderPath, $".cys_write_test_{Guid.NewGuid():N}.tmp");
            bool createdDirectory = !Directory.Exists(folderPath);

            try
            {
                Directory.CreateDirectory(folderPath);
                IOFile.WriteAllBytes(probeFilePath, Array.Empty<byte>());
                IOFile.Delete(probeFilePath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                error = "没有权限在此文件夹写入文件。";
            }
            catch (DirectoryNotFoundException)
            {
                error = "目录不存在，或所在磁盘当前不可用。";
            }
            catch (DriveNotFoundException)
            {
                error = "所在磁盘当前不可用。";
            }
            catch (Exception e)
            {
                error = $"无法在此文件夹写入文件：{e.Message}";
            }

            DeleteFileIfExists(probeFilePath);

            if (createdDirectory)
                DeleteDirectoryIfEmpty(folderPath);

            return false;
        }

        private static void DeleteFileIfExists(string filePath)
        {
            try
            {
                if (IOFile.Exists(filePath))
                    IOFile.Delete(filePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"清理文件失败，可能遗留垃圾文件：{filePath} {e.Message}");
            }
        }

        private static void DeleteDirectoryIfEmpty(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath) &&
                    Directory.GetFileSystemEntries(folderPath).Length == 0)
                {
                    Directory.Delete(folderPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"清理路径失败，可能遗留垃圾路径：{folderPath} {e.Message}");
            }
        }
    }
}
