#nullable enable

using System.Collections.Generic;
using CyanStars.Utils;
using NUnit.Framework;

namespace CyanStars.Test.EditMode
{
    /// <summary>
    /// 测试 PathUtil 的路径归一化、拼接、名称提取、包含判断与语义比较
    /// </summary>
    public class PathUtilTest
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        /// <summary>
        /// 当前平台的文件系统是否不区分大小写
        /// </summary>
        private const bool IsCaseInsensitiveFileSystem = true;
#else
        /// <summary>
        /// 当前平台的文件系统是否不区分大小写
        /// </summary>
        private const bool IsCaseInsensitiveFileSystem = false;
#endif


        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("C:\\Users\\test\\", "C:/Users/test")]
        [TestCase("C:\\Users\\test", "C:/Users/test")]
        [TestCase("C:\\", "C:/")]
        [TestCase("C:/", "C:/")]
        [TestCase("/", "/")]
        [TestCase("//server/share/", "//server/share")]
        public void NormalizeTest(string? path, string? expected)
        {
            Assert.AreEqual(expected, PathUtil.Normalize(path));
        }

        [Test]
        public void CombineTest()
        {
            Assert.AreEqual("C:/root/child", PathUtil.Combine("C:\\root", "child"));
            Assert.AreEqual("C:/root/child", PathUtil.Combine("C:/root/", "child"));
            Assert.AreEqual("C:/root/a/b", PathUtil.Combine("C:/root", "a", "b"));
            Assert.AreEqual("C:/root/a/b/c", PathUtil.Combine("C:/root", "a", "b", "c"));
        }

        [TestCase("C:/a/b.txt", "b.txt")]
        [TestCase("C:\\a\\b.txt", "b.txt")]
        [TestCase("C:/a/b/", "b")]
        [TestCase("b.txt", "b.txt")]
        [TestCase("", "")]
        public void GetNameTest(string path, string expected)
        {
            Assert.AreEqual(expected, PathUtil.GetName(path));
        }

        [Test]
        public void IsSubPathOfAcceptsSelfAndDescendants()
        {
            Assert.IsTrue(PathUtil.IsSubPathOf("C:/Test/Root", "C:/Test/Root"));
            Assert.IsTrue(PathUtil.IsSubPathOf("C:/Test/Root/", "C:/Test/Root"));
            Assert.IsTrue(PathUtil.IsSubPathOf("C:/Test/Root/Child", "C:/Test/Root"));
            Assert.IsTrue(PathUtil.IsSubPathOf("C:/Test/Root/Child/Grand", "C:/Test/Root"));
        }

        [Test]
        public void IsSubPathOfRejectsOutsidePaths()
        {
            Assert.IsFalse(PathUtil.IsSubPathOf("C:/Test/Root2", "C:/Test/Root"));
            Assert.IsFalse(PathUtil.IsSubPathOf("C:/Test", "C:/Test/Root"));
            Assert.IsFalse(PathUtil.IsSubPathOf("C:/Test/Root/Child", "C:/Test/Root/Child2"));
        }

        [Test]
        public void IsSubPathOfFoldsDotSegments()
        {
            Assert.IsTrue(PathUtil.IsSubPathOf("C:/Test/Root/Child/../Child", "C:/Test/Root"));
            Assert.IsFalse(PathUtil.IsSubPathOf("C:/Test/Root/Child/../../Other", "C:/Test/Root"));
        }

        [Test]
        public void IsSubPathOfComparesNonLocalPathsVerbatim()
        {
            Assert.IsTrue(PathUtil.IsSubPathOf("content://com.test/folder/file.txt", "content://com.test/folder"));
            Assert.IsFalse(PathUtil.IsSubPathOf("content://com.test/folder2", "content://com.test/folder"));
            Assert.IsFalse(PathUtil.IsSubPathOf("", "C:/Test/Root"));
            Assert.IsFalse(PathUtil.IsSubPathOf("C:/Test/Root", ""));
            Assert.IsFalse(PathUtil.IsSubPathOf(null, "C:/Test/Root"));
            Assert.IsFalse(PathUtil.IsSubPathOf("C:/Test/Root", null));
        }

        [Test]
        public void IsSubPathOfHonorsPlatformCaseSensitivity()
        {
            Assert.AreEqual(IsCaseInsensitiveFileSystem, PathUtil.IsSubPathOf("C:/Test/Root/Child", "c:/test/root"));
        }

        [Test]
        public void PathEqualsIgnoresSeparatorAndTrailingSlash()
        {
            Assert.IsTrue(PathUtil.PathEquals(null, null));
            Assert.IsFalse(PathUtil.PathEquals("C:/A/B", null));
            Assert.IsFalse(PathUtil.PathEquals(null, "C:/A/B"));
            Assert.IsTrue(PathUtil.PathEquals("C:/A/B/", "C:\\A\\B"));
            Assert.IsFalse(PathUtil.PathEquals("C:/A/B", "C:/A/B/C"));
        }

        [Test]
        public void PathEqualsHonorsPlatformCaseSensitivity()
        {
            Assert.AreEqual(IsCaseInsensitiveFileSystem, PathUtil.PathEquals("C:/A/B", "c:/a/b"));
        }

        [Test]
        public void PathComparerDeduplicatesNormalizedForms()
        {
            var paths = new HashSet<string>(PathUtil.PathComparer) { "C:\\A\\B\\", "C:/A/B" };

            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual(PathUtil.PathComparer.GetHashCode("C:\\A\\B\\"), PathUtil.PathComparer.GetHashCode("C:/A/B"));
        }
    }
}
