#nullable enable

using System.IO;
using System.Text.RegularExpressions;
using CyanStars.Framework.File.Content;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CyanStars.Test.EditMode
{
    /// <summary>
    /// 测试不经过会话暂存的普通文本读取
    /// </summary>
    public class FileContentServiceTest
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
        public void TryReadTextFromPathReadsLocalFile()
        {
            string filePath = Path.Combine(rootPath, "text.txt");
            const string expected = "你好，CyanStars";
            File.WriteAllText(filePath, expected);

            Assert.IsTrue(FileContentService.TryReadTextFromPath(filePath, out string text));
            Assert.AreEqual(expected, text);
        }

        [Test]
        public void TryReadTextFromPathReturnsEmptyContentForEmptyFile()
        {
            string filePath = Path.Combine(rootPath, "empty.txt");
            File.WriteAllText(filePath, "");

            Assert.IsTrue(FileContentService.TryReadTextFromPath(filePath, out string text));
            Assert.IsEmpty(text);
        }

        [Test]
        public void TryReadTextFromPathRejectsEmptyPath()
        {
            LogAssert.Expect(LogType.Error, "读取文本时来源路径为空。");
            Assert.IsFalse(FileContentService.TryReadTextFromPath("", out string text));
            Assert.IsEmpty(text);

            LogAssert.Expect(LogType.Error, "读取文本时来源路径为空。");
            Assert.IsFalse(FileContentService.TryReadTextFromPath(null!, out text));
            Assert.IsEmpty(text);
        }

        [Test]
        public void TryReadTextFromPathReturnsFalseForMissingFile()
        {
            string missingPath = Path.Combine(rootPath, "missing.txt");

            LogAssert.Expect(LogType.Error, new Regex($"{Regex.Escape($"读取文件 {missingPath} 时出错：")}.*"));
            Assert.IsFalse(FileContentService.TryReadTextFromPath(missingPath, out string text));
            Assert.IsEmpty(text);
        }
    }
}
