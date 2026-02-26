#nullable enable

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


        /// <summary>
        /// 构造函数：将纯数据实例转为可观察实例，用于制谱器绑定
        /// </summary>
        public ChartMetaDataEditorModel(ChartMetaData chartMetaData)
        {
            FilePath = new ReactiveProperty<string>(chartMetaData.FilePath);
            Difficulty = new ReactiveProperty<ChartDifficulty?>(chartMetaData.Difficulty);
            Level = new ReactiveProperty<string>(chartMetaData.Level);
            ChartHash = new ReactiveProperty<string>(chartMetaData.ChartHash);
        }

        /// <summary>
        /// 将可观察实例转为纯数据实例，用于序列化
        /// </summary>
        public ChartMetaData ToChartMetaData()
        {
            return new ChartMetaData(FilePath.CurrentValue, Difficulty.CurrentValue, Level.CurrentValue, ChartHash.CurrentValue);
        }
    }
}
