#nullable enable

#if UNITY_ANDROID && !UNITY_EDITOR
using SimpleFileBrowser;
#endif

namespace CyanStars.Framework.File
{
    /// <summary>
    /// 创建当前运行环境对应的平台环境
    /// </summary>
    internal static class PlatformEnvironmentFactory
    {
        public static IPlatformEnvironment Create()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Simple File Browser 会在运行时判断安卓 10+ 是否必须走 SAF
            if (FileBrowserHelpers.ShouldUseSAF)
                return new AndroidSafPlatformEnvironment();
#endif
            return new DiskPlatformEnvironment();
        }
    }
}
