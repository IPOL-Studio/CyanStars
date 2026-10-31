#nullable enable

using System.Collections.Generic;
using CyanStars.Framework.File.Staging;

namespace CyanStars.Framework.File.Platform
{
    /// <summary>
    /// 平台环境：向框架和业务提供当前平台的存储路径与文件能力
    /// </summary>
    /// <remarks>
    /// <para>由 <see cref="PlatformEnvironmentFactory"/> 按编译和运行环境创建唯一一个实现实例，并注入 <see cref="FileHandleService"/></para>
    /// <para>业务逻辑需要平台相关的路径或能力时，应通过本接口获取，而不是直接访问 <c>UnityEngine.Application</c> 的路径与平台属性</para>
    /// <para>本接口提供的是平台默认路径；允许玩家迁移的应用目录由 <see cref="FileManager.GetAppPath"/> 解析</para>
    /// <para>返回的路径可能是普通绝对路径，也可能是 <c>content://</c> 路径</para>
    /// </remarks>
    public interface IPlatformEnvironment
    {
        /// <summary>
        /// 应用持久化数据目录
        /// </summary>
        public string PersistentDataRoot { get; }

        /// <summary>
        /// 应用临时数据目录
        /// </summary>
        public string TemporaryCacheRoot { get; }

        /// <summary>
        /// 游戏安装根目录；无法取得时返回 null
        /// </summary>
        public string? GameInstallRoot { get; }

        /// <summary>
        /// 当前实现是否支持从外部文件夹导入内容到应用内
        /// </summary>
        public bool CanImportExternalFolder { get; }

        /// <summary>
        /// 当前实现是否支持把应用内内容导出到外部文件夹
        /// </summary>
        public bool CanExportFolder { get; }

        /// <summary>
        /// 当前平台是否允许玩家修改应用自有目录
        /// </summary>
        public bool CanCustomizeAppPaths { get; }

        /// <summary>
        /// 当前平台是否支持自动重启游戏
        /// </summary>
        public bool CanRestartApplication { get; }

        /// <summary>
        /// 取当前平台实际可用的常用文件夹
        /// </summary>
        /// <returns>常用文件夹列表；没有可用文件夹时返回空列表</returns>
        public IReadOnlyList<PlatformFolder> GetCommonFolders();

        /// <summary>
        /// 判断路径是否可以直接通过 <c>System.IO</c> 读写
        /// </summary>
        /// <param name="path">待判断的路径</param>
        /// <returns>普通本地路径返回 true；需要经平台外部存储接口访问的路径返回 false</returns>
        public bool IsLocalPath(string path);
    }
}
