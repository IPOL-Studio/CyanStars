#nullable enable

using System.Collections.Generic;
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
        public readonly ReactiveProperty<uint> ReadyBeat;
        public readonly ObservableList<SpeedTemplateDataEditorModel> SpeedTemplateDatas;
        public readonly ObservableList<BaseChartNoteData> Notes;
        public readonly ObservableList<ChartTrackData> TrackDatas;


        /// <summary>
        /// 构造函数：将纯数据实例转为可观察实例，用于制谱器绑定
        /// </summary>
        public ChartDataEditorModel(ChartData chartData)
        {
            ReadyBeat = new ReactiveProperty<uint>(chartData.ReadyBeat);
            SpeedTemplateDatas = new ObservableList<SpeedTemplateDataEditorModel>(chartData.SpeedTemplateDatas.Select(d => new SpeedTemplateDataEditorModel(d)));
            Notes = new ObservableList<BaseChartNoteData>(chartData.Notes);
            TrackDatas = new ObservableList<ChartTrackData>(chartData.TrackDatas);
        }

        /// <summary>
        /// 将可观察实例转为纯数据实例，用于序列化
        /// </summary>
        public ChartData ToChartData()
        {
            return new ChartData(
                ReadyBeat.CurrentValue,
                new List<SpeedTemplateData>(SpeedTemplateDatas.Select(d => d.ToSpeedTemplateData())),
                new List<BaseChartNoteData>(Notes),
                new List<ChartTrackData>(TrackDatas)
            );
        }
    }
}
