#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ChartDataViewModel : BaseViewModel
    {
        private readonly ChartMetaDataEditorModel MetaData;
        private readonly ChartDataEditorModel ChartData;

        public readonly ReadOnlyReactiveProperty<ChartDifficulty?> ChartDifficulty;
        public readonly ReadOnlyReactiveProperty<string> ReadyBeatCountString;


        public ChartDataViewModel(ChartEditorModel model)
            : base(model)
        {
            MetaData = Model.ChartPackData.CurrentValue.ChartMetaDatas[Model.ChartMetaDataIndex];
            ChartData = Model.ChartData.CurrentValue;

            ChartDifficulty = MetaData.Difficulty
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);

            ReadyBeatCountString = ChartData.ReadyBeat
                .Select(beat => beat.ToString())
                .ToReadOnlyReactiveProperty(ForceUpdateEqualityComparer<string>.Instance, ChartData.ReadyBeat.Value.ToString())
                .AddTo(base.Disposables);
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
