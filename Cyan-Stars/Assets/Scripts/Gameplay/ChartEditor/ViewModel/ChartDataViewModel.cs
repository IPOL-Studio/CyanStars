#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ChartDataViewModel : BaseViewModel
    {
        private readonly ChartMetaDataEditorModel MetaData;
        private readonly ChartDataEditorModel ChartData;

        public ReadOnlyReactiveProperty<bool> ChartDataCanvasVisibility { get; }
        public ReadOnlyReactiveProperty<ChartDifficulty?> ChartDifficulty { get; }
        public ReadOnlyReactiveProperty<string> ChartLevelString { get; }
        public ReadOnlyReactiveProperty<string> ReadyBeatCountString { get; }


        public ChartDataViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            MetaData = Model.ChartPackData.CurrentValue.ChartMetaDatas[Model.ChartMetaDataIndex];
            ChartData = Model.ChartData.CurrentValue;

            ChartDataCanvasVisibility = Model.ChartDataCanvasVisibility
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);

            ChartDifficulty = MetaData.Difficulty
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);

            ChartLevelString = MetaData.Level
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);

            ReadyBeatCountString = ChartData.ReadyBeat
                .Select(beat => beat.ToString())
                .ToReadOnlyReactiveProperty(ChartData.ReadyBeat.Value.ToString())
                .AddTo(base.Disposables);
        }

        public void CloseCanvas()
        {
            if (!ChartDataCanvasVisibility.CurrentValue)
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
            var metaDatas = Model.ChartPackData.CurrentValue.ChartMetaDatas;
            var oldDifficulty = metaDatas[Model.ChartMetaDataIndex].Difficulty.Value;

            if (newDifficulty == oldDifficulty)
                return;

            // 如果 newDifficulty 不为 null，则不允许与谱包中其他谱面的难度重复
            if (newDifficulty != null)
            {
                for (int i = 0; i < metaDatas.Count; i++)
                {
                    if (Model.ChartMetaDataIndex == i)
                        continue;

                    if (metaDatas[i].Difficulty.Value != newDifficulty)
                        continue;

                    Debug.LogWarning($"谱包有其他谱面已经使用了难度 {newDifficulty}，无法修改到目标难度！");
                    MetaData.Difficulty.ForceNotify();
                    return;
                }
            }

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    MetaData.Difficulty.Value = newDifficulty;
                },
                () =>
                {
                    MetaData.Difficulty.Value = oldDifficulty;
                }
            ));
        }

        public void SetChartLevelString(string newLevel)
        {
            var metaData = Model.ChartPackData.CurrentValue.ChartMetaDatas[Model.ChartMetaDataIndex];
            var oldLevel = metaData.Level.Value;

            if (newLevel == oldLevel)
                return;

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    metaData.Level.Value = newLevel;
                },
                () =>
                {
                    metaData.Level.Value = oldLevel;
                }
            ));
        }

        public void SetReadyBeatCount(string newBeatCount)
        {
            if (!int.TryParse(newBeatCount, out int newBeatCountInt) || newBeatCountInt < 0)
            {
                ChartData.ReadyBeat.ForceNotify();
                return;
            }

            var oldBeatIntCount = ChartData.ReadyBeat.Value;
            if (newBeatCountInt == oldBeatIntCount)
                return;

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    ChartData.ReadyBeat.Value = newBeatCountInt;
                },
                () =>
                {
                    ChartData.ReadyBeat.Value = oldBeatIntCount;
                }
            ));
        }
    }
}
