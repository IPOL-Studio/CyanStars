#nullable enable

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 平台环境：向框架和业务提供当前平台的存储路径与文件能力
    /// </summary>
    /// <remarks>
    /// <para>由 <see cref="FileManager"/> 在初始化时按编译和运行环境创建唯一一个实现实例，并注入 <see cref="FilePortal"/></para>
    /// <para>业务逻辑需要平台相关的路径或能力时，应通过本接口获取，而不是直接访问 <c>UnityEngine.Application</c> 的路径与平台属性</para>
    /// <para>返回的路径可能是普通绝对路径，也可能是 <c>content://</c> 路径</para>
    /// </remarks>
    public interface IPlatformEnvironment
    {
        /// <summary>
        /// 应用持久化数据目录
        /// </summary>
        string PersistentDataRoot { get; }

        /// <summary>
        /// 应用临时数据目录
        /// </summary>
        string TemporaryCacheRoot { get; }

        /// <summary>
        /// 是否支持从外部文件夹导入内容到应用内
        /// </summary>
        bool CanImportExternalFolder { get; }

        /// <summary>
        /// 是否支持把应用内内容导出到外部文件夹
        /// </summary>
        bool CanExportFolder { get; }

        /// <summary>
        /// 取常用文件夹在当前平台上的路径，用于文件对话框的快速跳转
        /// </summary>
        /// <param name="folder">要获取的常用文件夹</param>
        /// <returns>路径；该文件夹在当前平台上不可用时返回 null</returns>
        string? GetCommonFolderPath(CommonFolders folder);

        /// <summary>
        /// 判断某个路径在当前平台上是否只能通过 Storage Access Framework 读写
        /// </summary>
        /// <param name="path">待判断的路径</param>
        bool RequiresStorageAccessFramework(string path);
    }
}
