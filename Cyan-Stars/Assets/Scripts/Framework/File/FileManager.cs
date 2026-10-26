#nullable enable

using System;
using System.Collections.Generic;
using CyanStars.Framework.File.AppPaths;
using CyanStars.Framework.File.Dialog;
using CyanStars.Framework.File.Platform;
using CyanStars.Framework.File.Staging;
using CyanStars.Framework.File.Transfer;
using CyanStars.Utils;
using SimpleFileBrowser;
using UnityEngine;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 文件交互入口：处理外部文件（夹）的选择、导入导出、应用路径解析与会话缓存句柄
    /// </summary>
    /// <remarks>应用自有目录由 <see cref="GetAppPath"/> 解析，保证为本地路径；
    /// 外部路径可能为 <c>content://</c>，相关读写应经本类完成</remarks>
    public class FileManager : BaseManager
    {
        [SerializeField]
        private UISkin fileBrowserSkin = null!;


        public override int Priority { get; }


        /// <summary>
        /// 当前平台环境
        /// </summary>
        public IPlatformEnvironment Environment { get; private set; } = null!;


        /// <summary>
        /// 文件句柄服务
        /// </summary>
        private FileHandleService fileHandleService = null!;

        /// <summary>
        /// 应用路径管理器
        /// </summary>
        private AppPathManager appPathManager = null!;

        /// <summary>
        /// 文件对话框服务
        /// </summary>
        private FileDialogService fileDialogService = null!;


        public override void OnInit()
        {
            Environment = PlatformEnvironmentFactory.Create();

            // 会话临时目录名在创建 AppPathManager 与 FileHandleService 前生成并共享给两者：
            // AppPathManager 校验路径时排除它，FileHandleService 实际占用它，临时目录回退后两边仍指向同一目录
            string sessionFolderName = GameSessionTempFolder.GenerateSessionFolderName();

            appPathManager = new AppPathManager(Environment, sessionFolderName);
            fileHandleService = CreateFileHandleService(sessionFolderName);
            fileDialogService = new FileDialogService(Environment);
            fileDialogService.Initialize(fileBrowserSkin);

            Debug.Log("FileManager Initialized.");
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        private void OnDestroy()
        {
            // 退出游戏时关闭文件句柄服务并删除本次会话的临时目录
            fileHandleService.Shutdown();
        }


        /// <summary>
        /// 按应用路径创建文件句柄服务；自定义临时目录初始化失败时回退到默认目录重试
        /// </summary>
        /// <param name="sessionFolderName">本次会话的临时目录名，与 AppPathManager 共享</param>
        private FileHandleService CreateFileHandleService(string sessionFolderName)
        {
            string temporaryRoot = GetAppPath(AppPathKind.TemporaryCacheRoot);
            var service = new FileHandleService(Environment, temporaryRoot, sessionFolderName);

            try
            {
                service.Init();
                return service;
            }
            catch (Exception e)
            {
                string defaultTemporaryRoot =
                    appPathManager.GetDefaultAppPath(AppPathKind.TemporaryCacheRoot);

                if (PathUtil.PathEquals(temporaryRoot, defaultTemporaryRoot))
                    throw;

                Debug.LogError($"临时文件目录不可用，本次已回退到默认目录：{temporaryRoot}\n{e.Message}");

                try
                {
                    service.Shutdown();
                }
                catch (Exception f)
                {
                    Debug.LogError($"关闭文件句柄服务时出错：{f}");
                    throw;
                }

                appPathManager.MarkTemporaryRootFallback(e.Message);

                service = new FileHandleService(Environment, defaultTemporaryRoot, sessionFolderName);
                service.Init();
                return service;
            }
        }


        #region --- 检查文件和路径存在性 ---

        /// <summary>
        /// 文件是否存在（支持普通路径和安卓 content:// 路径）
        /// </summary>
        public static bool IsFileExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return FileBrowserHelpers.FileExists(path);
        }

        /// <summary>
        /// 文件夹是否存在（支持普通路径和安卓 content:// 路径）
        /// </summary>
        public static bool IsFolderExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return FileBrowserHelpers.DirectoryExists(path);
        }

        /// <summary>
        /// 取文件（夹）名（支持普通路径和安卓 content:// 路径）
        /// </summary>
        public static string GetFileOrFolderName(string path)
        {
            return string.IsNullOrEmpty(path) ? "" : FileBrowserHelpers.GetFilename(path);
        }

        #endregion

        #region --- 获取/修改常用路径指向 ---

        /// <summary>
        /// 根据路径类型获取当前指向的真实路径
        /// </summary>
        public string GetAppPath(AppPathKind kind)
        {
            return appPathManager.GetAppPath(kind);
        }

        /// <summary>
        /// 校验文件夹导入或导出的源路径与目标路径
        /// </summary>
        /// <param name="sourceFolderPath">源文件夹绝对路径</param>
        /// <param name="targetFolderPath">目标文件夹绝对路径</param>
        /// <param name="direction">导入或导出</param>
        /// <param name="error">失败原因；校验通过时为空字符串</param>
        /// <returns>路径组合是否符合方向要求</returns>
        public bool TryValidateFolderTransfer(
            string sourceFolderPath,
            string targetFolderPath,
            FolderTransferDirection direction,
            out string error)
        {
            return FolderTransferPolicy.TryValidate(
                sourceFolderPath,
                targetFolderPath,
                direction,
                appPathManager.IsPathInManagedArea,
                out error
            );
        }

        // TODO: 后续实装路径迁移功能
        //
        // /// <summary>
        // /// 默认路径、保存的覆盖、本次生效路径与回退原因的完整状态
        // /// </summary>
        // public AppPathInfo GetAppPathInfo(AppPathKind kind)
        // {
        //     return appPathManager.GetAppPathInfo(kind);
        // }
        //
        // /// <summary>
        // /// 本次回退到默认路径的条目及原因
        // /// </summary>
        // public IReadOnlyList<AppPathInfo> GetUnavailableAppPaths()
        // {
        //     return appPathManager.GetUnavailableAppPaths();
        // }
        //
        // /// <summary>
        // /// 配置中是否保存了任何路径覆盖
        // /// </summary>
        // public bool HasAnyAppPathOverride => appPathManager.HasAnyAppPathOverride;
        //
        // /// <summary>
        // /// 本次运行是否有路径不在默认位置
        // /// </summary>
        // public bool IsAnyAppPathOverrideActive => appPathManager.IsAnyAppPathOverrideActive;
        //
        // /// <summary>
        // /// 校验路径能否作为指定用途的目录
        // /// </summary>
        // /// <param name="kind">路径用途</param>
        // /// <param name="path">待校验的绝对路径</param>
        // /// <param name="normalizedPath">归一化后的路径；校验失败时为空字符串</param>
        // /// <param name="error">失败原因；校验通过时为空字符串</param>
        // public bool TryValidateAppPath(
        //     AppPathKind kind,
        //     string path,
        //     out string normalizedPath,
        //     out string error)
        // {
        //     return appPathManager.TryValidateAppPath(kind, path, out normalizedPath, out error);
        // }
        //
        // /// <summary>
        // /// 保存一个路径覆盖，重启游戏后生效；与默认路径相同时转为清除覆盖
        // /// </summary>
        // /// <param name="kind">路径用途</param>
        // /// <param name="path">新的目录绝对路径</param>
        // /// <param name="error">失败原因；未失败时为空字符串</param>
        // /// <remarks>只写配置，不改变本次运行已经打开的路径；不会移动旧目录中的内容</remarks>
        // public AppPathChangeResult TrySetAppPath(AppPathKind kind, string path, out string error)
        // {
        //     return appPathManager.TrySetAppPath(kind, path, out error);
        // }
        //
        // /// <summary>
        // /// 清除一个路径覆盖，恢复使用默认路径
        // /// </summary>
        // public AppPathChangeResult ResetAppPath(AppPathKind kind, out string error)
        // {
        //     return appPathManager.ResetAppPath(kind, out error);
        // }
        //
        // /// <summary>
        // /// 清除全部路径覆盖
        // /// </summary>
        // public AppPathChangeResult ResetAllAppPaths(out string error)
        // {
        //     return appPathManager.ResetAllAppPaths(out error);
        // }

        #endregion

        #region --- 文件（夹）路径对话框 ---

        /// <summary>
        /// 打开文件选择对话框
        /// </summary>
        /// <param name="onSuccess">成功获取的回调，参数为归一化后的玩家选中的文件路径</param>
        /// <param name="onCancel">取消时的回调</param>
        /// <param name="title">弹窗标题</param>
        /// <param name="showAllFilesFilter">是否允许玩家选择任意后缀的文件</param>
        /// <param name="filters">依据后缀筛选文件</param>
        /// <param name="defaultFilter">默认后缀过滤器的显示名</param>
        /// <returns>是否成功打开；已有对话框打开时返回 false，并触发 <paramref name="onCancel"/></returns>
        public bool TryOpenLoadFilePathBrowser(
            Action<string>? onSuccess,
            Action? onCancel = null,
            string title = "打开文件",
            bool showAllFilesFilter = false,
            FileTypeFilter[]? filters = null,
            string? defaultFilter = null)
        {
            return fileDialogService.TryOpenLoadFilePathBrowser(
                onSuccess,
                onCancel,
                title,
                showAllFilesFilter,
                filters,
                defaultFilter
            );
        }

        /// <summary>
        /// 打开文件夹选择对话框
        /// </summary>
        /// <param name="onSuccess">成功获取的回调，参数为归一化后的玩家选中的文件夹路径</param>
        /// <param name="onCancel">取消后的回调</param>
        /// <param name="title">弹窗标题</param>
        /// <returns>是否成功打开；已有对话框打开时返回 false，并触发 <paramref name="onCancel"/></returns>
        public bool TryOpenLoadFolderPathBrowser(
            Action<string>? onSuccess,
            Action? onCancel = null,
            string title = "打开文件夹")
        {
            return fileDialogService.TryOpenLoadFolderPathBrowser(onSuccess, onCancel, title);
        }

        /// <summary>
        /// 打开文件夹保存对话框
        /// </summary>
        /// <param name="onSuccess">成功获取的回调，参数为归一化后的文件夹路径</param>
        /// <param name="onCancel">取消后的回调</param>
        /// <param name="title">弹窗标题</param>
        /// <returns>是否成功打开；已有对话框打开时返回 false，并触发 <paramref name="onCancel"/></returns>
        public bool TryOpenSaveFolderPathBrowser(
            Action<string>? onSuccess,
            Action? onCancel = null,
            string title = "保存到")
        {
            return fileDialogService.TryOpenSaveFolderPathBrowser(onSuccess, onCancel, title);
        }

        #endregion

        #region --- 外部文件夹导入导出 ---

        /// <summary>
        /// 将外部文件夹复制到应用自有目录
        /// </summary>
        /// <param name="sourceFolderPath">外部文件夹路径，可以是本地绝对路径或安卓 <c>content://</c> 路径</param>
        /// <param name="targetFolderPath">目标绝对路径；目标已存在时追加序号后缀</param>
        /// <param name="copiedFolderPath">实际复制到的路径；失败时为空字符串</param>
        /// <param name="error">平台不支持或路径校验失败时的原因；复制阶段失败时为空字符串，原因见日志</param>
        /// <returns>是否复制成功</returns>
        /// <remarks>平台不支持导入或路径组合不符合导入要求时返回 false，并记录原因</remarks>
        public bool TryImportFolder(
            string sourceFolderPath,
            string targetFolderPath,
            out string copiedFolderPath,
            out string error)
        {
            copiedFolderPath = "";
            error = "";

            if (!Environment.CanImportExternalFolder)
            {
                error = "当前平台不支持导入外部文件夹。";
                Debug.LogWarning(error);
                return false;
            }

            if (!TryValidateFolderTransfer(
                    sourceFolderPath,
                    targetFolderPath,
                    FolderTransferDirection.Import,
                    out error))
            {
                Debug.LogError(error);
                return false;
            }

            return FolderUtil.TryCopyFolder(
                Environment,
                sourceFolderPath,
                targetFolderPath,
                FolderCopyMode.UniqueName,
                out copiedFolderPath
            );
        }

        /// <summary>
        /// 将应用自有目录的文件夹复制到外部位置
        /// </summary>
        /// <param name="sourceFolderPath">应用自有目录内的源绝对路径</param>
        /// <param name="targetFolderPath">外部目标绝对路径；目标已存在时追加序号后缀</param>
        /// <param name="copiedFolderPath">实际复制到的路径；失败时为空字符串</param>
        /// <param name="error">平台不支持或路径校验失败时的原因；复制阶段失败时为空字符串，原因见日志</param>
        /// <returns>是否复制成功</returns>
        /// <remarks>平台不支持导出或路径组合不符合导出要求时返回 false，并记录原因</remarks>
        public bool TryExportFolder(
            string sourceFolderPath,
            string targetFolderPath,
            out string copiedFolderPath,
            out string error)
        {
            copiedFolderPath = "";
            error = "";

            if (!Environment.CanExportFolder)
            {
                error = "当前平台不支持导出文件夹。";
                Debug.LogWarning(error);
                return false;
            }

            if (!TryValidateFolderTransfer(
                    sourceFolderPath,
                    targetFolderPath,
                    FolderTransferDirection.Export,
                    out error))
            {
                Debug.LogError(error);
                return false;
            }

            return FolderUtil.TryCopyFolder(
                Environment,
                sourceFolderPath,
                targetFolderPath,
                FolderCopyMode.UniqueName,
                out copiedFolderPath
            );
        }

        #endregion

        #region --- 文件句柄读写 ---

        /// <summary>
        /// 暂存一份已有的文件（夹）并创建句柄
        /// </summary>
        /// <param name="pathUri">外部路径，可以是普通绝对路径或安卓 <c>content://</c> 路径</param>
        /// <param name="cacheScope">缓存作用域标识，决定缓存副本放在会话临时目录的哪个子目录下，由业务侧自定义</param>
        /// <returns>文件句柄；失败时返回 null</returns>
        /// <remarks>内容一律复制进会话缓存，来源路径不能位于会话缓存内</remarks>
        public FileHandle? TryStageFile(string pathUri, string cacheScope)
        {
            return fileHandleService.TryStageFile(pathUri, cacheScope);
        }

        /// <summary>
        /// 设置句柄的保存位置
        /// </summary>
        /// <param name="fileHandle">修改此文件句柄</param>
        /// <param name="targetPath">保存时写到的绝对路径（含文件名）；传 null 或空字符串表示解绑</param>
        /// <returns>是否已经按传入的路径完成设置</returns>
        public bool TrySetSaveTarget(FileHandle fileHandle, string? targetPath)
        {
            return fileHandleService.TrySetSaveTarget(fileHandle, targetPath);
        }

        /// <summary>
        /// 判断句柄当前是否可以读取
        /// </summary>
        /// <param name="fileHandle">待判断的句柄</param>
        /// <remarks>句柄已释放、或者临时缓存被系统清理掉时返回 false</remarks>
        public bool IsHandleReadable(FileHandle? fileHandle)
        {
            return fileHandleService.IsHandleReadable(fileHandle);
        }

        /// <summary>
        /// 把句柄指向的内容保存到它的目标路径
        /// </summary>
        /// <param name="fileHandle">保存此句柄</param>
        /// <param name="overwrite">允许覆盖目标路径原有的文件</param>
        /// <returns>是否保存成功</returns>
        public bool TrySaveToTarget(FileHandle fileHandle, bool overwrite = false)
        {
            return fileHandleService.TrySaveToTarget(fileHandle, overwrite);
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
            return fileHandleService.TrySaveAll(fileHandles, overwrite);
        }

        /// <summary>
        /// 释放句柄：将句柄标记为已释放，一并删除缓存文件
        /// </summary>
        /// <param name="fileHandle">要释放的句柄</param>
        /// <returns>是否释放成功；已经释放过的句柄返回 false</returns>
        public bool TryReleaseFile(FileHandle fileHandle)
        {
            return fileHandleService.TryReleaseFile(fileHandle);
        }

        /// <summary>
        /// 批量释放句柄
        /// </summary>
        /// <param name="fileHandles">要释放的句柄集合</param>
        /// <returns>成功释放的句柄数量</returns>
        public int TryReleaseAll(IEnumerable<FileHandle> fileHandles)
        {
            return fileHandleService.TryReleaseAll(fileHandles);
        }

        /// <summary>
        /// 删除某个缓存作用域的文件夹（只在它已经是空文件夹时删除）
        /// </summary>
        /// <param name="cacheScope">缓存作用域标识</param>
        /// <remarks>业务侧在用完一个缓存作用域后用它收尾；文件夹里还有别的缓存文件时什么都不做</remarks>
        public void ClearCacheScopeIfEmpty(string cacheScope)
        {
            fileHandleService.ClearCacheScopeIfEmpty(cacheScope);
        }

        /// <summary>
        /// 尝试向文件中写入文本
        /// </summary>
        /// <param name="targetPath">目标路径，可以是普通绝对路径或安卓 <c>content://</c> 路径</param>
        /// <param name="text">要写入的内容</param>
        /// <returns>是否写入成功</returns>
        /// <remarks>目标路径不能位于会话缓存内</remarks>
        public bool TryWriteTextToPath(string targetPath, string text)
        {
            return fileHandleService.TryWriteTextToPath(targetPath, text);
        }

        #endregion

    }
}
