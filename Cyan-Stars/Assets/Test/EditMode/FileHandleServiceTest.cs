#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CyanStars.Framework.File;
using CyanStars.Framework.File.Platform;
using CyanStars.Framework.File.Staging;
using CyanStars.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CyanStars.Test.EditMode
{
    /// <summary>
    /// 测试 FileHandleService 的句柄暂存、保存目标校验、保存与释放、缓存作用域管理与文本写入
    /// </summary>
    public class FileHandleServiceTest
    {
        private const string CacheScope = "TestScope";

        private string rootPath = null!;
        private string externalFolderPath = null!;
        private string temporaryRootPath = null!;
        private FileHandleService service = null!;

        [SetUp]
        public void SetUp()
        {
            rootPath = TestFolderUtil.CreateTestRoot();
            externalFolderPath = Path.Combine(rootPath, "external");
            temporaryRootPath = Path.Combine(rootPath, "temp");
            Directory.CreateDirectory(externalFolderPath);
            Directory.CreateDirectory(temporaryRootPath);

            service = new FileHandleService(new TestPlatformEnvironment(temporaryRootPath));
            service.Init();
        }

        [TearDown]
        public void TearDown()
        {
            service.Shutdown();
            TestFolderUtil.DeleteTestRoot(rootPath);
        }


        [Test]
        public void ConstructorRejectsNullEnvironment()
        {
            Assert.Throws<ArgumentNullException>(() => new FileHandleService(null!));
        }

        [Test]
        public void TryStageFileCopiesFileIntoCache()
        {
            string sourcePath = CreateExternalFile("music.ogg", "audio");

            FileHandle? handle = service.TryStageFile(sourcePath, CacheScope);

            Assert.IsNotNull(handle);
            Assert.AreEqual(FileHandleState.Available, handle!.State);
            Assert.AreEqual(sourcePath, handle.SourcePath);
            Assert.AreEqual("music.ogg", handle.EntryName);
            Assert.IsTrue(PathUtil.IsSubPathOf(handle.ReadablePath, temporaryRootPath));
            Assert.IsFalse(PathUtil.PathEquals(sourcePath, handle.ReadablePath));
            Assert.AreEqual("audio", File.ReadAllText(handle.ReadablePath));
            Assert.AreEqual("audio", File.ReadAllText(sourcePath));
        }

        [Test]
        public void TryStageFileCopiesFolderIntoCache()
        {
            string sourcePath = CreateExternalFolder("chart", "chart.json", "{}");
            File.WriteAllText(Path.Combine(sourcePath, "readme.txt"), "hello");

            FileHandle? handle = service.TryStageFile(sourcePath, CacheScope);

            Assert.IsNotNull(handle);
            Assert.AreEqual("chart", handle!.EntryName);
            Assert.IsTrue(Directory.Exists(handle.ReadablePath));
            Assert.AreEqual("{}", File.ReadAllText(Path.Combine(handle.ReadablePath, "chart.json")));
            Assert.AreEqual("hello", File.ReadAllText(Path.Combine(handle.ReadablePath, "readme.txt")));
        }

        [Test]
        public void TryStageFileReturnsNullForInvalidPaths()
        {
            string missingPath = Path.Combine(externalFolderPath, "missing.txt");
            LogAssert.Expect(LogType.Error, "加载文件时传入的路径为空。");
            Assert.IsNull(service.TryStageFile("", CacheScope));

            LogAssert.Expect(LogType.Error, $"要加载的文件（夹）不存在：{missingPath}");
            Assert.IsNull(service.TryStageFile(missingPath, CacheScope));

            string sourcePath = CreateExternalFile("inside.txt", "x");
            FileHandle handle = service.TryStageFile(sourcePath, CacheScope)!;

            // 加载来源不能位于会话缓存内
            LogAssert.Expect(LogType.Error, $"加载路径不能位于会话缓存内：{handle.ReadablePath}");
            Assert.IsNull(service.TryStageFile(handle.ReadablePath, CacheScope));

            string childPath = Path.Combine(handle.ReadablePath, "once-more.txt");
            LogAssert.Expect(LogType.Error, $"加载路径不能位于会话缓存内：{childPath}");
            Assert.IsNull(service.TryStageFile(childPath, CacheScope));
        }

        [Test]
        public void TryStageFileGivesUniqueCachePathForSameName()
        {
            string firstPath = CreateExternalFile(Path.Combine("a", "chart.json"), "first");
            string secondPath = CreateExternalFile(Path.Combine("b", "chart.json"), "second");

            FileHandle first = service.TryStageFile(firstPath, CacheScope)!;
            FileHandle second = service.TryStageFile(secondPath, CacheScope)!;

            Assert.AreNotEqual(first.ReadablePath, second.ReadablePath);
            StringAssert.Contains(" (1)", PathUtil.GetName(second.ReadablePath));
            Assert.AreEqual("chart.json", first.EntryName);
            Assert.AreEqual("chart.json", second.EntryName);
            Assert.AreEqual("first", File.ReadAllText(first.ReadablePath));
            Assert.AreEqual("second", File.ReadAllText(second.ReadablePath));
        }

        [Test]
        public void TrySetSaveTargetNormalizesAndDetaches()
        {
            FileHandle handle = service.TryStageFile(CreateExternalFile("a.txt", "x"), CacheScope)!;
            string targetPath = Path.Combine(externalFolderPath, "out", "a.txt");

            Assert.IsTrue(service.TrySetSaveTarget(handle, targetPath));
            Assert.AreEqual(PathUtil.Normalize(targetPath), handle.TargetPath);

            Assert.IsTrue(service.TrySetSaveTarget(handle, null));
            Assert.IsNull(handle.TargetPath);

            Assert.IsTrue(service.TrySetSaveTarget(handle, ""));
            Assert.IsNull(handle.TargetPath);
        }

        [Test]
        public void TrySetSaveTargetRejectsPathInsideSessionFolder()
        {
            FileHandle handle = service.TryStageFile(CreateExternalFile("a.txt", "x"), CacheScope)!;
            FileHandle other = service.TryStageFile(CreateExternalFile("b.txt", "x"), CacheScope)!;

            LogAssert.Expect(LogType.Error, $"保存路径不能位于会话缓存内：{PathUtil.Normalize(other.ReadablePath)}");
            Assert.IsFalse(service.TrySetSaveTarget(handle, other.ReadablePath));

            string childPath = Path.Combine(other.ReadablePath, "child.txt");
            LogAssert.Expect(LogType.Error, $"保存路径不能位于会话缓存内：{PathUtil.Normalize(childPath)}");
            Assert.IsFalse(service.TrySetSaveTarget(handle, childPath));

            Assert.IsNull(handle.TargetPath);
        }

        [Test]
        public void TrySetSaveTargetRejectsReleasedHandle()
        {
            FileHandle handle = service.TryStageFile(CreateExternalFile("a.txt", "x"), CacheScope)!;
            Assert.IsTrue(service.TryReleaseFile(handle));

            LogAssert.Expect(LogType.Error, $"句柄 {handle.ReadablePath} 不是可用状态，无法修改。");
            Assert.IsFalse(service.TrySetSaveTarget(handle, Path.Combine(externalFolderPath, "a.txt")));
            Assert.IsNull(handle.TargetPath);
        }

        [Test]
        public void IsHandleReadableChecksStateAndContent()
        {
            FileHandle fileHandle = service.TryStageFile(CreateExternalFile("a.txt", "x"), CacheScope)!;
            FileHandle folderHandle = service.TryStageFile(CreateExternalFolder("chart", "chart.json", "{}"), CacheScope)!;

            Assert.IsTrue(service.IsHandleReadable(fileHandle));
            Assert.IsTrue(service.IsHandleReadable(folderHandle));
            Assert.IsFalse(service.IsHandleReadable(null));

            File.Delete(fileHandle.ReadablePath);
            Directory.Delete(folderHandle.ReadablePath, true);
            Assert.IsFalse(service.IsHandleReadable(fileHandle));
            Assert.IsFalse(service.IsHandleReadable(folderHandle));
        }

        [Test]
        public void TrySaveToTargetRequiresTargetAndReadableContent()
        {
            FileHandle handle = service.TryStageFile(CreateExternalFile("a.txt", "content"), CacheScope)!;

            // 尚未指定保存位置
            LogAssert.Expect(LogType.Error, $"句柄 {handle.ReadablePath} 尚未指定保存位置，无法保存。");
            Assert.IsFalse(service.TrySaveToTarget(handle));

            string targetPath = Path.Combine(externalFolderPath, "out", "a.txt");
            service.TrySetSaveTarget(handle, targetPath);

            // 模拟缓存副本被系统清理
            File.Delete(handle.ReadablePath);
            LogAssert.Expect(LogType.Error,
                $"句柄 {handle.ReadablePath} 的内容已经不存在，无法保存到 {PathUtil.Normalize(targetPath)}。");
            Assert.IsFalse(service.TrySaveToTarget(handle));
            Assert.IsFalse(File.Exists(targetPath));
        }

        [Test]
        public void TrySaveToTargetWritesFileAndCreatesFolder()
        {
            FileHandle handle = service.TryStageFile(CreateExternalFile("a.txt", "content"), CacheScope)!;
            string targetPath = Path.Combine(externalFolderPath, "out", "nested", "a.txt");
            Assert.IsTrue(service.TrySetSaveTarget(handle, targetPath));

            Assert.IsTrue(service.TrySaveToTarget(handle));
            Assert.AreEqual("content", File.ReadAllText(targetPath));
        }

        [Test]
        public void TrySaveToTargetHonorsOverwriteFlag()
        {
            FileHandle handle = service.TryStageFile(CreateExternalFile("a.txt", "new"), CacheScope)!;
            string targetPath = Path.Combine(externalFolderPath, "out", "a.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            File.WriteAllText(targetPath, "old");
            service.TrySetSaveTarget(handle, targetPath);

            Assert.IsFalse(service.TrySaveToTarget(handle));
            Assert.AreEqual("old", File.ReadAllText(targetPath));

            Assert.IsTrue(service.TrySaveToTarget(handle, true));
            Assert.AreEqual("new", File.ReadAllText(targetPath));
        }

        [Test]
        public void TrySaveToTargetRejectsReleasedHandle()
        {
            FileHandle handle = service.TryStageFile(CreateExternalFile("a.txt", "x"), CacheScope)!;
            service.TrySetSaveTarget(handle, Path.Combine(externalFolderPath, "out", "a.txt"));
            service.TryReleaseFile(handle);

            LogAssert.Expect(LogType.Error, $"句柄 {handle.ReadablePath} 不是可用状态，无法保存。");
            Assert.IsFalse(service.TrySaveToTarget(handle));
        }

        [Test]
        public void TrySaveToTargetCopiesFolderWithReplacementSemantics()
        {
            string sourcePath = CreateExternalFolder("chart", "fresh.txt", "fresh");
            FileHandle handle = service.TryStageFile(sourcePath, CacheScope)!;
            string targetPath = Path.Combine(externalFolderPath, "out", "chart");
            Directory.CreateDirectory(targetPath);
            File.WriteAllText(Path.Combine(targetPath, "stale.txt"), "stale");
            service.TrySetSaveTarget(handle, targetPath);

            Assert.IsFalse(service.TrySaveToTarget(handle));
            Assert.IsTrue(File.Exists(Path.Combine(targetPath, "stale.txt")));

            // 覆盖按替换语义处理：先删除旧目录再复制
            Assert.IsTrue(service.TrySaveToTarget(handle, true));
            Assert.IsTrue(File.Exists(Path.Combine(targetPath, "fresh.txt")));
            Assert.IsFalse(File.Exists(Path.Combine(targetPath, "stale.txt")));
        }

        [Test]
        public void TrySaveAllContinuesAfterFailure()
        {
            FileHandle good = service.TryStageFile(CreateExternalFile("good.txt", "good"), CacheScope)!;
            FileHandle bad = service.TryStageFile(CreateExternalFile("bad.txt", "bad"), CacheScope)!;
            service.TrySetSaveTarget(good, Path.Combine(externalFolderPath, "out", "good.txt"));

            LogAssert.Expect(LogType.Error, $"句柄 {bad.ReadablePath} 尚未指定保存位置，无法保存。");
            Assert.IsFalse(service.TrySaveAll(new[] { good, bad }));
            Assert.IsTrue(File.Exists(Path.Combine(externalFolderPath, "out", "good.txt")));

            service.TrySetSaveTarget(bad, Path.Combine(externalFolderPath, "out", "bad.txt"));
            Assert.IsTrue(service.TrySaveAll(new[] { good, bad }, true));
            Assert.IsTrue(File.Exists(Path.Combine(externalFolderPath, "out", "bad.txt")));

            Assert.Throws<ArgumentNullException>(() => service.TrySaveAll(null!));
        }

        [Test]
        public void TryReleaseFileDeletesCacheCopyOnce()
        {
            FileHandle handle = service.TryStageFile(CreateExternalFile("a.txt", "x"), CacheScope)!;
            string readablePath = handle.ReadablePath;

            Assert.IsTrue(service.TryReleaseFile(handle));
            Assert.AreEqual(FileHandleState.Released, handle.State);
            Assert.IsNull(handle.TargetPath);
            Assert.IsFalse(File.Exists(readablePath));
            Assert.IsTrue(File.Exists(handle.SourcePath));

            Assert.IsFalse(service.TryReleaseFile(handle));
        }

        [Test]
        public void TryReleaseFileHandlesMissingCacheCopy()
        {
            FileHandle handle = service.TryStageFile(CreateExternalFile("a.txt", "x"), CacheScope)!;
            File.Delete(handle.ReadablePath);

            Assert.IsTrue(service.TryReleaseFile(handle));
            Assert.AreEqual(FileHandleState.Released, handle.State);
        }

        [Test]
        public void TryReleaseAllReturnsReleasedCount()
        {
            FileHandle first = service.TryStageFile(CreateExternalFile(Path.Combine("a", "a.txt"), "x"), CacheScope)!;
            FileHandle second = service.TryStageFile(CreateExternalFile(Path.Combine("b", "b.txt"), "x"), CacheScope)!;
            FileHandle third = service.TryStageFile(CreateExternalFile(Path.Combine("c", "c.txt"), "x"), CacheScope)!;
            service.TryReleaseFile(first);

            Assert.AreEqual(2, service.TryReleaseAll(new[] { first, second, third }));
            Assert.Throws<ArgumentNullException>(() => service.TryReleaseAll(null!));
            Assert.Throws<ArgumentNullException>(() => service.TryReleaseFile(null!));
        }

        [Test]
        public void ClearCacheScopeIfEmptyOnlyDeletesEmptyFolder()
        {
            FileHandle first = service.TryStageFile(CreateExternalFile(Path.Combine("a", "a.txt"), "x"), CacheScope)!;
            FileHandle second = service.TryStageFile(CreateExternalFile(Path.Combine("b", "b.txt"), "x"), CacheScope)!;
            string scopeFolderPath = Path.GetDirectoryName(first.ReadablePath)!;

            // 还有缓存文件时不删除
            service.ClearCacheScopeIfEmpty(CacheScope);
            Assert.IsTrue(Directory.Exists(scopeFolderPath));

            service.TryReleaseFile(first);
            service.TryReleaseFile(second);
            service.ClearCacheScopeIfEmpty(CacheScope);
            Assert.IsFalse(Directory.Exists(scopeFolderPath));

            // 不存在的缓存作用域不报错
            service.ClearCacheScopeIfEmpty("MissingScope");
        }

        [Test]
        public void TryWriteTextToPathWritesUtf8WithoutBom()
        {
            string targetPath = Path.Combine(externalFolderPath, "out", "nested", "text.txt");
            const string text = "你好，世界";

            Assert.IsTrue(service.TryWriteTextToPath(targetPath, text));

            byte[] bytes = File.ReadAllBytes(targetPath);
            Assert.AreEqual(Encoding.UTF8.GetByteCount(text), bytes.Length);
            Assert.AreEqual(text, File.ReadAllText(targetPath, Encoding.UTF8));
        }

        [Test]
        public void TryWriteTextToPathRejectsInvalidTargets()
        {
            LogAssert.Expect(LogType.Error, "写入文本时目标路径为空。");
            Assert.IsFalse(service.TryWriteTextToPath("", "text"));

            FileHandle handle = service.TryStageFile(CreateExternalFolder("cache", "f.txt", "x"), CacheScope)!;
            string cacheTargetPath = Path.Combine(handle.ReadablePath, "new.txt");
            LogAssert.Expect(LogType.Error, $"目标路径不能位于会话缓存内：{cacheTargetPath}");
            Assert.IsFalse(service.TryWriteTextToPath(cacheTargetPath, "text"));

            // 目标路径中间存在同名文件，创建上级目录会失败
            string blockingFilePath = CreateExternalFile("blocker.txt", "x");
            string blockedTargetPath = Path.Combine(blockingFilePath, "child.txt");
            LogAssert.Expect(LogType.Error, new Regex($"{Regex.Escape($"写入文件 {blockedTargetPath} 时出错：")}.*"));
            Assert.IsFalse(service.TryWriteTextToPath(blockedTargetPath, "text"));
        }


        /// <summary>
        /// 在外部目录里创建一个文件，自动创建上级目录
        /// </summary>
        private string CreateExternalFile(string relativePath, string content)
        {
            string filePath = Path.Combine(externalFolderPath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, content);
            return filePath;
        }

        /// <summary>
        /// 在外部目录里创建一个带文件的文件夹
        /// </summary>
        private string CreateExternalFolder(string relativePath, string fileName, string content)
        {
            string folderPath = Path.Combine(externalFolderPath, relativePath);
            Directory.CreateDirectory(folderPath);
            File.WriteAllText(Path.Combine(folderPath, fileName), content);
            return folderPath;
        }


        /// <summary>
        /// 测试用的平台环境：临时数据目录指向测试目录，所有路径都按普通本地路径处理
        /// </summary>
        private sealed class TestPlatformEnvironment : IPlatformEnvironment
        {
            public TestPlatformEnvironment(string temporaryCacheRoot)
            {
                TemporaryCacheRoot = PathUtil.Normalize(temporaryCacheRoot);
            }

            public string PersistentDataRoot => TemporaryCacheRoot;

            public string TemporaryCacheRoot { get; }

            public string? GameInstallRoot => null;

            public bool CanImportExternalFolder => true;

            public bool CanExportFolder => true;

            public bool CanCustomizeAppPaths => false;

            public bool CanRestartApplication => false;

            public IReadOnlyList<PlatformFolder> GetCommonFolders()
            {
                return Array.Empty<PlatformFolder>();
            }

            public bool IsLocalPath(string path)
            {
                return true;
            }
        }
    }
}
