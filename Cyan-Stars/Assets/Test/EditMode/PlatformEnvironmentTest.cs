#nullable enable

using System;
using System.IO;
using CyanStars.Framework.File;
using CyanStars.Utils;
using NUnit.Framework;

namespace CyanStars.Test.EditMode
{
    /// <summary>
    /// 测试两个 IPlatformEnvironment 实现的路径归一化、常用文件夹与 SAF 判定
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
            Assert.IsFalse(environment.RequiresStorageAccessFramework("C:/any/path.txt"));
        }

        [Test]
        public void DiskEnvironmentFindsCommonFolders()
        {
            var environment = new DiskPlatformEnvironment();

            AssertCommonFolder(environment.GetCommonFolderPath(CommonFolders.Downloads));
            AssertCommonFolder(environment.GetCommonFolderPath(CommonFolders.Desktop));
            Assert.Throws<ArgumentOutOfRangeException>(() => environment.GetCommonFolderPath((CommonFolders)(-1)));
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

            Assert.AreEqual(
                "content://com.android.externalstorage.documents/root/downloads",
                environment.GetCommonFolderPath(CommonFolders.Downloads));
            Assert.IsNull(environment.GetCommonFolderPath(CommonFolders.Desktop));
            Assert.Throws<ArgumentOutOfRangeException>(() => environment.GetCommonFolderPath((CommonFolders)(-1)));

            // 编辑器下不启用 SAF
            Assert.IsFalse(environment.RequiresStorageAccessFramework("content://com.test/folder"));
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
        /// 断言常用文件夹路径可用：要么返回 null，要么是存在的已归一化目录
        /// </summary>
        private static void AssertCommonFolder(string? folderPath)
        {
            if (folderPath == null)
                return;

            Assert.AreEqual(PathUtil.Normalize(folderPath), folderPath);
            Assert.IsTrue(Directory.Exists(folderPath));
        }

    }
}
