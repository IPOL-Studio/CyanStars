#nullable enable

using System.IO;
using CyanStars.Framework.File;
using NUnit.Framework;

namespace CyanStars.Test.EditMode
{
    /// <summary>
    /// 测试 FileManager 的静态路径检查与文件类型过滤器
    /// </summary>
    public class FileManagerTest
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
        public void IsFileExistsAndIsFolderExistsRejectEmptyPath()
        {
            Assert.IsFalse(FileManager.IsFileExists(""));
            Assert.IsFalse(FileManager.IsFileExists(null!));
            Assert.IsFalse(FileManager.IsFolderExists(""));
            Assert.IsFalse(FileManager.IsFolderExists(null!));
        }

        [Test]
        public void IsFileExistsAndIsFolderExistsDetectEntries()
        {
            string filePath = Path.Combine(rootPath, "file.txt");
            File.WriteAllText(filePath, "x");
            string folderPath = Path.Combine(rootPath, "folder");
            Directory.CreateDirectory(folderPath);

            Assert.IsTrue(FileManager.IsFileExists(filePath));
            Assert.IsFalse(FileManager.IsFolderExists(filePath));
            Assert.IsTrue(FileManager.IsFolderExists(folderPath));
            Assert.IsFalse(FileManager.IsFileExists(folderPath));
            Assert.IsFalse(FileManager.IsFileExists(Path.Combine(rootPath, "missing.txt")));
        }

        [Test]
        public void GetFileOrFolderNameReturnsLastSegment()
        {
            Assert.AreEqual("file.txt", FileManager.GetFileOrFolderName(Path.Combine(rootPath, "file.txt")));
            Assert.AreEqual("", FileManager.GetFileOrFolderName(""));
            Assert.AreEqual("", FileManager.GetFileOrFolderName(null!));
        }

        [Test]
        public void FileTypeFilterKeepsDisplayNameAndExtensions()
        {
            var filter = new FileTypeFilter("音频", ".ogg", ".wav");

            Assert.AreEqual("音频", filter.DisplayName);
            CollectionAssert.AreEqual(new[] { ".ogg", ".wav" }, filter.Extensions);
        }
    }
}
