#nullable enable

using System;
using System.Collections.Generic;
using CatAsset.Runtime;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor.Procedure;
using CyanStars.Utils;
using SimpleFileBrowser;
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
                    GameRoot.File.OpenLoadFilePathBrowser(async (path) =>
                        {
                            path = path.Replace('\\', '/');

                            // 加载谱包并选中
                            chartPackFilePathText.text = path;
                            await chartModule.SetChartPackDataFromDesk(path);
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
                                        await chartModule.SelectChartDataAsync(index);
                                        GameRoot.ChangeProcedure<ChartEditorProcedure>();
                                    }
                                );
                            }

                            RectTransform rectTransform = chartsFrame.GetComponent<RectTransform>();
                            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                            newChartButton.gameObject.SetActive(true);
                        },
                        null,
                        "打开谱包索引文件",
                        false,
                        new[] { new FileBrowser.Filter("谱包索引文件", ".json") }
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
