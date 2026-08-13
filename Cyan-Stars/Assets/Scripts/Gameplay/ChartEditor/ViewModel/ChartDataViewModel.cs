#nullable enable

using CyanStars.Chart;
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
            // Difficulty 为 tracked 属性，等值赋值自动忽略，值变化则自动生成撤销命令
            MetaData.Difficulty.Value = newDifficulty;
        }

        public void SetReadyBeatCount(string newBeatCount)
        {
            if (!uint.TryParse(newBeatCount, out uint newBeatCountInt))
            {
                ChartData.ReadyBeat.ForceNotify();
                return;
            }

            ChartData.ReadyBeat.Value = newBeatCountInt;
        }
    }
}
