using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CatAsset.Editor;
using UnityEditor;
using UnityEngine;
using BuildPipeline = UnityEditor.BuildPipeline;
using Debug = UnityEngine.Debug;

namespace CyanStars.Editor
{
    /// <summary>
    /// 自动构建安装包的工具类
    /// </summary>
    public static class AutoBuildPlayerUtil
    {

        [MenuItem("CyanStars工具箱/构建安装包/Windows")]
        private static void BuildPlayerWithWin64()
        {
            AutoBuildPlayer(BuildTarget.StandaloneWindows64);
        }

        [MenuItem("CyanStars工具箱/构建安装包/Android")]
        private static void BuildPlayerWithAndroid()
        {
            AutoBuildPlayer(BuildTarget.Android);
        }

        private static void AutoBuildPlayer(BuildTarget buildTarget)
        {
            BuildAssetBundle(buildTarget);
            BuildPlayer(buildTarget);
        }

        private static void BuildAssetBundle(BuildTarget buildTarget)
        {
            BundleBuildConfigSO.Instance.RefreshBundleBuildInfos();
            CatAsset.Editor.BuildPipeline.BuildBundles(buildTarget, false);
        }


        private static void BuildPlayer(BuildTarget buildTarget)
        {
            string path = $"./BuildPlayer/{buildTarget}/CyanStars";
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows64:
                    path += ".exe";
                    break;
                case BuildTarget.Android:
                    path += ".apk";
                    break;
            }
            EditorBuildSettingsScene[] targetScenes = new EditorBuildSettingsScene[1];
            targetScenes[0] = new EditorBuildSettingsScene("Assets/CyanStarsEntry.unity", true);
            BuildPipeline.BuildPlayer(targetScenes, path, buildTarget, BuildOptions.Development);
            Open(new FileInfo(path).DirectoryName);
        }

        /// <summary>
        /// 打开指定目录
        /// </summary>
        private static void Open(string directory)
        {
            directory = string.Format("\"{0}\"", directory);

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                Process.Start("Explorer.exe", directory.Replace('/', '\\'));
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                Process.Start("open", directory);
            }
        }
    }
}

