#nullable enable

using System;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor.Procedure;
using CyanStars.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars
{
    /// <summary>
    /// 临时用于选择进入制谱器编辑的谱面的弹窗
    /// </summary>
    public class Temp_ChartSelectPopup : MonoBehaviour
    {
        [SerializeField]
        private GameObject chartButtonObjectPrefab = null!;

        [SerializeField]
        private Canvas popupCanvas = null!;

        [SerializeField]
        private Button openPopupCanvasButton = null!;

        [SerializeField]
        private Button closePopupCanvasButton = null!;

        [SerializeField]
        private TMP_Text chartPackFilePathText = null!;

        [SerializeField]
        private Button importChartPackButton = null!;

        [SerializeField]
        private GameObject chartsFrame = null!;

        [SerializeField]
        private Button newChartPackButton = null!;

        [SerializeField]
        private Button newChartButton = null!;


        private ChartModule chartModule = null!;


        private void Awake()
        {
            popupCanvas.enabled = false;
            newChartButton.gameObject.SetActive(false);

            chartModule = GameRoot.GetDataModule<ChartModule>();
            if (chartModule == null)
                throw new Exception("获取谱面数据模块失败！");

            openPopupCanvasButton.onClick.AddListener(() => popupCanvas.enabled = true);
            closePopupCanvasButton.onClick.AddListener(() => popupCanvas.enabled = false);

            newChartPackButton.onClick.AddListener(() =>
                {
                    chartModule.CancelSelectChartPackData();
                    GameRoot.ChangeProcedure<ChartEditorProcedure>();
                }
            );

            importChartPackButton.onClick.AddListener(() =>
                {
                    // TODO: 移动端短期改为选择文件夹并整体复制谱包（SAF 目录），长期改用 .cyscp 单文件格式导入
                    if (!GameRoot.File.Environment.CanImportExternalFolder)
                    {
                        Debug.LogWarning("当前平台不支持导入外部谱包。");
                        chartPackFilePathText.text = "当前平台不支持导入外部谱包";
                        return;
                    }

                    GameRoot.File.OpenLoadFolderPathBrowser(async sourceFolderPath =>
                        {
                            // 整个谱包目录复制进玩家谱包目录，之后的编辑都针对这份导入副本
                            if (!chartModule.TryCopyChartPackToPlayerFolder(sourceFolderPath, out string chartPackFilePath))
                            {
                                chartPackFilePathText.text = "复制谱包到玩家数据目录失败，具体原因见日志";
                                return;
                            }

                            // 加载谱包并选中
                            chartPackFilePathText.text = chartPackFilePath;
                            await chartModule.SetSingleChartPackFromDisk(chartPackFilePath);
                            chartModule.SelectChartPackData(0);

                            // 清空旧谱面列表，生成新谱面列表
                            for (int i = chartsFrame.transform.childCount - 2; i >= 0; i--)
                            {
                                Destroy(chartsFrame.transform.GetChild(i).gameObject);
                            }

                            for (int i = 0; i < chartModule.SelectedRuntimeChartPack.ChartPackData.ChartMetaDatas.Count; i++)
                            {
                                string chartFilePath = PathUtil.Combine(chartModule.SelectedRuntimeChartPack.WorkspacePath, chartModule.SelectedRuntimeChartPack.ChartPackData.ChartMetaDatas[i].FilePath);

                                GameObject newButtonObject = Instantiate(chartButtonObjectPrefab, chartsFrame.transform);
                                newButtonObject.transform.SetSiblingIndex(chartsFrame.transform.childCount - 2);
                                Temp_ChartButton button = newButtonObject.GetComponent<Temp_ChartButton>();
                                ChartMetaData chartMetaData = chartModule.SelectedRuntimeChartPack.ChartPackData.ChartMetaDatas[i];
                                string difficultText = GetDifficultText(chartMetaData.Difficulty);
                                button.Text.text = $"【{difficultText}】{chartMetaData.FilePath}";
                                int index = i;
                                button.Button.onClick.AddListener(async () =>
                                    {
                                        chartModule.PreSelectChartData(index);
                                        GameRoot.ChangeProcedure<ChartEditorProcedure>();
                                    }
                                );
                            }

                            RectTransform rectTransform = chartsFrame.GetComponent<RectTransform>();
                            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                            newChartButton.gameObject.SetActive(true);
                        },
                        null,
                        "选择谱包文件夹"
                    );
                }
            );

            newChartButton.onClick.AddListener(() =>
                {
                    chartModule.CancelSelectChartData();
                    GameRoot.ChangeProcedure<ChartEditorProcedure>();
                }
            );
        }

        private string GetDifficultText(ChartDifficulty? difficulty)
        {
            return difficulty switch
            {
                ChartDifficulty.KuiXing => "窥星",
                ChartDifficulty.QiMing => "启明",
                ChartDifficulty.TianShu => "天枢",
                ChartDifficulty.WuYin => "无垠",
                null => "未定义",
                _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
            };
        }

        private void OnDestroy()
        {
            openPopupCanvasButton.onClick.RemoveAllListeners();
            closePopupCanvasButton.onClick.RemoveAllListeners();
        }
    }
}
