#nullable enable

using System.Diagnostics.Contracts;
using CyanStars.Chart;
using R3;

namespace CyanStars.Gameplay.ChartEditor.Model
{
    /// <summary>
    /// 在制谱器内使用的谱包元数据类
    /// </summary>
    public class ChartMetaDataEditorModel
    {
        public readonly ReactiveProperty<string> FilePath;
        public readonly ReactiveProperty<ChartDifficulty?> Difficulty;
        public readonly ReactiveProperty<string> Level;
        public readonly ReactiveProperty<string> ChartHash;

        public ChartMetaDataEditorModel(ChartMetaData chartMetaData)
        {
            FilePath = new ReactiveProperty<string>(chartMetaData.FilePath);
            Difficulty = new ReactiveProperty<ChartDifficulty?>(chartMetaData.Difficulty);
            Level = new ReactiveProperty<string>(chartMetaData.Level);
            ChartHash = new ReactiveProperty<string>(chartMetaData.ChartHash);
        }

        /// <summary>
        /// 将制谱器的可观察数据转为常规数据，以用于序列化
        /// </summary>
        [Pure]
        public ChartMetaData ToChartMetaData()
        {
            var filePath = FilePath.CurrentValue;
            var difficulty = Difficulty.CurrentValue;
            var level = Level.CurrentValue;
            var chartHash = ChartHash.CurrentValue;
            return new ChartMetaData(filePath, difficulty, level, chartHash);
        }
    }
}
