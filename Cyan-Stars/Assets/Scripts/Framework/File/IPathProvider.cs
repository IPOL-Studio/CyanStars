#nullable enable

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 跨平台路径提供者：把常用文件夹和磁盘上的普通路径统一成当前平台可用的路径
    /// </summary>
    /// <remarks>
    /// <para>由 <see cref="PlatformFilePortal"/> 在初始化时按编译和运行环境挑选唯一一个子类实例</para>
    /// <para>返回的路径可能是普通绝对路径，也可能是 <c>content://</c> 路径</para>
    /// </remarks>
    public interface IPathProvider
    {
        /// <summary>
        /// 取常用文件夹在当前平台上的路径，用于文件对话框的快速跳转
        /// </summary>
        /// <param name="folder">要获取的常用文件夹</param>
        /// <returns>路径；该文件夹在当前平台上不可用时返回 null</returns>
        string? GetCommonFolderPath(CommonFolders folder);
    }
}
