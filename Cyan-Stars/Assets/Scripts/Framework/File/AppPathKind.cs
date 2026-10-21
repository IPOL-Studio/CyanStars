#nullable enable

using System;

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 应用自己管理、允许玩家重新指定的目录
    /// </summary>
    /// <remarks>平台默认路径由 <see cref="IPlatformEnvironment"/> 提供，覆盖配置与本次生效路径由 AppPathManager 解析</remarks>
    public enum AppPathKind
    {
        /// <summary>
        /// 玩家谱包目录
        /// </summary>
        PlayerChartPacks,

        /// <summary>
        /// 临时文件目录
        /// </summary>
        TemporaryCacheRoot,
    }

    public static class AppPathKindExtensions
    {
        /// <summary>
        /// 界面显示名称
        /// </summary>
        public static string GetDisplayName(this AppPathKind kind)
        {
            return kind switch
            {
                AppPathKind.PlayerChartPacks => "玩家谱包目录",
                AppPathKind.TemporaryCacheRoot => "临时文件目录",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
        }
    }

    /// <summary>
    /// 硬编码的应用目录名称
    /// </summary>
    internal static class AppDirectoryNames
    {
        /// <summary>
        /// 玩家谱包目录名
        /// </summary>
        public const string ChartPacks = "ChartPacks";
    }
}
