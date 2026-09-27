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
    /// 保存顺序：两份 json 先序列化到内存 → 资源文件写进工作区 → 最后提交元数据，
    /// 且谱面必须排在谱包之前：谱包里的 <c>ChartMetaDatas</c> 引用着谱面文件，
    /// 新建谱面时该文件在磁盘上还不存在。
    /// </remarks>
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
        /// <remarks>保存后缓存区里的句柄和映射不变，撤销/重做仍指向同一份暂存副本</remarks>
        public static bool SaveChartAndAssetsToDisk(string workspacePath,
            int chartMetaDataIndex,
            ChartPackDataEditorModel chartPackDataEditorModel,
            ChartDataEditorModel chartDataEditorModel,
            TempFileStore assetStore)
        {
            // 1. 先把两份 json 都序列化到内存，任何一份失败都不会动磁盘上的文件
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

                if (!JsonFileSerializer.TrySerialize(chartPackData, out chartPackJson) ||
                    !JsonFileSerializer.TrySerialize(chartData, out chartJson))
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

            // 2. 把资源文件写进工作区。落盘范围以当前谱包引用的资源为准：
            //    缓存区里已被删除或已撤销的历史残留不写回磁盘
            HashSet<string> assetAbsolutePaths = GetAssetAbsolutePaths(workspacePath, chartPackDataEditorModel);

            // 先报告「谱包引用了、但磁盘上和缓存区里都没有」的资源。这里只报错、不中止保存
            foreach (string missingPath in assetStore.CollectMissingTargets(assetAbsolutePaths))
                Debug.LogError($"谱包引用了资源 {missingPath}，但它既不在磁盘上、也不在本次会话的缓存区里。");

            if (!assetStore.ApplyAll(assetAbsolutePaths))
            {
                Debug.LogError("保存资源文件失败，已跳过谱包谱面的写入");
                return false;
            }

            // 3. 最后提交元数据
            //    由于谱包的 ChartMetaDatas 引用着谱面文件，因此先写谱面
            var transaction = new FileWriteTransaction();
            bool allPrepared =
                transaction.TryWriteText(chartFilePath, chartJson) &
                transaction.TryWriteText(chartPackFilePath, chartPackJson);

            if (!allPrepared)
            {
                transaction.Abort();
                Debug.LogError("准备谱包或谱面内容失败，已跳过保存");
                return false;
            }

            if (!transaction.CommitAll())
            {
                Debug.LogError("谱包或谱面写入失败。");
                return false;
            }

            Debug.Log("已保存谱面。");
            return true;
        }

        /// <summary>
        /// 把谱包引用的资源相对路径换算成工作区里的绝对路径
        /// </summary>
        /// <returns>绝对路径集合，交给 <see cref="TempFileStore.ApplyAll"/> 决定落盘哪些暂存文件</returns>
        /// <remarks>
        /// 相对路径 → 绝对路径必须与其它调用方写法一致，否则暂存文件找不到自己的目标路径
        /// </remarks>
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
