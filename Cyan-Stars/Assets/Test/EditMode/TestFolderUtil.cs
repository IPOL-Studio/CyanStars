#nullable enable

using System;
using System.IO;

namespace CyanStars.Test.EditMode
{
    /// <summary>
    /// 测试用临时目录工具：为每个测试提供独立的根目录，并负责清理
    /// </summary>
    internal static class TestFolderUtil
    {
        /// <summary>
        /// 创建本次测试专属的临时根目录
        /// </summary>
        public static string CreateTestRoot()
        {
            string rootPath = Path.Combine(Path.GetTempPath(), "CyanStarsTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(rootPath);
            return rootPath;
        }

        /// <summary>
        /// 递归删除测试临时根目录
        /// </summary>
        /// <remarks>删除失败时忽略，避免干扰测试结果</remarks>
        public static void DeleteTestRoot(string rootPath)
        {
            try
            {
                if (Directory.Exists(rootPath))
                    Directory.Delete(rootPath, true);
            }
            catch (Exception)
            {
                // 删除失败通常是被占用，残留目录由系统清理即可
            }
        }
    }
}
