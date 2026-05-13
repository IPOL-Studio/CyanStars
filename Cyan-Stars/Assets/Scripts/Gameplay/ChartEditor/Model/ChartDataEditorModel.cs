#nullable enable

using System.Diagnostics.Contracts;
using System.Linq;
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
        public readonly ObservableList<SpeedTemplateData> SpeedGroupDatas;
        public readonly ObservableList<BaseChartNoteData> Notes;
        public readonly ObservableList<ChartTrackData> TrackDatas;

        public ChartDataEditorModel(ChartData chartData)
        {
            ChartData = chartData;

            ReadyBeat = new ReactiveProperty<uint>(chartData.ReadyBeat);
            SpeedGroupDatas = new ObservableList<SpeedTemplateData>(chartData.SpeedGroupDatas);
            Notes = new ObservableList<BaseChartNoteData>(chartData.Notes);
            TrackDatas = new ObservableList<ChartTrackData>(chartData.TrackDatas);
        }

        /// <summary>
        /// 将制谱器的可观察数据转为常规数据，以用于序列化
        /// </summary>
        [Pure]
        public ChartData ToChartData()
        {
            var readyBeat = ReadyBeat.CurrentValue;
            var speedGroupDatas = SpeedGroupDatas.ToList();
            var notes = Notes.ToList();
            var trackDatas = TrackDatas.ToList();
            return new ChartData(readyBeat, speedGroupDatas, notes, trackDatas);
        }
    }
}
