#nullable enable

using System;
using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.Chart.Loading;
using CyanStars.Framework.File;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Utils;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.Management
{
    /// <summary>
    /// 把制谱器里编辑中的数据写进磁盘
    /// </summary>
    /// <remarks>
    /// <para>谱包、谱面在内存里序列化并校验通过后，统一经 <see cref="PlatformFilePortal"/> 落盘，
    /// 本类不再自己调用 <see cref="System.IO"/> 写文件</para>
    /// <para>只有本次会话导入或改过的资源才有句柄；没有句柄的资源说明它没有被改过，
    /// 磁盘上的旧文件继续有效，不必重写</para>
    /// </remarks>
    public static class ChartEditorSaver
    {
        private const string ChartPackFileName = ChartPackDataLoader.ChartPackFileName;


        /// <summary>
        /// 把谱包、谱面、资源文件覆盖保存到磁盘
        /// </summary>
        /// <param name="editorModel">编辑会话 Model</param>
        /// <returns>是否全部保存成功</returns>
        /// <remarks>资源缺失或写失败时不写谱包和谱面：避免元数据指向一份没有内容的资源</remarks>
        public static bool SaveChartAndAssetsToDisk(ChartEditorModel editorModel)
        {
            ChartPackDataEditorModel chartPackDataEditorModel = editorModel.ChartPackData.CurrentValue;
            ChartDataEditorModel chartDataEditorModel = editorModel.ChartData.CurrentValue;

            // 先在内存中校验，确保文件能够正确序列化
            string chartPackFilePath;
            string chartFilePath;
            string chartPackJson;
            string chartJson;

            try
            {
                ChartPackData chartPackData = chartPackDataEditorModel.ToChartPackData();
                ChartData chartData = chartDataEditorModel.ToChartData();

                chartPackFilePath = PathUtil.Combine(editorModel.WorkspacePath, ChartPackFileName);
                chartFilePath = editorModel.GetAssetAbsolutePath(chartPackData.ChartMetaDatas[editorModel.ChartMetaDataIndex].FilePath);

                if (!JsonSerializer.TrySerialize(chartPackData, out chartPackJson) ||
                    !JsonSerializer.TrySerialize(chartData, out chartJson))
                {
                    Debug.LogError("谱包或谱面序列化失败，已跳过保存");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"序列化谱包谱面时出现异常：{e.Message}");
                return false;
            }

            HashSet<string> assetAbsolutePaths = GetAssetAbsolutePaths(editorModel, chartPackDataEditorModel);

            List<string> missingAssets = CollectMissingAssets(editorModel, assetAbsolutePaths);
            if (missingAssets.Count > 0)
            {
                foreach (string missingPath in missingAssets)
                    Debug.LogError($"谱包引用了资源 {missingPath}，但它既不在磁盘上、也不是本次会话导入的，已跳过保存。");

                return false;
            }

            if (!PlatformFilePortal.TrySaveAll(editorModel.GetAllAssetHandles(), true))
            {
                Debug.LogError("保存资源文件失败，已跳过谱包谱面的写入");
                return false;
            }

            // TODO: 定期在后台把整个谱包工作区备份到临时文件路径
            // 谱面被谱包引用，因此先写谱面
            if (!TryWriteJsonFile(chartFilePath, chartJson) || !TryWriteJsonFile(chartPackFilePath, chartPackJson))
                return false;

            Debug.Log("已保存谱面。");
            return true;
        }

        /// <summary>
        /// 把谱包引用的资源相对路径换算成工作区里的绝对路径
        /// </summary>
        private static HashSet<string> GetAssetAbsolutePaths(
            ChartEditorModel editorModel,
            ChartPackDataEditorModel chartPackDataEditorModel
        )
        {
            var absolutePaths = new HashSet<string>(PathUtil.PathComparer);

            foreach (string relativePath in chartPackDataEditorModel.GetAssetRelativePaths())
                absolutePaths.Add(editorModel.GetAssetAbsolutePath(relativePath));

            return absolutePaths;
        }

        /// <summary>
        /// 找出「当前数据仍然引用、但磁盘上没有、也没有句柄」的资源
        /// </summary>
        /// <returns>缺少内容的资源路径，顺序不保证</returns>
        /// <remarks>本次会话没有导入、也没有改过的资源不需要句柄：磁盘上的旧文件就是它的内容</remarks>
        private static List<string> CollectMissingAssets(
            ChartEditorModel editorModel,
            HashSet<string> assetAbsolutePaths
        )
        {
            var missingAssets = new List<string>();

            foreach (string assetAbsolutePath in assetAbsolutePaths)
            {
                if (FileManager.IsFileExists(assetAbsolutePath) ||
                    editorModel.FindAssetHandle(assetAbsolutePath) != null)
                    continue;

                missingAssets.Add(assetAbsolutePath);
            }

            return missingAssets;
        }

        /// <summary>
        /// 把序列化好的 json 写进目标路径
        /// </summary>
        /// <returns>是否写入成功</returns>
        private static bool TryWriteJsonFile(string targetFilePath, string json)
        {
            return PlatformFilePortal.TryWriteTextToPath(targetFilePath, json);
        }
    }
}
