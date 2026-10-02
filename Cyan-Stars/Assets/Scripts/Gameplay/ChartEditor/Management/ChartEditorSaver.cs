#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
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
    public static class ChartEditorSaver
    {
        /// <summary>
        /// 把谱包、谱面、资源文件覆盖保存到磁盘
        /// </summary>
        /// <param name="workspacePath">工作区绝对路径（谱包索引文件所在目录）</param>
        /// <param name="chartMetaDataIndex">谱面文件在谱包元数据里的下标</param>
        /// <param name="chartPackDataEditorModel">谱包实例</param>
        /// <param name="chartDataEditorModel">谱面实例</param>
        /// <param name="assetStore">本次编辑会话的缓存区，里面的暂存文件会被写进工作区</param>
        /// <returns>是否全部保存成功</returns>
        public static bool SaveChartAndAssetsToDisk(
            string workspacePath,
            int chartMetaDataIndex,
            ChartPackDataEditorModel chartPackDataEditorModel,
            ChartDataEditorModel chartDataEditorModel,
            TempFileStore assetStore
        )
        {
            // 先在内存中校验，确保文件能够正确序列化
            string chartPackFilePath;
            string chartFilePath;
            string chartPackJson;
            string chartJson;

            try
            {
                ChartPackData chartPackData = chartPackDataEditorModel.ToChartPackData();
                ChartData chartData = chartDataEditorModel.ToChartData();

                chartPackFilePath = PathUtil.Combine(workspacePath, ChartPackDataLoader.ChartPackFileName);
                chartFilePath = PathUtil.Combine(workspacePath, chartPackData.ChartMetaDatas[chartMetaDataIndex].FilePath);

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

            // 落盘范围以当前谱包引用的资源为准，历史记录引用的数据不写回磁盘
            HashSet<string> assetAbsolutePaths = GetAssetAbsolutePaths(workspacePath, chartPackDataEditorModel);

            // 缺失的资源只报错，不中止保存
            foreach (string missingPath in assetStore.CollectMissingTargets(assetAbsolutePaths))
                Debug.LogError($"谱包引用了资源 {missingPath}，但它既不在磁盘上、也不在本次会话的缓存区里。");

            if (!assetStore.ApplyAll(assetAbsolutePaths))
            {
                Debug.LogError("保存资源文件失败，已跳过谱包谱面的写入");
                return false;
            }

            // TODO: 定期在后台把整个谱包工作区备份到临时文件路径
            // 覆盖旧文件，不产生临时文件或备份；谱面被谱包引用，因此先写谱面
            try
            {
                string? chartDirectory = System.IO.Path.GetDirectoryName(chartFilePath);
                if (!string.IsNullOrEmpty(chartDirectory))
                    System.IO.Directory.CreateDirectory(chartDirectory);

                // 固定写无 BOM 的 UTF-8，不用平台默认编码
                System.IO.File.WriteAllText(chartFilePath, chartJson, new UTF8Encoding(false));
                System.IO.File.WriteAllText(chartPackFilePath, chartPackJson, new UTF8Encoding(false));
            }
            catch (Exception e)
            {
                Debug.LogError($"写入谱包或谱面时出现异常：{e}");
                return false;
            }

            Debug.Log("已保存谱面。");
            return true;
        }

        /// <summary>
        /// 把谱包引用的资源相对路径换算成工作区里的绝对路径
        /// </summary>
        /// <returns>绝对路径集合，交给 <see cref="TempFileStore.ApplyAll"/> 决定落盘哪些暂存文件</returns>
        private static HashSet<string> GetAssetAbsolutePaths(
            string workspacePath,
            ChartPackDataEditorModel chartPackDataEditorModel
        )
        {
            var absolutePaths = new HashSet<string>(PathUtil.PathComparer);

            foreach (string relativePath in chartPackDataEditorModel.GetAssetRelativePaths())
                absolutePaths.Add(PathUtil.Combine(workspacePath, relativePath));

            return absolutePaths;
        }
    }
}
