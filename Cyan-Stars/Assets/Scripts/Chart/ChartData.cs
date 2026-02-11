#nullable enable

using System.Collections.Generic;
using CyanStars.Gameplay.ChartEditor.Model;
using Newtonsoft.Json;

namespace CyanStars.Chart
{
    /// <summary>
    /// 谱包中单个难度的谱面数据
    /// </summary>
    public class ChartData
    {
        /// <summary>在第一个 BPM 组开始前播放几次预备拍音效</summary>
        /// <remarks>必须大于等于0，一般为4，预备拍的时间间隔取决于第一个 BPM 组的 bpm</remarks>
        public int ReadyBeat;

        /// <summary>变速组</summary>
        /// <remarks>必定存在一个相对 1 速的变速组，不可编辑或删除</remarks>
        public List<SpeedTemplateData> SpeedGroupDatas;

        /// <summary>谱面音符数据</summary>
        public List<BaseChartNoteData> Notes;

        /// <summary>
        /// 谱面轨道数据，需要在加载谱面时转换
        /// </summary>
        public List<ChartTrackData> TrackDatas;


        /// <summary>
        /// 构造函数
        /// </summary>
        [JsonConstructor]
        public ChartData(int readyBeat = 4, List<SpeedTemplateData>? speedGroupDatas = null,
                         List<BaseChartNoteData>? notes = null, List<ChartTrackData>? trackDatas = null)
        {
            ReadyBeat = readyBeat;
            SpeedGroupDatas = speedGroupDatas ??
                              new List<SpeedTemplateData>() { new SpeedTemplateData(SpeedGroupType.Relative, new BezierCurve()) };
            Notes = notes ?? new List<BaseChartNoteData>();
            TrackDatas = trackDatas ?? new List<ChartTrackData>();
        }

        /// <summary>
        /// 将制谱器的可观察数据转为常规数据，以用于序列化
        /// </summary>
        public ChartData(ChartDataEditorModel editorData)
        {
            ReadyBeat = editorData.ReadyBeat.CurrentValue;
            SpeedGroupDatas = new List<SpeedTemplateData>();
            foreach (var speedTemplate in editorData.SpeedGroupDatas)
                SpeedGroupDatas.Add(speedTemplate);
            Notes = new List<BaseChartNoteData>();
            foreach (var note in editorData.Notes)
                Notes.Add(note);
            TrackDatas = new List<ChartTrackData>();
            foreach (var trackData in editorData.TrackDatas)
                TrackDatas.Add(trackData);
        }
    }
}
