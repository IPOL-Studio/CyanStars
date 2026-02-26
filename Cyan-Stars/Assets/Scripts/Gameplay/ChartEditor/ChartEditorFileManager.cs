#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Framework.File;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Utils;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor
{
    /// <summary>
    /// 用于制谱器的文件管理类
    /// </summary>
    public class ChartEditorFileManager : MonoBehaviour
    {
        public const string ChartPackAssetsFolderName = "Assets";

        private static string TempFolderPath => PathUtil.Combine(Application.persistentDataPath, "TempSession", "ChartEditorFileManager");
        private static readonly Dictionary<string, TempFileHandler> TempPathToHandlerMap = new(); // 缓存路径->句柄 映射表，一定是齐全的
        private static readonly Dictionary<string, TempFileHandler> TargetPathToHandlerMap = new(); // 目标路径->句柄 映射表，不一定齐全（文件缓存了但没有指定映射路径，用于制谱器可撤销操作时缓存文件）


        public void Start()
        {
            // 清理旧的缓存路径
            if (Directory.Exists(TempFolderPath))
            {
                try
                {
                    Directory.Delete(TempFolderPath, true);
                    Debug.Log($"已清除缓存文件夹：{TempFolderPath}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"在删除缓存文件夹时捕获了异常：{e.Message}");
                }
            }
        }


        /// <summary>
        /// 将原始文件复制到缓存区
        /// </summary>
        /// <param name="originFilePath">原始文件的绝对路径（含后缀名）</param>
        /// <param name="targetFilePath">目标文件的绝对路径（含后缀名）</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">originFilePath 为空</exception>
        /// <exception cref="Exception">在缓存文件时出错</exception>
        public static IReadonlyTempFileHandler TempFile(string originFilePath, string? targetFilePath = null)
        {
            if (string.IsNullOrEmpty(originFilePath))
                throw new ArgumentNullException(nameof(originFilePath));
            if (string.IsNullOrEmpty(targetFilePath))
                targetFilePath = null;

            var handler = new TempFileHandler(originFilePath, TempFolderPath, targetFilePath);
            if (handler.State == TempFileHandlerState.Unavailable || handler.TempFilePath == null)
                throw new Exception("缓存文件时出错");

            TempPathToHandlerMap[handler.TempFilePath] = handler;

            if (handler.TargetFilePath != null)
                TargetPathToHandlerMap[handler.TargetFilePath] = handler;

            return handler;
        }

        /// <summary>
        /// 更新句柄的目标文件路径和映射表记录
        /// </summary>
        /// <param name="handler">句柄</param>
        /// <param name="targetFilePath">绝对目标路径</param>
        /// <remarks>如果传入的路径不为 null，且原先有其他指向此目标路径的句柄，原有句柄的目标路径将被设为 null</remarks>
        public static void UpdateTargetFilePath(TempFileHandler handler, string? targetFilePath)
        {
            if (handler.State == TempFileHandlerState.Unavailable)
            {
                Debug.LogWarning("句柄已卸载，无法更新其目标文件路径！");
                return;
            }

            if (handler.TargetFilePath == targetFilePath)
                return;

            if (targetFilePath != null && string.IsNullOrEmpty(targetFilePath))
                targetFilePath = null;

            // 如果有其他句柄指向此新路径，应该先更新其他句柄
            if (targetFilePath != null && TargetPathToHandlerMap.TryGetValue(targetFilePath, out TempFileHandler? otherHandler))
                otherHandler.TargetFilePath = null;

            // 清除旧路径映射
            if (!string.IsNullOrEmpty(handler.TargetFilePath))
                TargetPathToHandlerMap.Remove(handler.TargetFilePath);

            // 更新句柄路径和映射表
            handler.TargetFilePath = targetFilePath;
            if (targetFilePath != null)
                TargetPathToHandlerMap[targetFilePath] = handler;
        }

        /// <summary>
        /// 根据目标文件路径获取句柄
        /// </summary>
        /// <param name="targetPath">目标文件路径</param>
        /// <returns>句柄，找不到时返回 null</returns>
        public static IReadonlyTempFileHandler? GetHandlerByTargetPath(string targetPath)
        {
            return TargetPathToHandlerMap.GetValueOrDefault(targetPath);
        }

        /// <summary>
        /// 将谱包、谱面、资源文件保存并覆盖到磁盘
        /// </summary>
        /// <param name="workspacePath">工作区绝对路径（谱包索引文件所在的目录）</param>
        /// <param name="chartMetaDataIndex">谱面文件在谱包元数据中的下标</param>
        /// <param name="chartPackDataEditorModel">谱包实例</param>
        /// <param name="chartDataEditorModel">谱面实例</param>
        /// <returns></returns>
        public static bool SaveChartAndAssetsToDesk(string workspacePath,
                                                    int chartMetaDataIndex,
                                                    ChartPackDataEditorModel chartPackDataEditorModel,
                                                    ChartDataEditorModel chartDataEditorModel)
        {
            ChartPackData chartPackData = chartPackDataEditorModel.ToChartPackData();
            ChartData chartData = chartDataEditorModel.ToChartData();

            try
            {
                string chartPackFilePath = PathUtil.Combine(workspacePath, ChartModule.ChartPackFileName);
                GameRoot.File.SerializationToJson(chartPackData, chartPackFilePath);

                string chartFilePath = PathUtil.Combine(workspacePath, chartPackData.ChartMetaDatas[chartMetaDataIndex].FilePath);
                GameRoot.File.SerializationToJson(chartData, chartFilePath);

                foreach (var kvp in TargetPathToHandlerMap)
                {
                    var tempFilePath = kvp.Value.TempFilePath;
                    string dir = Path.GetDirectoryName(kvp.Key);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    File.Copy(tempFilePath, kvp.Key, true);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"序列化谱包谱面时出现异常：{e.Message}");
                return false;
            }
        }
    }
}
