#nullable enable

using System.IO;
using System.Text.RegularExpressions;
using CyanStars.Framework.File;
using CyanStars.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CyanStars.Test.EditMode
{
    /// <summary>
    /// 测试 FolderUtil 的整目录复制：内容、重名后缀、自我嵌套拒绝与失败清理
    /// </summary>
    public class FolderUtilTest
    {
        private string rootPath = null!;

        [SetUp]
        public void SetUp()
        {
            rootPath = TestFolderUtil.CreateTestRoot();
        }

        [TearDown]
        public void TearDown()
        {
            TestFolderUtil.DeleteTestRoot(rootPath);
        }


        [Test]
        public void TryCopyFolderCopiesNestedContent()
        {
            string sourcePath = Path.Combine(rootPath, "source");
            Directory.CreateDirectory(Path.Combine(sourcePath, "sub"));
            File.WriteAllText(Path.Combine(sourcePath, "a.txt"), "A");
            File.WriteAllText(Path.Combine(sourcePath, "sub", "b.txt"), "B");

            string targetPath = Path.Combine(rootPath, "target");

            Assert.IsTrue(FolderUtil.TryCopyFolder(sourcePath, targetPath, out string copiedFolderPath));
            Assert.IsTrue(PathUtil.PathEquals(targetPath, copiedFolderPath));
            Assert.AreEqual("A", File.ReadAllText(Path.Combine(copiedFolderPath, "a.txt")));
            Assert.AreEqual("B", File.ReadAllText(Path.Combine(copiedFolderPath, "sub", "b.txt")));
        }

        [Test]
        public void TryCopyFolderKeepsSourceIntact()
        {
            string sourcePath = Path.Combine(rootPath, "source");
            Directory.CreateDirectory(sourcePath);
            File.WriteAllText(Path.Combine(sourcePath, "a.txt"), "A");

            string targetPath = Path.Combine(rootPath, "target");

            Assert.IsTrue(FolderUtil.TryCopyFolder(sourcePath, targetPath, out _));
            Assert.IsTrue(Directory.Exists(sourcePath));
            Assert.AreEqual("A", File.ReadAllText(Path.Combine(sourcePath, "a.txt")));
        }

        [Test]
        public void TryCopyFolderAppendsSuffixWhenTargetExists()
        {
            string sourcePath = Path.Combine(rootPath, "source");
            Directory.CreateDirectory(sourcePath);
            File.WriteAllText(Path.Combine(sourcePath, "a.txt"), "A");

            string targetPath = Path.Combine(rootPath, "target");
            Directory.CreateDirectory(targetPath);

            Assert.IsTrue(FolderUtil.TryCopyFolder(sourcePath, targetPath, out string firstCopiedPath));
            Assert.IsTrue(PathUtil.PathEquals(targetPath + " (1)", firstCopiedPath));
            Assert.IsTrue(Directory.Exists(firstCopiedPath));

            Assert.IsTrue(FolderUtil.TryCopyFolder(sourcePath, targetPath, out string secondCopiedPath));
            Assert.IsTrue(PathUtil.PathEquals(targetPath + " (2)", secondCopiedPath));
        }

        [Test]
        public void TryCopyFolderRejectsTargetInsideSource()
        {
            string sourcePath = Path.Combine(rootPath, "source");
            Directory.CreateDirectory(Path.Combine(sourcePath, "sub"));

            LogAssert.Expect(LogType.Error, $"复制文件夹的目标不能等于或位于源之内：{sourcePath}");
            Assert.IsFalse(FolderUtil.TryCopyFolder(sourcePath, sourcePath, out string copiedFolderPath));
            Assert.AreEqual("", copiedFolderPath);

            string subFolderPath = Path.Combine(sourcePath, "sub");
            LogAssert.Expect(LogType.Error, $"复制文件夹的目标不能等于或位于源之内：{subFolderPath}");
            Assert.IsFalse(FolderUtil.TryCopyFolder(sourcePath, subFolderPath, out copiedFolderPath));
            Assert.AreEqual("", copiedFolderPath);
        }

        [Test]
        public void TryCopyFolderRejectsInvalidArguments()
        {
            string sourcePath = Path.Combine(rootPath, "source");
            Directory.CreateDirectory(sourcePath);
            string targetPath = Path.Combine(rootPath, "target");

            LogAssert.Expect(LogType.Error, "要复制的源文件夹不存在：");
            Assert.IsFalse(FolderUtil.TryCopyFolder("", targetPath, out _));

            string missingSourcePath = Path.Combine(rootPath, "missing");
            LogAssert.Expect(LogType.Error, $"要复制的源文件夹不存在：{missingSourcePath}");
            Assert.IsFalse(FolderUtil.TryCopyFolder(missingSourcePath, targetPath, out _));

            LogAssert.Expect(LogType.Error, "复制文件夹时目标路径为空。");
            Assert.IsFalse(FolderUtil.TryCopyFolder(sourcePath, "", out _));
        }

        [Test]
        public void TryCopyFolderCleansUpWhenCopyFails()
        {
            string sourcePath = Path.Combine(rootPath, "source");
            Directory.CreateDirectory(sourcePath);
            File.WriteAllText(Path.Combine(sourcePath, "a.txt"), "A");

            // 目标位置被同名文件占用，创建目录会失败
            string targetPath = Path.Combine(rootPath, "occupied");
            File.WriteAllText(targetPath, "blocker");

            LogAssert.Expect(LogType.Error,
                new Regex($"{Regex.Escape($"把文件夹 {sourcePath} 复制到 {targetPath} 时出错：")}.*"));
            Assert.IsFalse(FolderUtil.TryCopyFolder(sourcePath, targetPath, out string copiedFolderPath));
            Assert.AreEqual("", copiedFolderPath);
            Assert.AreEqual("blocker", File.ReadAllText(targetPath));
        }

        [Test]
        public void TryCopyFolderRejectsSourceBeyondMaxDepth()
        {
            string sourcePath = Path.Combine(rootPath, "source");
            string currentPath = sourcePath;
            for (int depth = 0; depth < 20; depth++)
            {
                currentPath = Path.Combine(currentPath, "d" + depth);
            }

            Directory.CreateDirectory(currentPath);
            File.WriteAllText(Path.Combine(currentPath, "a.txt"), "A");

            string targetPath = Path.Combine(rootPath, "target");

            LogAssert.Expect(LogType.Error,
                new Regex($"{Regex.Escape($"把文件夹 {sourcePath} 复制到 {targetPath} 时出错：")}.*"));
            Assert.IsFalse(FolderUtil.TryCopyFolder(sourcePath, targetPath, out string copiedFolderPath));
            Assert.AreEqual("", copiedFolderPath);
            Assert.IsFalse(Directory.Exists(targetPath));
        }
    }
}
