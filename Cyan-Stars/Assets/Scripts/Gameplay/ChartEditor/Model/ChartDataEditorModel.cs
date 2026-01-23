#nullable enable

using CyanStars.Chart;
using ObservableCollections;
using R3;

namespace CyanStars.Gameplay.ChartEditor.Model
{
    /// <summary>
    /// 在制谱器内使用的谱面数据类
    /// </summary>
    public class ChartDataEditorModel
    {
        private readonly ChartData ChartData;

        public readonly ReactiveProperty<int> ReadyBeat;
        public readonly ObservableList<SpeedTemplateData> SpeedGroupDatas;
        public readonly ObservableList<BaseChartNoteData> Notes;
        public readonly ObservableList<ChartTrackData> TrackDatas;

        public ChartDataEditorModel(ChartData chartData)
        {
            ChartData = chartData;

            ReadyBeat = new ReactiveProperty<int>(chartData.ReadyBeat);
            SpeedGroupDatas = new ObservableList<SpeedTemplateData>();
            Notes = new ObservableList<BaseChartNoteData>();
            TrackDatas = new ObservableList<ChartTrackData>();
        }
    }
}
