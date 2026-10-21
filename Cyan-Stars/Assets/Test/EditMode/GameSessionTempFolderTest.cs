#nullable enable

using System;
using System.IO;
using CyanStars.Framework.File;
using CyanStars.Utils;
using NUnit.Framework;

namespace CyanStars.Test.EditMode
{
    /// <summary>
    /// 测试 GameSessionTempFolder 的会话目录创建、残留清理、占用标记与缓存作用域管理
    /// </summary>
    public class GameSessionTempFolderTest
    {
        private const string SessionLockFileName = ".session_lock";

        private string rootPath = null!;
        private GameSessionTempFolder sessionFolder = null!;

        [SetUp]
        public void SetUp()
        {
            rootPath = TestFolderUtil.CreateTestRoot();
            sessionFolder = new GameSessionTempFolder(rootPath);
        }

        [TearDown]
        public void TearDown()
        {
            sessionFolder.DeleteSessionFolder();
            TestFolderUtil.DeleteTestRoot(rootPath);
        }


        [Test]
        public void InitCreatesSessionFolderWithLockFile()
        {
            sessionFolder.Init();

            Assert.IsTrue(Directory.Exists(sessionFolder.SessionFolderPath));
            Assert.IsTrue(PathUtil.IsSubPathOf(sessionFolder.SessionFolderPath, rootPath));
            StringAssert.StartsWith("GameSession_", PathUtil.GetName(sessionFolder.SessionFolderPath));
            Assert.IsTrue(File.Exists(Path.Combine(sessionFolder.SessionFolderPath, SessionLockFileName)));
        }

        [Test]
        public void IsInsideSessionFolderChecksPathPrefix()
        {
            sessionFolder.Init();

            Assert.IsTrue(sessionFolder.IsInsideSessionFolder(sessionFolder.SessionFolderPath));
            Assert.IsTrue(sessionFolder.IsInsideSessionFolder(Path.Combine(sessionFolder.SessionFolderPath, "Scope", "a.txt")));
            Assert.IsFalse(sessionFolder.IsInsideSessionFolder(rootPath));
            Assert.IsFalse(sessionFolder.IsInsideSessionFolder("content://com.test/folder/f.txt"));
            Assert.IsFalse(sessionFolder.IsInsideSessionFolder(""));
        }

        [Test]
        public void InitDeletesLeftoverSessionFolder()
        {
            string leftoverPath = Path.Combine(rootPath, "GameSession_0123456789abcdef");
            Directory.CreateDirectory(leftoverPath);
            File.WriteAllText(Path.Combine(leftoverPath, "cache.bin"), "x");
            File.WriteAllText(Path.Combine(leftoverPath, SessionLockFileName), "");

            string unrelatedPath = Path.Combine(rootPath, "OtherFolder");
            Directory.CreateDirectory(unrelatedPath);

            sessionFolder.Init();

            Assert.IsFalse(Directory.Exists(leftoverPath));
            Assert.IsTrue(Directory.Exists(unrelatedPath));
        }

        [Test]
        public void InitKeepsSessionFolderInUse()
        {
            string inUsePath = Path.Combine(rootPath, "GameSession_ffffffffffffffff");
            Directory.CreateDirectory(inUsePath);
            string lockFilePath = Path.Combine(inUsePath, SessionLockFileName);

            using (new FileStream(lockFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                sessionFolder.Init();

                Assert.IsTrue(Directory.Exists(inUsePath));
            }
        }

        [Test]
        public void DeleteSessionFolderRemovesFolderAndAllowsReinit()
        {
            sessionFolder.Init();
            Assert.IsTrue(Directory.Exists(sessionFolder.SessionFolderPath));

            sessionFolder.DeleteSessionFolder();
            Assert.IsFalse(Directory.Exists(sessionFolder.SessionFolderPath));

            sessionFolder.Init();
            Assert.IsTrue(Directory.Exists(sessionFolder.SessionFolderPath));
        }

        [Test]
        public void DeleteSessionFolderIsSafeBeforeInit()
        {
            sessionFolder.DeleteSessionFolder();

            Assert.IsFalse(Directory.Exists(sessionFolder.SessionFolderPath));
        }

        [Test]
        public void GetCacheFolderPathRejectsEmptyScope()
        {
            Assert.Throws<ArgumentException>(() => sessionFolder.GetCacheFolderPath(""));
            Assert.Throws<ArgumentException>(() => sessionFolder.GetCacheFolderPath(null!));
        }

        [Test]
        public void EnsureCacheFolderCreatesOnlyMissingFolder()
        {
            sessionFolder.Init();

            string cacheFolderPath = sessionFolder.EnsureCacheFolder("Scope");

            Assert.IsTrue(Directory.Exists(cacheFolderPath));
            Assert.IsTrue(PathUtil.IsSubPathOf(cacheFolderPath, sessionFolder.SessionFolderPath));
            Assert.AreEqual(cacheFolderPath, sessionFolder.EnsureCacheFolder("Scope"));
        }

        [Test]
        public void DeleteCacheFolderIfEmptyOnlyDeletesEmptyFolder()
        {
            sessionFolder.Init();

            string cacheFolderPath = sessionFolder.EnsureCacheFolder("Scope");
            sessionFolder.DeleteCacheFolderIfEmpty("Scope");
            Assert.IsFalse(Directory.Exists(cacheFolderPath));

            sessionFolder.EnsureCacheFolder("Scope");
            File.WriteAllText(Path.Combine(cacheFolderPath, "cache.bin"), "x");
            sessionFolder.DeleteCacheFolderIfEmpty("Scope");
            Assert.IsTrue(Directory.Exists(cacheFolderPath));

            sessionFolder.DeleteCacheFolderIfEmpty("MissingScope");
        }
    }
}
