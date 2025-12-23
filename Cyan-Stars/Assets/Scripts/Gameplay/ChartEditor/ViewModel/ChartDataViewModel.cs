#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.BindableProperty;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ChartDataViewModel : BaseViewModel
    {
        private readonly BindableProperty<bool> chartDataCanvasVisibility;
        private readonly BindableProperty<ChartDifficulty?> chartDifficulty;
        private readonly BindableProperty<string> chartLevelString;
        private readonly BindableProperty<string> readyBeatCountString;

        public IReadonlyBindableProperty<bool> ChartDataCanvasVisibility => chartDataCanvasVisibility;
        public IReadonlyBindableProperty<ChartDifficulty?> ChartDifficulty => chartDifficulty;
        public IReadonlyBindableProperty<string> ChartLevelString => chartLevelString;
        public IReadonlyBindableProperty<string> ReadyBeatCountString => readyBeatCountString;


        public ChartDataViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            chartDataCanvasVisibility = new BindableProperty<bool>(Model.ChartDataCanvasVisibility.Value);

            ChartMetadata metaData = Model.ChartPackData.ChartMetaDatas[Model.ChartMetadataIndex];

            chartDifficulty = new BindableProperty<ChartDifficulty?>(metaData.Difficulty);
            chartLevelString = new BindableProperty<string>(metaData.Level);
            readyBeatCountString = new BindableProperty<string>(Model.ChartData.ReadyBeat.ToString());

            Model.ChartDataCanvasVisibility.OnValueChanged += visibility =>
            {
                chartDataCanvasVisibility.Value = visibility;
            };
            Model.OnChartBasicDataChanged += (chartPackData, chartData) =>
            {
                chartDifficulty.Value = chartPackData.ChartMetaDatas[Model.ChartMetadataIndex].Difficulty;
                chartLevelString.Value = chartPackData.ChartMetaDatas[Model.ChartMetadataIndex].Level;
                readyBeatCountString.Value = chartData.ReadyBeat.ToString();
            };
        }

        public void CloseCanvas()
        {
            if (!chartDataCanvasVisibility.Value)
                return;

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.ChartDataCanvasVisibility.Value = false;
                }, () =>
                {
                    Model.ChartDataCanvasVisibility.Value = true;
                }
            ));
        }

        public void SetChartDifficulty(ChartDifficulty? newDifficulty)
        {
            var metaDatas = Model.ChartPackData.ChartMetaDatas;
            var oldDifficulty = metaDatas[Model.ChartMetadataIndex].Difficulty;

            if (newDifficulty == oldDifficulty)
                return;

            // 如果 newDifficulty 不为 null，则不允许与谱包中其他谱面的难度重复
            if (newDifficulty != null)
            {
                for (int i = 0; i < metaDatas.Count; i++)
                {
                    if (Model.ChartMetadataIndex == i)
                        continue;

                    if (metaDatas[i].Difficulty != newDifficulty)
                        continue;

                    Debug.LogWarning($"谱包有其他谱面已经使用了难度 {newDifficulty}，无法修改到目标难度！");
                    chartDifficulty.ForceNotify();
                    return;
                }
            }

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.SetChartDifficulty(newDifficulty);
                },
                () =>
                {
                    Model.SetChartDifficulty(oldDifficulty);
                }
            ));
        }

        public void SetChartLevelString(string newLevel)
        {
            var metaDatas = Model.ChartPackData.ChartMetaDatas;
            var oldLevel = metaDatas[Model.ChartMetadataIndex].Level;

            if (newLevel == oldLevel)
                return;

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.SetChartLevel(newLevel);
                },
                () =>
                {
                    Model.SetChartLevel(oldLevel);
                }
            ));
        }

        public void SetReadyBeatCount(string newBeatCount)
        {
            if (!int.TryParse(newBeatCount, out int newBeatCountInt) || newBeatCountInt < 0)
            {
                readyBeatCountString.ForceNotify();
                return;
            }

            var oldBeatIntCount = Model.ChartData.ReadyBeat;
            if (newBeatCountInt == oldBeatIntCount)
                return;

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.SetChartReadyBeatCount(newBeatCountInt);
                },
                () =>
                {
                    Model.SetChartReadyBeatCount(oldBeatIntCount);
                }
            ));
        }
    }
}
