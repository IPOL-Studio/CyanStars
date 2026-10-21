#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CyanStars.Utils;
using UnityEngine;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 应用路径管理器：解析并持有各类 <see cref="AppPathKind"/> 的默认路径、本次生效路径与玩家保存的覆盖配置
    /// </summary>
    /// <remarks>
    /// <para>配置固定保存在平台默认持久化数据目录下，不跟随被迁移的目录移动</para>
    /// <para>修改路径只写配置，本次运行已打开的路径不受影响，重启后才生效</para>
    /// <para>启动时保存的路径不可用则本次回退到默认路径，并记录原因供界面提示</para>
    /// </remarks>
    internal sealed class AppPathManager
    {
        /// <summary>
        /// 配置中是否保存了任何路径覆盖
        /// </summary>
        internal bool HasAnyAppPathOverride => Enum.GetValues(typeof(AppPathKind))
            .Cast<AppPathKind>()
            .Any(Settings.HasOverride);

        /// <summary>
        /// 本次运行是否有路径不在默认位置
        /// </summary>
        internal bool IsAnyAppPathOverrideActive => Enum.GetValues(typeof(AppPathKind))
            .Cast<AppPathKind>()
            .Any(kind => !PathUtil.PathEquals(GetAppPath(kind), GetDefaultAppPath(kind)));

        /// <summary>
        /// 平台环境
        /// </summary>
        private readonly IPlatformEnvironment Environment;

        /// <summary>
        /// 当前平台是否允许修改自定义路径
        /// </summary>
        private readonly bool CanCustomize;

        /// <summary>
        /// 当前平台的应用数据目录
        /// </summary>
        private string PersistentDataRoot => Environment.PersistentDataRoot;

        /// <summary>
        /// 自定义路径配置
        /// </summary>
        private readonly AppPathSettings Settings;

        private readonly Dictionary<AppPathKind, string> ActiveAppPaths =
            new Dictionary<AppPathKind, string>();

        private readonly Dictionary<AppPathKind, string> FallbackReasons =
            new Dictionary<AppPathKind, string>();


        /// <summary>
        /// 本次会话的临时目录名，位于临时文件目录下，校验路径时用于排除它
        /// </summary>
        private readonly string SessionFolderName;

        /// <summary>
        /// 本次会话的临时目录，跟随本次生效的临时文件目录（包括初始化失败后的回退）
        /// </summary>
        private string SessionFolderPath =>
            PathUtil.Combine(GetAppPath(AppPathKind.TemporaryCacheRoot), SessionFolderName);


        internal AppPathManager(IPlatformEnvironment environment, bool canCustomize, string sessionFolderName)
        {
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
            CanCustomize = canCustomize;

            if (string.IsNullOrEmpty(sessionFolderName))
                throw new ArgumentException("会话临时目录名为空", nameof(sessionFolderName));

            SessionFolderName = sessionFolderName;
            Settings = AppPathSettingsStore.Load(PersistentDataRoot);
            ResolveActiveAppPaths();
        }


        /// <summary>
        /// 获取平台默认路径
        /// </summary>
        internal string GetDefaultAppPath(AppPathKind kind)
        {
            return kind switch
            {
                AppPathKind.PlayerChartPacks =>
                    PathUtil.Combine(Environment.PersistentDataRoot, AppDirectoryNames.ChartPacks),
                AppPathKind.TemporaryCacheRoot => Environment.TemporaryCacheRoot,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }

        /// <summary>
        /// 本次运行实际使用的路径
        /// </summary>
        internal string GetAppPath(AppPathKind kind)
        {
            return ActiveAppPaths[kind];
        }

        /// <summary>
        /// 默认路径、保存的覆盖、本次生效路径与回退原因的完整状态
        /// </summary>
        internal AppPathInfo GetAppPathInfo(AppPathKind kind)
        {
            string? savedOverride = Settings.GetOverride(kind);
            bool isFallback = FallbackReasons.TryGetValue(kind, out string? fallbackReason);

            return new AppPathInfo(
                kind,
                kind.GetDisplayName(),
                GetDefaultAppPath(kind),
                savedOverride,
                GetAppPath(kind),
                isFallback,
                fallbackReason
            );
        }


        /// <summary>
        /// 本次回退到默认路径的条目及原因
        /// </summary>
        internal IReadOnlyList<AppPathInfo> GetUnavailableAppPaths()
        {
            var infos = new List<AppPathInfo>();
            foreach (AppPathKind kind in FallbackReasons.Keys)
            {
                infos.Add(GetAppPathInfo(kind));
            }

            return infos;
        }


        /// <summary>
        /// 校验路径能否作为指定用途的目录
        /// </summary>
        internal bool TryValidateAppPath(
            AppPathKind kind,
            string path,
            out string normalizedPath,
            out string error)
        {
            normalizedPath = "";

            if (!CanCustomize)
            {
                error = "当前平台不支持修改路径。";
                return false;
            }

            // 其它用途按"下次启动生效"的口径比较：已保存的覆盖优先于本次生效路径
            var otherPaths = new Dictionary<AppPathKind, string>();
            foreach (AppPathKind otherKind in Enum.GetValues(typeof(AppPathKind)))
            {
                if (otherKind == kind)
                    continue;

                otherPaths[otherKind] = Settings.GetOverride(otherKind) ?? GetAppPath(otherKind);
            }

            return TryValidateAppPathInternal(kind, path, otherPaths, out normalizedPath, out error);
        }

        /// <summary>
        /// 保存一个路径覆盖；与默认路径相同时转为清除覆盖
        /// </summary>
        internal AppPathChangeResult TrySetAppPath(AppPathKind kind, string path, out string error)
        {
            error = "";

            if (!CanCustomize)
            {
                error = "当前平台不支持修改路径。";
                return AppPathChangeResult.NotSupported;
            }

            if (!TryValidateAppPath(kind, path, out string normalizedPath, out error))
                return AppPathChangeResult.InvalidPath;

            if (PathUtil.PathEquals(normalizedPath, GetDefaultAppPath(kind)))
                return ResetAppPath(kind, out error);

            string? previousSavedPath = Settings.GetOverride(kind);
            if (previousSavedPath != null &&
                PathUtil.PathEquals(previousSavedPath, normalizedPath) &&
                !FallbackReasons.ContainsKey(kind))
            {
                return AppPathChangeResult.NoChange;
            }

            Settings.SetOverride(kind, normalizedPath);

            if (!AppPathSettingsStore.TrySave(PersistentDataRoot, Settings, out error))
            {
                if (previousSavedPath == null)
                    Settings.RemoveOverride(kind);
                else
                    Settings.SetOverride(kind, previousSavedPath);

                return AppPathChangeResult.SaveFailed;
            }

            FallbackReasons.Remove(kind);
            return PathUtil.PathEquals(GetAppPath(kind), normalizedPath)
                ? AppPathChangeResult.Applied
                : AppPathChangeResult.RequiresRestart;
        }

        /// <summary>
        /// 清除一个路径覆盖，恢复使用默认路径
        /// </summary>
        internal AppPathChangeResult ResetAppPath(AppPathKind kind, out string error)
        {
            error = "";

            if (!CanCustomize)
            {
                error = "当前平台不支持修改路径。";
                return AppPathChangeResult.NotSupported;
            }

            string? previousSavedPath = Settings.GetOverride(kind);
            if (previousSavedPath == null)
            {
                FallbackReasons.Remove(kind);
                return AppPathChangeResult.NoChange;
            }

            Settings.RemoveOverride(kind);

            if (!AppPathSettingsStore.TrySave(PersistentDataRoot, Settings, out error))
            {
                Settings.SetOverride(kind, previousSavedPath);
                return AppPathChangeResult.SaveFailed;
            }

            FallbackReasons.Remove(kind);
            return PathUtil.PathEquals(GetAppPath(kind), GetDefaultAppPath(kind))
                ? AppPathChangeResult.Applied
                : AppPathChangeResult.RequiresRestart;
        }

        /// <summary>
        /// 清除全部路径覆盖
        /// </summary>
        internal AppPathChangeResult ResetAllAppPaths(out string error)
        {
            error = "";

            if (!CanCustomize)
            {
                error = "当前平台不支持修改路径。";
                return AppPathChangeResult.NotSupported;
            }

            if (!HasAnyAppPathOverride)
            {
                FallbackReasons.Clear();
                return AppPathChangeResult.NoChange;
            }

            var previousOverrides = new Dictionary<string, string>(Settings.Overrides);
            Settings.Overrides.Clear();

            if (!AppPathSettingsStore.TrySave(PersistentDataRoot, Settings, out error))
            {
                Settings.Overrides = previousOverrides;
                return AppPathChangeResult.SaveFailed;
            }

            FallbackReasons.Clear();
            return IsAnyAppPathOverrideActive
                ? AppPathChangeResult.RequiresRestart
                : AppPathChangeResult.Applied;
        }

        /// <summary>
        /// 路径是否位于应用管理的目录之内（持久化数据目录或任一应用路径下）
        /// </summary>
        internal bool IsPathInManagedArea(string path)
        {
            if (PathUtil.IsSubPathOf(path, Environment.PersistentDataRoot))
                return true;

            foreach (AppPathKind kind in Enum.GetValues(typeof(AppPathKind)))
            {
                if (PathUtil.IsSubPathOf(path, GetAppPath(kind)))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 把临时文件目录回退到默认路径并记录原因
        /// </summary>
        /// <remarks>保存的覆盖不动，玩家下次启动仍可看到并处理自己的配置</remarks>
        internal void MarkTemporaryRootFallback(string error)
        {
            ActiveAppPaths[AppPathKind.TemporaryCacheRoot] =
                GetDefaultAppPath(AppPathKind.TemporaryCacheRoot);
            FallbackReasons[AppPathKind.TemporaryCacheRoot] = error;
        }


        /// <summary>
        /// 解析本次运行各用途实际使用的路径：默认路径为底，逐个套用校验通过的保存覆盖
        /// </summary>
        private void ResolveActiveAppPaths()
        {
            ActiveAppPaths.Clear();
            FallbackReasons.Clear();

            foreach (AppPathKind kind in Enum.GetValues(typeof(AppPathKind)))
            {
                ActiveAppPaths[kind] = GetDefaultAppPath(kind);
            }

            if (!CanCustomize)
                return;

            bool settingsChanged = false;

            foreach (AppPathKind kind in Enum.GetValues(typeof(AppPathKind)))
            {
                string? savedPath = Settings.GetOverride(kind);
                if (savedPath == null)
                    continue;

                var otherPaths = new Dictionary<AppPathKind, string>();
                foreach (AppPathKind otherKind in Enum.GetValues(typeof(AppPathKind)))
                {
                    if (otherKind != kind)
                        otherPaths[otherKind] = ActiveAppPaths[otherKind];
                }

                if (TryValidateAppPathInternal(kind, savedPath, otherPaths, out string normalizedPath, out string error))
                {
                    ActiveAppPaths[kind] = normalizedPath;

                    if (!PathUtil.PathEquals(normalizedPath, savedPath))
                    {
                        Settings.SetOverride(kind, normalizedPath);
                        settingsChanged = true;
                    }
                }
                else
                {
                    FallbackReasons[kind] = error;
                    Debug.LogWarning($"{kind.GetDisplayName()}设置不可用，本次已回退到默认路径：{error}");
                }
            }

            // 归一化修正过的配置立即落盘，避免磁盘上的配置与实际生效值长期不一致
            if (settingsChanged &&
                !AppPathSettingsStore.TrySave(PersistentDataRoot, Settings, out string saveError))
            {
                Debug.LogWarning($"归一化后的路径设置保存失败：{saveError}");
            }
        }

        private bool TryValidateAppPathInternal(
            AppPathKind kind,
            string path,
            IReadOnlyDictionary<AppPathKind, string> otherPaths,
            out string normalizedPath,
            out string error)
        {
            return AppPathValidator.TryValidate(
                kind,
                path,
                Environment.PersistentDataRoot,
                GetGameInstallRoot(),
                SessionFolderPath,
                otherPaths,
                out normalizedPath,
                out error
            );
        }

        /// <summary>
        /// 游戏安装根目录（Data 目录的上一级），编辑器下为工程根目录
        /// </summary>
        private static string? GetGameInstallRoot()
        {
            string? parentPath = Path.GetDirectoryName(PathUtil.Normalize(Application.dataPath));
            return string.IsNullOrEmpty(parentPath) ? null : PathUtil.Normalize(parentPath);
        }
    }
}
