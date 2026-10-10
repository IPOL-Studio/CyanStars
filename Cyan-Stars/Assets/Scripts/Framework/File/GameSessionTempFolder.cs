#nullable enable

using System;
using System.IO;
using CyanStars.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;
using IOFile = System.IO.File;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 游戏会话的临时目录：应用临时数据目录下、归本次游戏进程独占的一层文件夹
    /// </summary>
    /// <remarks>
    /// <para>目录名固定为 <c>GameSession_{GUID}</c>，随游戏进程懒创建、退出时删除</para>
    /// <para>目录里的 <c>.session_lock</c> 由当前实例独占打开并一直持有到会话结束：
    /// 若此文件被占，则视为有一个多开副本在运行，不删除对应的文件夹。</para>
    /// <para>缓存按用途分目录隔离，用途标识（缓存作用域）由业务侧自定义，框架不枚举业务场景</para>
    /// </remarks>
    public sealed class GameSessionTempFolder
    {
        /// <summary>
        /// 会话目录的固定前缀
        /// </summary>
        private const string SessionFolderPrefix = "GameSession_";

        /// <summary>
        /// 会话占用标记文件名，被某个游戏实例独占打开即代表它的会话目录仍在使用
        /// </summary>
        private const string SessionLockFileName = ".session_lock";


        /// <summary>
        /// 应用临时数据目录的绝对路径
        /// </summary>
        private readonly string temporaryCacheRoot;

        /// <summary>
        /// 本次游戏会话的临时目录绝对路径
        /// </summary>
        public string SessionFolderPath { get; }

        /// <summary>
        /// 本进程是否已经建好会话目录
        /// </summary>
        private bool isInitialized;

        /// <summary>
        /// 本次会话持有的占用标记文件流
        /// </summary>
        /// <remarks>独占打开期间别的实例打不开这个文件，以此证明本次会话的目录还在使用</remarks>
        private FileStream? sessionLockStream;


        /// <summary>
        /// 创建会话临时目录对象
        /// </summary>
        /// <param name="temporaryCacheRoot">应用临时数据目录的绝对路径，由平台环境提供</param>
        public GameSessionTempFolder(string temporaryCacheRoot)
        {
            this.temporaryCacheRoot = temporaryCacheRoot;
            SessionFolderPath = PathUtil.Combine(
                temporaryCacheRoot,
                $"{SessionFolderPrefix}{Guid.NewGuid():N}"
            );
        }


        /// <summary>
        /// 清理上一次运行残留的会话目录，并占用本次会话的目录
        /// </summary>
        /// <remarks>
        /// <para>重复调用不会重复清理；初始化失败会直接抛出异常</para>
        /// <para>只清理应用临时数据目录下、带会话前缀、且 <c>.session_lock</c> 没有被任何实例独占打开的目录；
        /// 独占打开成功的目录属于另一个仍在运行的实例，将会保留</para>
        /// </remarks>
        public void Init()
        {
            if (isInitialized)
                return;

            DeleteLeftoverSessionFolders();
            CreateSessionFolder();
            isInitialized = true;
        }

        /// <summary>
        /// 判断路径是否位于本次会话的临时目录内
        /// </summary>
        /// <param name="path">待判断的绝对路径，可以是普通路径或安卓 <c>content://</c> 路径</param>
        /// <remarks>
        /// <para>会话缓存既不能作为加载来源，也不能作为保存目标，相关判断统一走这里</para>
        /// <para><c>content://</c> 这类不是本地文件系统的路径不会命中会话目录，返回 false</para>
        /// </remarks>
        public bool IsInsideSessionFolder(string path)
        {
            return PathUtil.IsSubPathOf(path, SessionFolderPath);
        }

        /// <summary>
        /// 取会话内某个缓存作用域的文件夹路径
        /// </summary>
        /// <param name="cacheScope">缓存作用域标识，由业务侧自定义，直接作为子文件夹名</param>
        /// <returns>缓存文件夹的绝对路径，可能尚未创建</returns>
        public string GetCacheFolderPath(string cacheScope)
        {
            if (string.IsNullOrEmpty(cacheScope))
                throw new ArgumentException("缓存作用域标识为空", nameof(cacheScope));

            return PathUtil.Combine(SessionFolderPath, cacheScope);
        }

        /// <summary>
        /// 确保缓存文件夹存在
        /// </summary>
        /// <param name="cacheScope">缓存作用域标识</param>
        /// <returns>缓存文件夹的绝对路径</returns>
        public string EnsureCacheFolder(string cacheScope)
        {
            string folderPath = GetCacheFolderPath(cacheScope);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return folderPath;
        }

        /// <summary>
        /// 删除缓存文件夹（只在它已经是空文件夹时删除）
        /// </summary>
        /// <param name="cacheScope">缓存作用域标识</param>
        /// <remarks>句柄全部释放后用它收尾；文件夹里还有别的缓存文件时什么都不做</remarks>
        public void DeleteCacheFolderIfEmpty(string cacheScope)
        {
            string folderPath = GetCacheFolderPath(cacheScope);

            try
            {
                if (!Directory.Exists(folderPath))
                    return;

                if (Directory.GetFileSystemEntries(folderPath).Length > 0)
                    return;

                Directory.Delete(folderPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"删除空的缓存文件夹 {folderPath} 时出错：{e.Message}");
            }
        }

        /// <summary>
        /// 删除整个会话目录，游戏进程正常退出时调用
        /// </summary>
        /// <remarks>删除失败只告警：残留目录会在下一次游戏会话开始时被清理掉；删除后可以在同一进程内再次 <see cref="Init"/></remarks>
        public void DeleteSessionFolder()
        {
            isInitialized = false;

            // 放开占用标记，之后别的实例清理这个目录时才能独占打开它
            ReleaseSessionLock();

            if (!Directory.Exists(SessionFolderPath))
                return;

            DeleteFolderIfExists(SessionFolderPath);
        }


        /// <summary>
        /// 建好会话目录并独占打开占用标记
        /// </summary>
        private void CreateSessionFolder()
        {
            // 目录名带本次会话的 GUID，正常不会有同名目录留下来，这里只是先清干净再建
            DeleteFolderIfExists(SessionFolderPath);

            Directory.CreateDirectory(SessionFolderPath);
            SetHiddenAttribute(SessionFolderPath);

            AcquireSessionLock();
        }

        /// <summary>
        /// 独占打开会话目录里的占用标记文件
        /// </summary>
        /// <remarks>
        /// <para>文件流必须一直持有到会话结束，别的实例才能通过独占打开失败发现这个目录仍在使用，
        /// 所以不能在这里关掉它，也不能换成允许共享的打开方式</para>
        /// <para>进程异常退出时文件流随进程一起消失，标记文件会留在磁盘上但不再被独占，
        /// 下一次启动就能把这个残留目录清理掉</para>
        /// </remarks>
        private void AcquireSessionLock()
        {
            string lockFilePath = PathUtil.Combine(SessionFolderPath, SessionLockFileName);

            sessionLockStream = new FileStream(
                lockFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );

            SetHiddenAttribute(lockFilePath);
        }

        /// <summary>
        /// 放开占用标记
        /// </summary>
        private void ReleaseSessionLock()
        {
            if (sessionLockStream == null)
                return;

            try
            {
                sessionLockStream.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"释放会话占用标记时出错：{e.Message}");
            }
            finally
            {
                sessionLockStream = null;
            }
        }

        /// <summary>
        /// 删除应用临时数据目录下所有未被占用的残留会话目录
        /// </summary>
        private void DeleteLeftoverSessionFolders()
        {
            // TODO: 多开时另一实例可能处于「目录已建、占用标记尚未持有」的窗口，这里会误删它的目录；
            // 后续在临时根目录加全局清理互斥锁
            if (!Directory.Exists(temporaryCacheRoot))
                return;

            string[] candidates;
            try
            {
                candidates = Directory.GetDirectories(temporaryCacheRoot, SessionFolderPrefix + "*", SearchOption.TopDirectoryOnly);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"枚举应用临时数据目录时出错，已跳过清理：{e.Message}");
                return;
            }

            foreach (string candidate in candidates)
            {
                if (PathUtil.PathEquals(candidate, SessionFolderPath) || IsSessionFolderInUse(candidate))
                    continue;

                Debug.Log($"清理上次运行残留的临时目录：{candidate}");
                DeleteFolderIfExists(candidate);
            }
        }

        /// <summary>
        /// 判断会话目录是否正被另一个游戏实例占用
        /// </summary>
        /// <remarks>
        /// <para>占用标记文件由使用方独占打开，所以这里只要能把它独占打开，就说明没有实例在用它，
        /// 目录是上一次运行留下的残留</para>
        /// <para>没有标记文件时，目录要么还没建好、要么标记已被清掉，同样按未占用处理</para>
        /// </remarks>
        private static bool IsSessionFolderInUse(string sessionFolderPath)
        {
            string lockFilePath = PathUtil.Combine(sessionFolderPath, SessionLockFileName);

            if (!IOFile.Exists(lockFilePath))
                return false;

            try
            {
                using FileStream probeStream = new FileStream(
                    lockFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None
                );

                return false;
            }
            catch (Exception)
            {
                // 独占打开失败说明文件正被另一个实例持有；其它异常（权限不足、目录正在被删等）也保守当作占用
                return true;
            }
        }

        private static void SetHiddenAttribute(string path)
        {
            try
            {
                IOFile.SetAttributes(path, IOFile.GetAttributes(path) | FileAttributes.Hidden);
            }
            catch (Exception)
            {
                // 个别平台（如非 Windows）不支持设置隐藏属性，或者路径不允许改属性，忽略即可
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
                Debug.LogWarning($"删除临时目录 {folderPath} 时出错：{e.Message}");
            }
        }
    }
}
