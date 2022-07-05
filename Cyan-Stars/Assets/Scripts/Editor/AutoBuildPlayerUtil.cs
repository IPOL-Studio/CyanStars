using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatAsset.Editor;
using UnityEditor;
using UnityEngine;

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
            Packager.ExecutePackagePipeline(PkgUtil.PkgCfg.OutputPath, PkgUtil.PkgCfg.Options,
                buildTarget, PkgUtil.PkgCfg.ManifestVersion, PkgUtil.PkgCfg.IsAnalyzeRedundancy,
                true, PkgUtil.PkgCfg.CopyGroup);
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
            Debug.Log($"安装包构建完毕，路径:{path}");
        }
    }
}

