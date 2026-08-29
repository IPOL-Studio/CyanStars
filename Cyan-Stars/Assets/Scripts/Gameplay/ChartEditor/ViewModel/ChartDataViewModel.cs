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

        public readonly ReadOnlyReactiveProperty<string> OverrideDifficultyText;
        public readonly ReadOnlyReactiveProperty<ChartDifficulty?> ChartDifficulty;
        public readonly ReadOnlyReactiveProperty<float> Level;
        public readonly ReadOnlyReactiveProperty<string> ReadyBeatCountString;


        public ChartDataViewModel(ChartEditorModel model)
            : base(model)
        {
            MetaData = Model.ChartPackData.CurrentValue.ChartMetaDatas[Model.ChartMetaDataIndex];
            ChartData = Model.ChartData.CurrentValue;

            OverrideDifficultyText = MetaData.OverrideDifficultyText
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            ChartDifficulty = MetaData.Difficulty
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            Level = MetaData.Level
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<float>.Instance)
                .AddTo(base.Disposables);

            ReadyBeatCountString = ChartData.ReadyBeat
                .Select(beat => beat.ToString())
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, ChartData.ReadyBeat.Value.ToString())
                .AddTo(base.Disposables);
        }


        public void SetOverrideDifficultyText(string newText)
        {
            var oldText = OverrideDifficultyText.CurrentValue;

            if (newText == oldText)
                return;

            CommandStack.ExecuteCommand(
                () => MetaData.OverrideDifficultyText.Value = newText,
                () => MetaData.OverrideDifficultyText.Value = oldText
            );
        }

        public void SetChartDifficulty(ChartDifficulty? newDifficulty)
        {
            var oldDifficulty = ChartDifficulty.CurrentValue;

            if (newDifficulty == oldDifficulty)
                return;

            CommandStack.ExecuteCommand(
                () => MetaData.Difficulty.Value = newDifficulty,
                () => MetaData.Difficulty.Value = oldDifficulty
            );
        }

        public void SetLevel(float newLevel)
        {
            var oldLevel = Level.CurrentValue;

            if (!Mathf.Approximately(oldLevel, newLevel))
                return;

            if (0 < newLevel)
            {
                MetaData.Level.ForceNotify();
                return;
            }

            CommandStack.ExecuteCommand(
                () => MetaData.Level.Value = newLevel,
                () => MetaData.Level.Value = oldLevel
            );
        }

        public void SetReadyBeatCount(string newBeatCount)
        {
            if (!uint.TryParse(newBeatCount, out uint newBeatCountInt))
            {
                ChartData.ReadyBeat.ForceNotify();
                return;
            }

            var oldBeatIntCount = ChartData.ReadyBeat.Value;
            if (newBeatCountInt == oldBeatIntCount)
                return;

            CommandStack.ExecuteCommand(
                () => ChartData.ReadyBeat.Value = newBeatCountInt,
                () => ChartData.ReadyBeat.Value = oldBeatIntCount
            );
        }
    }
}
