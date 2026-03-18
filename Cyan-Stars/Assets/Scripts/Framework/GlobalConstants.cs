#nullable enable

namespace CyanStars.Framework
{
    /// <summary>
    /// 全局常量
    /// </summary>
    public static class GlobalConstants
    {
        /// <summary>
        /// 应用数据路径下的会话级缓存路径名
        /// </summary>
        /// <remarks>
        /// 存放需要在本次应用启动期间不被删除的文件，每次启动应用时清空
        /// </remarks>
        /// <example>
        /// /Application.persistentDataPath/[TempSessionFolderName]
        /// </example>
        public const string TempSessionFolderName = "TempSession";
    }
}
