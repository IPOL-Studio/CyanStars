#nullable enable

using System.Collections.Generic;
using System.IO;
using CyanStars.Framework.File;
using CyanStars.Framework.File.AppPaths;
using CyanStars.Utils;
using NUnit.Framework;

namespace CyanStars.Test.EditMode
{
    public class AppPathSettingsTest
    {
        private static readonly IReadOnlyDictionary<AppPathKind, string> EmptyOtherPaths =
            new Dictionary<AppPathKind, string>();

        private string rootPath = null!;
        private string persistentRoot = null!;
        private string defaultTemporaryRoot = null!;


        [SetUp]
        public void SetUp()
        {
            rootPath = TestFolderUtil.CreateTestRoot();
            persistentRoot = Path.Combine(rootPath, "persistent");
            defaultTemporaryRoot = Path.Combine(rootPath, "default-temp");

            Directory.CreateDirectory(persistentRoot);
            Directory.CreateDirectory(defaultTemporaryRoot);
        }

        [TearDown]
        public void TearDown()
        {
            TestFolderUtil.DeleteTestRoot(rootPath);
        }


        [Test]
        public void LoadMissingFileReturnsEmptySettings()
        {
            AppPathSettings settings = AppPathSettingsStore.Load(persistentRoot);

            Assert.IsNull(settings.GetOverride(AppPathKind.PlayerChartPacks));
            Assert.IsNull(settings.GetOverride(AppPathKind.TemporaryCacheRoot));
        }

        [Test]
        public void StoreRoundTripsOverrides()
        {
            string chartPacksPath = Path.Combine(rootPath, "charts");
            var settings = new AppPathSettings();
            settings.SetOverride(AppPathKind.PlayerChartPacks, chartPacksPath);

            Assert.IsTrue(AppPathSettingsStore.TrySave(persistentRoot, settings, out string error), error);

            AppPathSettings loaded = AppPathSettingsStore.Load(persistentRoot);

            Assert.AreEqual(chartPacksPath, loaded.GetOverride(AppPathKind.PlayerChartPacks));
            Assert.IsNull(loaded.GetOverride(AppPathKind.TemporaryCacheRoot));
        }

        [Test]
        public void LoadIgnoresExplicitNullOverrides()
        {
            string settingsFilePath = AppPathSettingsStore.GetSettingsFilePath(persistentRoot);
            Directory.CreateDirectory(Path.GetDirectoryName(settingsFilePath)!);
            File.WriteAllText(settingsFilePath, "{ \"Overrides\": null }");

            AppPathSettings loaded = AppPathSettingsStore.Load(persistentRoot);

            Assert.IsNotNull(loaded.Overrides);
            Assert.AreEqual(0, loaded.Overrides.Count);
        }

        [Test]
        public void ValidatorAcceptsWritableFolderAndNormalizesPath()
        {
            string candidatePath = Path.Combine(rootPath, "charts") + Path.DirectorySeparatorChar;

            Assert.IsTrue(AppPathValidator.TryValidate(
                AppPathKind.PlayerChartPacks,
                candidatePath,
                persistentRoot,
                null,
                null,
                EmptyOtherPaths,
                out string normalizedPath,
                out string error), error);

            Assert.AreEqual(PathUtil.Normalize(Path.GetFullPath(candidatePath)), normalizedPath);
            Assert.IsTrue(Directory.Exists(normalizedPath));
            Assert.IsEmpty(Directory.GetFiles(normalizedPath, ".cys_write_test_*"));
        }

        [Test]
        public void ValidatorRejectsRelativeAndRootPaths()
        {
            Assert.IsFalse(AppPathValidator.TryValidate(
                AppPathKind.PlayerChartPacks,
                "relative/path",
                persistentRoot,
                null,
                null,
                EmptyOtherPaths,
                out _,
                out _));

            string? root = Path.GetPathRoot(rootPath);
            Assert.IsNotNull(root);
            Assert.IsFalse(AppPathValidator.TryValidate(
                AppPathKind.PlayerChartPacks,
                root!,
                persistentRoot,
                null,
                null,
                EmptyOtherPaths,
                out _,
                out _));
        }

        [Test]
        public void ValidatorRejectsOverlappingAppPaths()
        {
            string chartsPath = Path.Combine(rootPath, "charts");
            var otherPaths = new Dictionary<AppPathKind, string>
            {
                [AppPathKind.TemporaryCacheRoot] = Path.Combine(chartsPath, "temp")
            };

            Assert.IsFalse(AppPathValidator.TryValidate(
                AppPathKind.PlayerChartPacks,
                chartsPath,
                persistentRoot,
                null,
                null,
                otherPaths,
                out _,
                out _));
        }

        [Test]
        public void ValidatorRejectsTemporaryRootInsidePersistentRoot()
        {
            string candidatePath = Path.Combine(persistentRoot, "temp");

            Assert.IsFalse(AppPathValidator.TryValidate(
                AppPathKind.TemporaryCacheRoot,
                candidatePath,
                persistentRoot,
                null,
                null,
                EmptyOtherPaths,
                out _,
                out _));
        }
    }
}
