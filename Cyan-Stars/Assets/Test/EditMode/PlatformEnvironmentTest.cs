#nullable enable

using System.Collections.Generic;
using System.IO;
using CyanStars.Framework.File;
using CyanStars.Utils;
using NUnit.Framework;

namespace CyanStars.Test.EditMode
{
    /// <summary>
    /// 测试两个 IPlatformEnvironment 实现的路径归一化、常用文件夹与本地路径判定
    /// </summary>
    public class PlatformEnvironmentTest
    {
        [Test]
        public void DiskEnvironmentProvidesPlainLocalPaths()
        {
            var environment = new DiskPlatformEnvironment();

            Assert.IsTrue(environment.CanImportExternalFolder);
            Assert.IsTrue(environment.CanExportFolder);
            Assert.IsNotNull(environment.GameInstallRoot);
            Assert.IsTrue(environment.CanRestartApplication);
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            Assert.IsTrue(environment.CanCustomizeAppPaths);
#else
            Assert.IsFalse(environment.CanCustomizeAppPaths);
#endif
            AssertNormalizedPath(environment.PersistentDataRoot);
            AssertNormalizedPath(environment.TemporaryCacheRoot);
            Assert.IsTrue(environment.IsLocalPath("C:/any/path.txt"));
        }

        [Test]
        public void DiskEnvironmentFindsCommonFolders()
        {
            var environment = new DiskPlatformEnvironment();

            IReadOnlyList<PlatformFolder> folders = environment.GetCommonFolders();

            Assert.IsNotNull(folders);
            foreach (PlatformFolder folder in folders)
            {
                Assert.IsNotEmpty(folder.DisplayName);
                AssertCommonFolder(folder.Path);
            }
        }

        [Test]
        public void AndroidSafEnvironmentUsesContentPaths()
        {
            var environment = new AndroidSafPlatformEnvironment();

            // 当前实现未开放 SAF 整目录导入导出
            Assert.IsFalse(environment.CanImportExternalFolder);
            Assert.IsFalse(environment.CanExportFolder);
            Assert.IsNull(environment.GameInstallRoot);
            Assert.IsFalse(environment.CanCustomizeAppPaths);
            Assert.IsFalse(environment.CanRestartApplication);
            AssertNormalizedPath(environment.PersistentDataRoot);
            AssertNormalizedPath(environment.TemporaryCacheRoot);

            IReadOnlyList<PlatformFolder> folders = environment.GetCommonFolders();
            Assert.AreEqual(1, folders.Count);
            Assert.AreEqual("下载", folders[0].DisplayName);
            Assert.AreEqual(
                "content://com.android.externalstorage.documents/root/downloads",
                folders[0].Path);

            // 编辑器下不启用 SAF
            Assert.IsTrue(environment.IsLocalPath("content://com.test/folder"));
        }


        /// <summary>
        /// 断言路径非空且已归一化
        /// </summary>
        private static void AssertNormalizedPath(string path)
        {
            Assert.IsNotEmpty(path);
            Assert.AreEqual(PathUtil.Normalize(path), path);
        }

        /// <summary>
        /// 断言常用文件夹路径为已归一化的现有目录
        /// </summary>
        private static void AssertCommonFolder(string folderPath)
        {
            Assert.AreEqual(PathUtil.Normalize(folderPath), folderPath);
            Assert.IsTrue(Directory.Exists(folderPath));
        }

    }
}
