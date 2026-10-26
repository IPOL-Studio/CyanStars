#nullable enable

namespace CyanStars.Framework.File.AppPaths
{
    /// <summary>
    /// 一次应用路径修改的结果
    /// </summary>
    public enum AppPathChangeResult
    {
        /// <summary>
        /// 配置没有变化
        /// </summary>
        NoChange,

        /// <summary>
        /// 已保存且本次运行即生效
        /// </summary>
        Applied,

        /// <summary>
        /// 已保存，重启游戏后生效
        /// </summary>
        RequiresRestart,

        /// <summary>
        /// 路径不可用，未保存
        /// </summary>
        InvalidPath,

        /// <summary>
        /// 当前平台不支持修改路径
        /// </summary>
        NotSupported,

        /// <summary>
        /// 写入配置文件失败，未保存
        /// </summary>
        SaveFailed,
    }

    /// <summary>
    /// 一个 <see cref="AppPathKind"/> 的完整状态：默认路径、保存的覆盖、本次生效路径与回退原因
    /// </summary>
    public readonly struct AppPathInfo
    {
        public AppPathKind Kind { get; }
        public string DisplayName { get; }
        public string DefaultPath { get; }
        public string? SavedOverride { get; }
        public string ActivePath { get; }

        /// <summary>
        /// 保存的覆盖本次不可用，已回退到默认路径
        /// </summary>
        public bool IsFallback { get; }

        /// <summary>
        /// 回退原因；未回退时为 null
        /// </summary>
        public string? FallbackReason { get; }

        public AppPathInfo(
            AppPathKind kind,
            string displayName,
            string defaultPath,
            string? savedOverride,
            string activePath,
            bool isFallback,
            string? fallbackReason)
        {
            Kind = kind;
            DisplayName = displayName;
            DefaultPath = defaultPath;
            SavedOverride = savedOverride;
            ActivePath = activePath;
            IsFallback = isFallback;
            FallbackReason = fallbackReason;
        }
    }
}
