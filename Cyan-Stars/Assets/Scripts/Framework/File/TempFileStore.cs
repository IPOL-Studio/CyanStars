#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using CyanStars.Utils;
using UnityEngine;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 暂存作用域：把一批文件暂存在独占的缓存区文件夹里，并维护 暂存路径 → 句柄、目标路径 → 句柄 两张映射表
    /// </summary>
    /// <remarks>
    /// <para>让一个句柄改指到已被其它句柄占用的路径之前，必须先把原占用者 <c>Retarget(原占用者, null)</c> 解绑。</para>
    /// <para>落盘时必须传入当前仍然有效的目标路径集合（见 <see cref="ApplyAll"/> 的 <c>liveTargetPaths</c> 参数），否则已删除或已撤销的陈旧句柄会被一并写回磁盘。</para>
    /// </remarks>
    public sealed class TempFileStore : IDisposable
    {
        /// <summary>
        /// 所有暂存作用域文件夹的根目录
        /// </summary>
        private static string StagingRootPath =>
            PathUtil.Combine(Application.temporaryCachePath, nameof(TempFileStore));

        // 暂存路径 → 句柄，保证齐全
        private readonly Dictionary<string, StagedFileHandle> StagedPathToFileMap =
            new(StringComparer.Ordinal);

        // 目标路径 → 句柄，可能不齐全：暂存后尚未指定目标路径的句柄不在此表
        private readonly Dictionary<string, StagedFileHandle> TargetPathToFileMap =
            new(PathUtil.PathComparer);


        /// <summary>
        /// 缓存区文件夹绝对路径，按需创建
        /// </summary>
        public string FolderPath { get; }


        /// <summary>
        /// 在应用临时缓存目录下创建一个归本作用域独占的缓存区
        /// </summary>
        /// <param name="scopeName">作用域名，只用于辨认文件夹，将会附加随机后缀以保证唯一</param>
        public static TempFileStore CreateInTempCache(string scopeName)
        {
            if (string.IsNullOrEmpty(scopeName))
                throw new ArgumentNullException(nameof(scopeName));

            return new TempFileStore(PathUtil.Combine(StagingRootPath, $"{scopeName}.{CreateShortGuid()}"));
        }

        /// <summary>
        /// 删除根目录下所有缓存区，用于清理上次运行残留的文件
        /// </summary>
        public static void DeleteAllTempCaches()
        {
            DeleteFolderIfExists(StagingRootPath, "缓存区根目录");
        }

        /// <summary>
        /// 实例化缓存区
        /// </summary>
        /// <param name="folderPath">缓存区文件夹绝对路径，由 <see cref="CreateInTempCache"/> 保证落在暂存根目录下</param>
        /// <exception cref="ArgumentNullException">folderPath 为空</exception>
        private TempFileStore(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentNullException(nameof(folderPath));

            FolderPath = PathUtil.Normalize(folderPath);
        }


        /// <summary>
        /// 把外部文件复制进缓存区，并返回句柄
        /// </summary>
        /// <param name="originFilePath">外部文件的绝对路径（含后缀名），可以是普通路径或安卓 content:// 路径</param>
        /// <returns>暂存文件的句柄，此时它还没有目标路径</returns>
        /// <exception cref="ArgumentNullException">originFilePath 为空</exception>
        /// <exception cref="FileNotFoundException">外部文件不存在</exception>
        /// <exception cref="Exception">复制到缓存区失败</exception>
        /// <remarks>
        /// 本方法只把内容复制进缓存区，不修改任何目标路径；要保存到某个目标路径需另外调用 <see cref="Retarget"/>。
        /// </remarks>
        public IReadonlyStagedFileHandle Stage(string originFilePath)
        {
            if (string.IsNullOrEmpty(originFilePath))
                throw new ArgumentNullException(nameof(originFilePath));

            Directory.CreateDirectory(FolderPath);

            string fileName = FileManager.GetFileName(originFilePath);
            if (string.IsNullOrEmpty(fileName))
                throw new Exception($"无法获取文件名，可能是暂不支持此操作：{originFilePath}");

            string stagedFilePath = CreateUniqueFilePath(FolderPath, fileName);

            // 复制由 StagedFileHandle 的构造函数完成，失败会抛出异常
            StagedFileHandle stagedFileHandle = new StagedFileHandle(originFilePath, stagedFilePath);

            StagedPathToFileMap[stagedFileHandle.StagedFilePath] = stagedFileHandle;

            return stagedFileHandle;
        }

        /// <summary>
        /// 修改句柄要保存到的目标路径
        /// </summary>
        /// <param name="stagedFileHandle">本缓存区创建的句柄</param>
        /// <param name="targetFilePath">绝对目标路径（含后缀名），传 null 或空字符串表示解除映射</param>
        /// <returns>是否已经按传入的路径完成映射</returns>
        /// <remarks>
        /// <para>一个目标路径同时只能由一个句柄占用：目标路径已被别的句柄占用时本次调用不生效、返回 false，
        /// 调用方须先把原占用者 <c>Retarget(旧句柄, null)</c> 摘下来。</para>
        /// <para>同一句柄从旧路径改指新路径时会自动从旧路径摘下，不必先经 null 中转。</para>
        /// </remarks>
        public bool Retarget(IReadonlyStagedFileHandle stagedFileHandle, string? targetFilePath)
        {
            if (stagedFileHandle is not StagedFileHandle file || !IsOwnedFile(file))
            {
                Debug.LogError("这个句柄不是本缓存区创建的，无法更新它的目标文件路径！");
                return false;
            }

            if (file.State == StagedFileState.Released)
            {
                Debug.LogError("句柄已丢弃，无法更新它的目标文件路径！");
                return false;
            }

            targetFilePath = NormalizePath(targetFilePath);

            // 目标路径未变，直接返回成功，不修改映射表
            if (PathUtil.PathEquals(file.TargetFilePath, targetFilePath))
                return true;

            if (targetFilePath == null)
            {
                // 先摘映射再改句柄，避免中途出现映射表与句柄不一致的窗口
                if (file.TargetFilePath != null)
                    TargetPathToFileMap.Remove(file.TargetFilePath);

                file.MarkDetached();
                return true;
            }

            if (TargetPathToFileMap.TryGetValue(targetFilePath, out StagedFileHandle? owner) &&
                !ReferenceEquals(owner, file))
            {
                Debug.LogError($"目标路径 {targetFilePath} 已被暂存文件 {owner.StagedFilePath} 占用，" +
                               $"拒绝让 {file.StagedFilePath} 抢占。" +
                               "请先把原来的占用者 Retarget 到 null。");
                return false;
            }

            // 一个句柄同时只能占一个目标路径，改指新路径前先摘掉它的旧映射
            if (file.TargetFilePath != null)
                TargetPathToFileMap.Remove(file.TargetFilePath);

            TargetPathToFileMap[targetFilePath] = file;
            file.SetTargetFilePath(targetFilePath);
            return true;
        }

        /// <summary>
        /// 按应用数据路径找句柄
        /// </summary>
        /// <param name="targetPath">目标文件路径（含后缀名）</param>
        /// <returns>句柄，找不到返回 null</returns>
        public IReadonlyStagedFileHandle? FindByTargetPath(string targetPath)
        {
            if (string.IsNullOrEmpty(targetPath))
                return null;

            return TargetPathToFileMap.GetValueOrDefault(targetPath);
        }

        /// <summary>
        /// 取应用数据端最新可读的文件路径：有暂存副本就用暂存副本，没有就用传入的路径
        /// </summary>
        /// <param name="targetPath">应用数据端的文件路径</param>
        /// <returns>最新数据对应的路径；句柄已失效时退化为 <paramref name="targetPath"/></returns>
        public string ResolveLatest(string targetPath)
        {
            IReadonlyStagedFileHandle? handle = FindByTargetPath(targetPath);

            if (handle == null || handle.State == StagedFileState.Released)
                return targetPath;

            return handle.StagedFilePath;
        }

        /// <summary>
        /// 找出「当前数据仍然引用、但本次保存不会产生内容」的目标路径
        /// </summary>
        /// <param name="liveTargetPaths">当前仍然有效的目标路径集合</param>
        /// <returns>保存后仍然没有内容的目标路径，顺序不保证</returns>
        /// <remarks>
        /// 只做诊断，不修改任何状态；查到缺失后是否中止保存由调用方决定。
        /// </remarks>
        public List<string> CollectMissingTargets(HashSet<string> liveTargetPaths)
        {
            if (liveTargetPaths == null)
                throw new ArgumentNullException(nameof(liveTargetPaths));

            var missingTargets = new List<string>();

            foreach (string targetPath in liveTargetPaths)
            {
                // 磁盘上已有这个文件，本次不需要写它
                if (FileManager.FileExists(targetPath))
                    continue;

                IReadonlyStagedFileHandle? handle = FindByTargetPath(targetPath);

                // 句柄存在还不够，暂存副本本身可能已经不在了
                if (handle != null && handle.State == StagedFileState.Staged &&
                    FileManager.FileExists(handle.StagedFilePath))
                {
                    continue;
                }

                missingTargets.Add(targetPath);
            }

            return missingTargets;
        }

        /// <summary>
        /// 把暂存文件保存到各自的目标路径（缓存区 → 应用数据）
        /// </summary>
        /// <param name="liveTargetPaths">
        /// 当前仍然有效的目标路径集合，须按路径语义比较（见 <see cref="PathUtil.PathComparer"/>）。
        /// 只有集合内的句柄会写盘，集合外的句柄会被跳过并打日志
        /// </param>
        /// <param name="overwrite">允许覆盖目标路径原有的文件</param>
        /// <returns>是否全部保存成功</returns>
        /// <remarks>
        /// <para>句柄和映射在保存后保持不变，撤销/重做可以继续用同一份暂存副本。</para>
        /// <para>传 null 表示不做过滤，写出映射表里的所有文件。</para>
        /// </remarks>
        public bool ApplyAll(HashSet<string>? liveTargetPaths = null, bool overwrite = true)
        {
            bool allSucceed = true;

            // 先摘出本次要写的条目，避免写盘过程中修改映射表
            var pending = new List<StagedFileHandle>();

            foreach (StagedFileHandle file in TargetPathToFileMap.Values)
            {
                if (file.State == StagedFileState.Released)
                    continue;

                if (liveTargetPaths != null && !liveTargetPaths.Contains(file.TargetFilePath!))
                {
                    // 不属于当前数据的历史残留，通常由漏掉 Retarget 造成
                    Debug.LogWarning($"暂存文件 {file.StagedFilePath} 指向的 {file.TargetFilePath} " +
                                     "已不被当前数据引用，本次跳过写入。");
                    continue;
                }

                pending.Add(file);
            }

            foreach (StagedFileHandle file in pending)
            {
                if (!Apply(file, overwrite))
                    allSucceed = false;
            }

            return allSucceed;
        }

        /// <summary>
        /// 丢弃所有句柄和暂存文件，并删掉缓存区文件夹
        /// </summary>
        /// <remarks>已经保存到目标路径的文件不受影响；本方法可以重复调用</remarks>
        public void Discard()
        {
            foreach (StagedFileHandle file in StagedPathToFileMap.Values)
                file.MarkReleased();

            StagedPathToFileMap.Clear();
            TargetPathToFileMap.Clear();

            DeleteFolderIfExists(FolderPath, "缓存区文件夹");
        }

        public void Dispose()
        {
            Discard();
        }


        /// <summary>
        /// 把外部文件复制进缓存区文件夹，并返回复制后的路径
        /// </summary>
        /// <param name="sourcePath">源文件绝对路径，可以是普通路径或安卓 content:// 路径</param>
        /// <remarks>返回的路径不登记成句柄。</remarks>
        public string CopyInFile(string sourcePath)
        {
            return CopyIn(sourcePath, false);
        }

        /// <summary>
        /// 把外部文件夹复制进缓存区文件夹，并返回复制后的路径
        /// </summary>
        /// <param name="sourcePath">源文件夹绝对路径，可以是普通路径或安卓 content:// 路径</param>
        /// <remarks>返回的路径不登记成句柄。</remarks>
        public string CopyInFolder(string sourcePath)
        {
            return CopyIn(sourcePath, true);
        }

        private string CopyIn(string sourcePath, bool isFolder)
        {
            if (string.IsNullOrEmpty(sourcePath))
                throw new ArgumentNullException(nameof(sourcePath), "源路径为空");

            Directory.CreateDirectory(FolderPath);

            string fileName = FileManager.GetFileName(sourcePath);
            if (string.IsNullOrEmpty(fileName))
                throw new Exception($"无法获取文件（夹）名：{sourcePath}");

            string destinationPath = CreateUniqueFilePath(FolderPath, fileName);

            FileManager.CopyFileOrFolderUnchecked(sourcePath, destinationPath, isFolder);
            return destinationPath;
        }

        /// <summary>
        /// 判断某个句柄是不是本缓存区创建的
        /// </summary>
        private bool IsOwnedFile(StagedFileHandle fileHandle)
        {
            return StagedPathToFileMap.TryGetValue(fileHandle.StagedFilePath, out StagedFileHandle? mappedFile) &&
                ReferenceEquals(mappedFile, fileHandle);
        }

        /// <summary>
        /// 把句柄的暂存副本写到它的目标路径
        /// </summary>
        private static bool Apply(StagedFileHandle fileHandle, bool overwrite)
        {
            string targetFilePath = fileHandle.TargetFilePath!;

            try
            {
                string? targetFolder = Path.GetDirectoryName(targetFilePath);
                if (!string.IsNullOrEmpty(targetFolder))
                    Directory.CreateDirectory(targetFolder);

                return FileManager.CopyFileOrFolder(fileHandle.StagedFilePath, targetFilePath, false, overwrite);
            }
            catch (Exception e)
            {
                Debug.LogError($"保存暂存文件到 {targetFilePath} 时出错：{e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 空字符串按 null 处理
        /// </summary>
        private static string? NormalizePath(string? path)
        {
            return string.IsNullOrEmpty(path) ? null : PathUtil.Normalize(path);
        }

        /// <summary>
        /// 在文件夹中生成一个未被占用的路径，格式为 [文件名].[7位GUID].[拓展名]
        /// </summary>
        /// <remarks>不检查生成的路径是否已存在。</remarks>
        private static string CreateUniqueFilePath(string folderPath, string originFileName)
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originFileName);
            string extension = Path.GetExtension(originFileName);

            return PathUtil.Combine(folderPath, $"{fileNameWithoutExt}.{CreateShortGuid()}{extension}");
        }

        private static string CreateShortGuid()
        {
            return Guid.NewGuid().ToString("N")[..7];
        }

        private static void DeleteFolderIfExists(string folderPath, string description)
        {
            if (!Directory.Exists(folderPath))
                return;

            try
            {
                Directory.Delete(folderPath, true);
                Debug.Log($"已删除{description}：{folderPath}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"在删除{description}时捕获了异常：{e.Message}");
            }
        }
    }
}
