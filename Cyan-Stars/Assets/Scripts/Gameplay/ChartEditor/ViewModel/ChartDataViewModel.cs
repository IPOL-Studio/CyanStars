#nullable enable

using System.Collections.Generic;
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

            ChartMetadata metaData = Model.ChartPackData.Value.ChartMetaDatas[Model.ChartMetadataIndex];

            chartDifficulty = new BindableProperty<ChartDifficulty?>(metaData.Difficulty);
            chartLevelString = new BindableProperty<string>(metaData.Level);
            readyBeatCountString = new BindableProperty<string>(Model.ChartData.Value.ReadyBeat.ToString());

            Model.ChartDataCanvasVisibility.OnValueChanged += data =>
            {
                chartDataCanvasVisibility.Value = data;
            };
            Model.ChartPackData.OnValueChanged += data =>
            {
                chartDifficulty.Value = data.ChartMetaDatas[Model.ChartMetadataIndex].Difficulty;
                chartLevelString.Value = data.ChartMetaDatas[Model.ChartMetadataIndex].Level;
            };
            Model.ChartData.OnValueChanged += data =>
            {
                readyBeatCountString.Value = data.ReadyBeat.ToString();
            };
        }

        public void CloseCanvas()
        {
            if (!chartDataCanvasVisibility.Value)
            {
                return;
            }

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.ChartDataCanvasVisibility.Value = false;
                }, () =>
                {
                    Model.ChartDataCanvasVisibility.Value = true;
                }
            ));
        }

        public void SetChartDifficulty(ChartDifficulty? difficulty)
        {
            if (difficulty == chartDifficulty.Value)
            {
                return;
            }

            var metaDatas = Model.ChartPackData.Value.ChartMetaDatas;
            if (difficulty != null)
            {
                for (int i = 0; i < metaDatas.Count; i++)
                {
                    if (Model.ChartMetadataIndex == i)
                    {
                        continue;
                    }

                    if (metaDatas[i].Difficulty == difficulty)
                    {
                        Debug.LogWarning($"谱包有其他谱面已经使用了难度 {difficulty}，无法修改到目标难度！");
                        chartDifficulty.ForceNotify();
                        return;
                    }
                }
            }

            var oldChartPackData = Model.ChartPackData.Value;
            var newMetaDatas = new List<ChartMetadata>(oldChartPackData.ChartMetaDatas);
            newMetaDatas[Model.ChartMetadataIndex].Difficulty = difficulty;
            var newChartPackData = new ChartPackData(
                oldChartPackData.Title, oldChartPackData.MusicVersionDatas, oldChartPackData.BpmGroup,
                oldChartPackData.MusicPreviewStartBeat, oldChartPackData.MusicPreviewEndBeat, oldChartPackData.CoverFilePath,
                oldChartPackData.CropStartPosition, oldChartPackData.CropHeight, newMetaDatas
            );

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.ChartPackData.Value = newChartPackData;
                },
                () =>
                {
                    Model.ChartPackData.Value = oldChartPackData;
                }
            ));
        }

        public void SetChartLevelString(string level)
        {
            if (level == ChartLevelString.Value)
            {
                return;
            }

            ChartPackData oldChartPackData = Model.ChartPackData.Value;
            var newMetaDatas = new List<ChartMetadata>(oldChartPackData.ChartMetaDatas);
            newMetaDatas[Model.ChartMetadataIndex].Level = level;
            var newChartPackData = new ChartPackData(
                oldChartPackData.Title, oldChartPackData.MusicVersionDatas, oldChartPackData.BpmGroup,
                oldChartPackData.MusicPreviewStartBeat, oldChartPackData.MusicPreviewEndBeat, oldChartPackData.CoverFilePath,
                oldChartPackData.CropStartPosition, oldChartPackData.CropHeight, newMetaDatas
            );

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.ChartPackData.Value = newChartPackData;
                },
                () =>
                {
                    Model.ChartPackData.Value = oldChartPackData;
                }
            ));
        }

        public void SetReadyBeatCount(string beatCount)
        {
            if (readyBeatCountString.Value == beatCount)
            {
                return;
            }

            if (!int.TryParse(beatCount, out int beatCountInt) || beatCountInt < 0)
            {
                readyBeatCountString.ForceNotify();
                return;
            }

            ChartData oldChartData = Model.ChartData.Value;
            var newChartData = new ChartData(
                beatCountInt, oldChartData.SpeedGroupDatas,
                oldChartData.Notes, oldChartData.TrackDatas
            );

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.ChartData.Value = newChartData;
                },
                () =>
                {
                    Model.ChartData.Value = oldChartData;
                }
            ));
        }
    }
}
