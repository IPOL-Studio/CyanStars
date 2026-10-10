#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CyanStars.Utils;
using SimpleFileBrowser;
using UnityEngine;
using IOFile = System.IO.File;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件读写业务门户：把每一份参与读写的文件封装成 <see cref="FileHandle"/>，并负责它在磁盘上的搬运
    /// </summary>
    /// <remarks>
    /// <para>由 <see cref="FileManager"/> 在初始化时创建并注入平台环境，生命周期与 <see cref="FileManager"/> 一致。</para>
    /// <para>缓存路径不能作为加载来源和保存目标，两者都必须是玩家选中的外部位置或应用数据目录。</para>
    /// <para>游戏会在结束和下次启动时清理临时文件，但仍建议业务逻辑自行管理文件句柄或路径的生命周期，
    /// 以实现即用即弃，并避免单次会话内复用时路径名冲突。</para>
    /// <para>注意：即使进程存活时，游戏可能在磁盘空间不足等极端情况下清理掉 <see cref="GameSessionTempFolder"/> 下的临时文件，
    /// 建议业务逻辑对此提供降级回退。</para>
    /// </remarks>
    public sealed class FilePortal
    {
        // TODO: 文件操作依赖于 SFB 插件，后续考虑改用其他独立插件或自有实现

        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);

        /// <summary>
        /// 当前平台环境，由 FileManager 在创建本门户时注入
        /// </summary>
        public IPlatformEnvironment Environment { get; }

        /// <summary>
        /// 本次游戏会话的临时目录
        /// </summary>
        private readonly GameSessionTempFolder sessionTempFolder;


        public FilePortal(IPlatformEnvironment environment)
        {
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
            sessionTempFolder = new GameSessionTempFolder(environment.TemporaryCacheRoot);
        }


        /// <summary>
        /// 初始化门户：清理上次运行残留的会话临时目录，并占用本次会话的目录
        /// </summary>
        /// <remarks>初始化失败会直接抛出异常</remarks>
        public void Init()
        {
            sessionTempFolder.Init();
        }

        /// <summary>
        /// 结束文件门户并删除本次会话的临时目录
        /// </summary>
        /// <remarks>已经保存到目标位置的文件不受影响；删除失败只告警，残留目录会在下次启动时清理</remarks>
        public void Shutdown()
        {
            sessionTempFolder.DeleteSessionFolder();
        }


        #region --- 加载句柄 ---

        /// <summary>
        /// 加载一份已有的文件（夹）并创建句柄
        /// </summary>
        /// <param name="pathUri">外部路径，可以是普通绝对路径或安卓 <c>content://</c> 路径</param>
        /// <param name="cacheScope">缓存作用域标识，决定缓存副本放在会话临时目录的哪个子目录下，由业务侧自定义</param>
        /// <returns>失败时返回 null</returns>
        /// <remarks>
        /// <para>内容一律复制进会话缓存：外部原文件之后的变动不影响本次会话，安卓 SAF 选中的
        /// <c>content://</c> 路径也因此可以交给 <see cref="System.IO"/> 处理</para>
        /// <para>加载路径不能位于会话缓存内</para>
        /// </remarks>
        public FileHandle? TryLoadFile(string pathUri, string cacheScope)
        {
            if (string.IsNullOrEmpty(pathUri))
            {
                Debug.LogError("加载文件时传入的路径为空。");
                return null;
            }

            if (sessionTempFolder.IsInsideSessionFolder(pathUri))
            {
                Debug.LogError($"加载路径不能位于会话缓存内：{pathUri}");
                return null;
            }

            string entryName;

            if (FileBrowserHelpers.DirectoryExists(pathUri))
            {
                string? cacheFolderPath = TryCopyInFolderToCache(pathUri, cacheScope, out entryName);
                return cacheFolderPath == null ? null : new FileHandle(pathUri, entryName, cacheFolderPath);
            }

            if (!FileBrowserHelpers.FileExists(pathUri))
            {
                Debug.LogError($"要加载的文件（夹）不存在：{pathUri}");
                return null;
            }

            string? cacheFilePath = TryCopyInFileToCache(pathUri, cacheScope, out entryName);
            return cacheFilePath == null ? null : new FileHandle(pathUri, entryName, cacheFilePath);
        }

        #endregion

        #region --- 保存位置 ---

        /// <summary>
        /// 设置句柄的保存位置
        /// </summary>
        /// <param name="fileHandle">修改此文件句柄</param>
        /// <param name="targetPath">保存时写到的绝对路径（含文件名），可以是普通绝对路径或安卓 <c>content://</c> 路径；传 null 或空字符串表示解绑</param>
        /// <returns>是否已经按传入的路径完成设置</returns>
        /// <remarks>
        /// <para>保存路径不能位于会话缓存内</para>
        /// <para>保存路径也不能等于或位于可读写路径之内，否则保存时会自我嵌套，这里提前拒绝</para>
        /// </remarks>
        public bool TrySetSaveTarget(FileHandle fileHandle, string? targetPath)
        {
            if (fileHandle.State != FileHandleState.Available)
            {
                Debug.LogError($"句柄 {fileHandle.ReadablePath} 不是可用状态，无法修改。");
                return false;
            }

            if (string.IsNullOrEmpty(targetPath))
            {
                fileHandle.SetTargetPath(null);
                return true;
            }

            string normalizedTargetPath = PathUtil.Normalize(targetPath);

            if (sessionTempFolder.IsInsideSessionFolder(normalizedTargetPath))
            {
                Debug.LogError($"保存路径不能位于会话缓存内：{normalizedTargetPath}");
                return false;
            }

            if (PathUtil.IsSubPathOf(normalizedTargetPath, fileHandle.ReadablePath))
            {
                Debug.LogError($"保存路径不能等于或位于句柄可读写路径之内：{normalizedTargetPath}");
                return false;
            }

            fileHandle.SetTargetPath(normalizedTargetPath);
            return true;
        }

        /// <summary>
        /// 获取常用文件夹在当前平台上的路径
        /// </summary>
        /// <returns>该文件夹在当前平台上不可用时返回 null</returns>
        public string? GetCommonFolderPath(CommonFolders folder)
        {
            return Environment.GetCommonFolderPath(folder);
        }

        #endregion

        #region --- 保存与释放 ---

        /// <summary>
        /// 判断句柄当前是否可以读取
        /// </summary>
        /// <remarks>句柄已释放、或者临时缓存被系统清理掉时返回 false</remarks>
        public bool IsHandleReadable(FileHandle? fileHandle)
        {
            if (fileHandle == null || fileHandle.State == FileHandleState.Released)
                return false;

            return FileBrowserHelpers.FileExists(fileHandle.ReadablePath) ||
                FileBrowserHelpers.DirectoryExists(fileHandle.ReadablePath);
        }

        /// <summary>
        /// 把句柄指向的内容保存到它的目标路径
        /// </summary>
        /// <param name="fileHandle">保存此句柄</param>
        /// <param name="overwrite">允许覆盖目标路径原有的文件</param>
        /// <returns>是否保存成功</returns>
        /// <remarks>
        /// <para>尚未指定目标路径、句柄已释放、内容已经不在了都算失败，具体原因见日志</para>
        /// </remarks>
        public bool TrySaveToTarget(FileHandle fileHandle, bool overwrite = false)
        {
            if (fileHandle.State != FileHandleState.Available)
            {
                Debug.LogError($"句柄 {fileHandle.ReadablePath} 不是可用状态，无法保存。");
                return false;
            }

            if (string.IsNullOrEmpty(fileHandle.TargetPath))
            {
                Debug.LogError($"句柄 {fileHandle.ReadablePath} 尚未指定保存位置，无法保存。");
                return false;
            }

            if (!IsHandleReadable(fileHandle))
            {
                Debug.LogError($"句柄 {fileHandle.ReadablePath} 的内容已经不存在，无法保存到 {fileHandle.TargetPath}。");
                return false;
            }

            return CopyEntryToTarget(fileHandle, overwrite);
        }

        /// <summary>
        /// 批量保存句柄
        /// </summary>
        /// <param name="fileHandles">要保存的句柄集合</param>
        /// <param name="overwrite">允许覆盖目标路径原有的文件</param>
        /// <returns>是否全部保存成功</returns>
        /// <remarks>任一项失败都不中断其余项；调用方应自行决定要不要在失败后再写引用这些资源的元数据文件</remarks>
        public bool TrySaveAll(IEnumerable<FileHandle> fileHandles, bool overwrite = false)
        {
            if (fileHandles == null)
                throw new ArgumentNullException(nameof(fileHandles));

            bool allSucceed = true;

            foreach (FileHandle fileHandle in fileHandles)
            {
                if (!TrySaveToTarget(fileHandle, overwrite))
                    allSucceed = false;
            }

            return allSucceed;
        }

        /// <summary>
        /// 释放句柄：将句柄标记为已释放，如果存在缓存文件则一并删除
        /// </summary>
        /// <returns>是否释放成功</returns>
        /// <remarks>
        /// <para>已经释放过的句柄直接返回 false，不会重复删除</para>
        /// <para>句柄不持有外部文件，释放时只删除它的缓存副本</para>
        /// </remarks>
        public bool TryReleaseFile(FileHandle fileHandle)
        {
            if (fileHandle == null)
                throw new ArgumentNullException(nameof(fileHandle));

            if (fileHandle.State == FileHandleState.Released)
                return false;

            DeleteCacheEntry(fileHandle.ReadablePath);

            fileHandle.MarkReleased();
            return true;
        }

        /// <summary>
        /// 批量释放句柄
        /// </summary>
        /// <returns>成功释放的句柄数量</returns>
        public int TryReleaseAll(IEnumerable<FileHandle> fileHandles)
        {
            if (fileHandles == null)
                throw new ArgumentNullException(nameof(fileHandles));

            int releasedCount = 0;

            foreach (FileHandle fileHandle in fileHandles)
            {
                if (TryReleaseFile(fileHandle))
                    releasedCount++;
            }

            return releasedCount;
        }

        /// <summary>
        /// 删除某个缓存作用域的文件夹（只在它已经是空文件夹时删除）
        /// </summary>
        /// <param name="cacheScope">缓存作用域标识</param>
        /// <remarks>业务侧在用完一个缓存作用域后用它收尾；文件夹里还有别的缓存文件时什么都不做</remarks>
        public void ClearCacheScopeIfEmpty(string cacheScope)
        {
            sessionTempFolder.DeleteCacheFolderIfEmpty(cacheScope);
        }

        #endregion

        #region --- 文本写入 ---

        /// <summary>
        /// 尝试写入文本
        /// </summary>
        /// <param name="targetPath">目标路径，可以是普通绝对路径或安卓 <c>content://</c> 路径</param>
        /// <param name="text">要写入的内容</param>
        /// <returns>是否写入成功</returns>
        /// <remarks>
        /// <para>目标路径不能位于会话缓存内</para>
        /// </remarks>
        public bool TryWriteTextToPath(string targetPath, string text)
        {
            if (string.IsNullOrEmpty(targetPath))
            {
                Debug.LogError("写入文本时目标路径为空。");
                return false;
            }

            if (sessionTempFolder.IsInsideSessionFolder(targetPath))
            {
                Debug.LogError($"目标路径不能位于会话缓存内：{targetPath}");
                return false;
            }

            // TODO: 改为先写同目录临时文件再替换，避免写入中断在目标路径留下损坏文件
            try
            {
                if (Environment.RequiresStorageAccessFramework(targetPath))
                {
                    FileBrowserHelpers.WriteTextToFile(targetPath, text);
                    return true;
                }

                string? targetFolderPath = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetFolderPath))
                    Directory.CreateDirectory(targetFolderPath);

                // 固定 Utf8NoBom 编码，不用平台默认编码
                IOFile.WriteAllText(targetPath, text, Utf8NoBom);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"写入文件 {targetPath} 时出错：{e.Message}");
                return false;
            }
        }

        #endregion


        /// <summary>
        /// 把句柄当前可读位置的内容覆盖写进目标路径
        /// </summary>
        /// <remarks>
        /// <para>目标路径的文件夹不存在时会创建</para>
        /// <para>目录句柄且目标已存在时，先删除目标目录再复制，避免新旧内容混合</para>
        /// </remarks>
        private bool CopyEntryToTarget(FileHandle handle, bool overwrite)
        {
            string readablePath = handle.ReadablePath;
            string targetPath = handle.TargetPath!;

            try
            {
                if (FileBrowserHelpers.DirectoryExists(readablePath))
                {
                    if (FileBrowserHelpers.DirectoryExists(targetPath))
                    {
                        if (!overwrite)
                        {
                            Debug.LogWarning($"目标路径已存在，按要求不覆盖：{targetPath}");
                            return false;
                        }

                        // 覆盖按替换语义处理：先删除旧目录，避免新旧内容混合
                        FileBrowserHelpers.DeleteDirectory(targetPath);
                    }

                    FileBrowserHelpers.CopyDirectory(readablePath, targetPath);
                }
                else if (Environment.RequiresStorageAccessFramework(targetPath))
                {
                    if (!overwrite && FileBrowserHelpers.FileExists(targetPath))
                    {
                        Debug.LogWarning($"目标路径已存在，按要求不覆盖：{targetPath}");
                        return false;
                    }

                    FileBrowserHelpers.WriteBytesToFile(targetPath, IOFile.ReadAllBytes(readablePath));
                }
                else
                {
                    string? targetFolderPath = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(targetFolderPath))
                        Directory.CreateDirectory(targetFolderPath);

                    if (!overwrite && IOFile.Exists(targetPath))
                    {
                        Debug.LogWarning($"目标路径已存在，按要求不覆盖：{targetPath}");
                        return false;
                    }

                    IOFile.Copy(readablePath, targetPath, true);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"把 {readablePath} 保存到 {targetPath} 时出错：{e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 把外部文件复制进会话缓存
        /// </summary>
        /// <param name="sourcePath">外部文件路径</param>
        /// <param name="cacheScope">缓存作用域标识</param>
        /// <param name="entryName">来源文件名称；复制失败时为空字符串</param>
        /// <returns>缓存文件路径；失败时返回 null</returns>
        private string? TryCopyInFileToCache(string sourcePath, string cacheScope, out string entryName)
        {
            try
            {
                entryName = GetEntryName(sourcePath);
                string cacheFilePath = CreateUniqueCachePath(cacheScope, entryName);
                FileBrowserHelpers.CopyFile(sourcePath, cacheFilePath);

                return cacheFilePath;
            }
            catch (Exception e)
            {
                Debug.LogError($"把 {sourcePath} 复制进缓存时出错：{e.Message}");
                entryName = "";
                return null;
            }
        }

        /// <summary>
        /// 把外部文件夹整体复制进会话缓存
        /// </summary>
        /// <param name="sourcePath">外部文件夹路径</param>
        /// <param name="cacheScope">缓存作用域标识</param>
        /// <param name="entryName">来源文件夹名称；复制失败时为空字符串</param>
        /// <returns>缓存文件夹路径；失败时返回 null</returns>
        private string? TryCopyInFolderToCache(string sourcePath, string cacheScope, out string entryName)
        {
            try
            {
                entryName = GetEntryName(sourcePath);
                string cacheFolderPath = CreateUniqueCachePath(cacheScope, entryName);
                FileBrowserHelpers.CopyDirectory(sourcePath, cacheFolderPath);

                return cacheFolderPath;
            }
            catch (Exception e)
            {
                Debug.LogError($"把 {sourcePath} 复制进缓存时出错：{e.Message}");
                entryName = "";
                return null;
            }
        }

        /// <summary>
        /// 在缓存文件夹里生成一个未被占用的路径
        /// </summary>
        /// <param name="cacheScope">缓存作用域标识</param>
        /// <param name="fileName">原始文件名</param>
        /// <remarks>同名时依次追加 (1)、(2) 等后缀</remarks>
        private string CreateUniqueCachePath(string cacheScope, string fileName)
        {
            string cacheFolderPath = sessionTempFolder.EnsureCacheFolder(cacheScope);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);

            if (string.IsNullOrEmpty(fileNameWithoutExtension))
                fileNameWithoutExtension = "untitled";

            string candidatePath = PathUtil.Combine(cacheFolderPath, fileNameWithoutExtension + extension);

            for (int index = 1; IOFile.Exists(candidatePath) || Directory.Exists(candidatePath); index++)
            {
                candidatePath = PathUtil.Combine(
                    cacheFolderPath,
                    $"{fileNameWithoutExtension} ({index}){extension}"
                );
            }

            return candidatePath;
        }

        /// <summary>
        /// 获取文件（夹）名
        /// </summary>
        private static string GetEntryName(string path)
        {
            string name = FileBrowserHelpers.GetFilename(path);

            return string.IsNullOrEmpty(name) ? PathUtil.GetName(path) : name;
        }

        /// <summary>
        /// 删除一份缓存文件（夹）
        /// </summary>
        private static void DeleteCacheEntry(string cachePath)
        {
            try
            {
                if (IOFile.Exists(cachePath))
                {
                    IOFile.Delete(cachePath);
                    return;
                }

                if (Directory.Exists(cachePath))
                    Directory.Delete(cachePath, true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"删除缓存副本 {cachePath} 时出错：{e.Message}");
            }
        }
    }
}
