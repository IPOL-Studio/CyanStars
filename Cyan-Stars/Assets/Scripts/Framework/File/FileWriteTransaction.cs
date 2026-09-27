#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CyanStars.Utils;
using UnityEngine;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件级原子化写入：先把所有内容写进同目录的临时文件，再逐个顶替目标文件
    /// </summary>
    /// <remarks>
    /// <para>注意：多个文件之间没有跨文件原子性：顶替阶段中途失败，磁盘上会同时存在新内容和旧内容。
    /// 调用方须安排顶替顺序，先写被引用的资源文件，最后写引用它们的元数据文件。</para>
    /// </remarks>
    public sealed class FileWriteTransaction
    {
        // 项目专属的临时文件后缀，DeleteLeftoverTempFiles 按它精确匹配
        private const string TempFileExtension = ".cystmp";

        private readonly List<Entry> PendingEntries = new();

        /// <summary>
        /// 把内容写进临时文件，此时不修改目标文件
        /// </summary>
        /// <param name="targetFilePath">最终要生效的路径和文件全名，临时文件为它加 <c>.cystmp</c> 后缀</param>
        /// <param name="content">文件内容</param>
        /// <returns>是否准备成功</returns>
        // TODO: 改用异步
        public bool TryWriteText(string targetFilePath, string content)
        {
            if (string.IsNullOrEmpty(targetFilePath))
            {
                Debug.LogError("准备写入的文件路径为空。");
                return false;
            }

            string tempFilePath = targetFilePath + TempFileExtension;

            try
            {
                string? directory = Path.GetDirectoryName(targetFilePath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                // 固定写入无 BOM 的 UTF-8，不用平台默认编码
                System.IO.File.WriteAllText(tempFilePath, content, new UTF8Encoding(false));
            }
            catch (Exception e)
            {
                Debug.LogError($"准备写入 {targetFilePath} 时出现异常：{e}");
                return false;
            }

            PendingEntries.Add(new Entry(tempFilePath, targetFilePath));
            return true;
        }

        /// <summary>
        /// 把所有已准备的内容逐个顶替到目标路径
        /// </summary>
        /// <returns>是否全部顶替成功</returns>
        /// <remarks>
        /// 顶替顺序与 <see cref="TryWriteText"/> 的调用顺序一致；任一项失败不中断其余项，失败项会删除自己的临时文件，最后返回 false。
        /// </remarks>
        // TODO: 改用异步
        public bool CommitAll()
        {
            bool allSucceed = true;

            foreach (Entry entry in PendingEntries)
            {
                if (!TryCommit(entry))
                    allSucceed = false;
            }

            PendingEntries.Clear();
            return allSucceed;
        }

        /// <summary>
        /// 放弃本次事务，删掉所有已准备的临时文件
        /// </summary>
        public void Abort()
        {
            foreach (Entry entry in PendingEntries)
                TryDeleteTempFile(entry.TempFilePath);

            PendingEntries.Clear();
        }


        private static bool TryCommit(Entry entry)
        {
            try
            {
                ReplaceFile(entry.TempFilePath, entry.TargetFilePath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"写入文件 {entry.TargetFilePath} 时出现异常：{e}");
                TryDeleteTempFile(entry.TempFilePath);
                return false;
            }
        }

        /// <summary>
        /// 删除目录下上次写入残留的临时文件
        /// </summary>
        /// <param name="folderPath">要清理的目录，必须位于应用数据目录之内；目录不存在时直接返回 0</param>
        /// <returns>成功删除的文件个数</returns>
        /// <remarks>
        /// 只删除后缀为 <c>.cystmp</c> 的文件，且目录必须位于应用数据目录之内，否则不删除任何文件并返回 0。
        /// </remarks>
        // TODO: 改用异步
        public static int DeleteLeftoverTempFiles(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return 0;

            if (!IsInsideAppDataFolders(folderPath))
            {
                Debug.LogError($"拒绝清理应用数据目录之外的位置：{folderPath}");
                return 0;
            }

            if (!Directory.Exists(folderPath))
                return 0;

            int deletedCount = 0;

            foreach (string filePath in Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories))
            {
                // 用后缀精确判断：Windows 上通配符 "*"+后缀 会受 8.3 短文件名影响，可能连 "xxx.cystmp.json" 一起匹配
                if (!filePath.EndsWith(TempFileExtension, StringComparison.Ordinal))
                    continue;

                if (!TryDeleteTempFile(filePath))
                    continue;

                Debug.Log($"已清理上次写入残留的临时文件：{filePath}");
                deletedCount++;
            }

            return deletedCount;
        }


        /// <summary>
        /// 目录是否位于应用自己的数据目录之内（persistentDataPath / temporaryCachePath）
        /// </summary>
        private static bool IsInsideAppDataFolders(string folderPath)
        {
            // 先 GetFullPath 消掉相对路径和 ".."，再按路径语义比较包含关系
            string fullPath = Path.GetFullPath(folderPath);

            return PathUtil.IsSubPathOf(fullPath, Path.GetFullPath(Application.persistentDataPath)) ||
                PathUtil.IsSubPathOf(fullPath, Path.GetFullPath(Application.temporaryCachePath));
        }

        /// <summary>
        /// 用 tempFilePath 顶替 filePath，目标文件不会出现只写了一半的状态
        /// </summary>
        private static void ReplaceFile(string tempFilePath, string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                System.IO.File.Move(tempFilePath, filePath);
                return;
            }

            try
            {
                System.IO.File.Replace(tempFilePath, filePath, null);
            }
            catch (PlatformNotSupportedException)
            {
                // 个别平台没有实现 Replace，退化为覆盖式复制
                System.IO.File.Copy(tempFilePath, filePath, true);
                TryDeleteTempFile(tempFilePath);
            }
        }

        /// <summary>
        /// 删除临时文件，失败时只告警
        /// </summary>
        /// <returns>文件是否已经不在了（本来就不存在也算成功）</returns>
        // TODO: 改用异步
        private static bool TryDeleteTempFile(string tempFilePath)
        {
            try
            {
                if (System.IO.File.Exists(tempFilePath))
                    System.IO.File.Delete(tempFilePath);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"清理临时文件 {tempFilePath} 时出错：{e.Message}");
                return false;
            }
        }


        private readonly struct Entry
        {
            public readonly string TempFilePath;
            public readonly string TargetFilePath;

            public Entry(string tempFilePath, string targetFilePath)
            {
                TempFilePath = tempFilePath;
                TargetFilePath = targetFilePath;
            }
        }
    }
}
