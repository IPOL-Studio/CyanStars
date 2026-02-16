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

        public readonly ReactiveProperty<uint> ReadyBeat;
        public readonly ObservableList<SpeedTemplateData> SpeedTemplateDatas;
        public readonly ObservableList<BaseChartNoteData> Notes;
        public readonly ObservableList<ChartTrackData> TrackDatas;

        public ChartDataEditorModel(ChartData chartData)
        {
            ChartData = chartData;

            ReadyBeat = new ReactiveProperty<uint>(chartData.ReadyBeat);
            SpeedTemplateDatas = new ObservableList<SpeedTemplateData>(chartData.SpeedTemplateDatas);
            Notes = new ObservableList<BaseChartNoteData>(chartData.Notes);
            TrackDatas = new ObservableList<ChartTrackData>(chartData.TrackDatas);
        }
    }
}
