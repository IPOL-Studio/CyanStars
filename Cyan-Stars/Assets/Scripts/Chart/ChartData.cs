using System;
using System.Collections.Generic;

namespace CyanStars.Chart
{
    /// <summary>
    /// 谱包中单个难度的谱面数据
    /// </summary>
    [Serializable]
    public class ChartData
    {
        /// <summary>
        /// 谱面难度，谱包中每个难度最多仅允许有一个，为 null 时为未定义，数量不受限制
        /// </summary>
        public ChartDifficulty? Difficulty;

        /// <summary>
        /// 谱面定数，内置谱包必须可转为为 int [1, 20]
        /// </summary>
        public string Level;

        /// <summary>在第一个 BPM 组开始前播放几次预备拍音效</summary>
        /// <remarks>必须大于等于0，一般为4，预备拍的时间间隔取决于第一个 BPM 组的 bpm</remarks>
        public int ReadyBeat;

        /// <summary>bpm 组</summary>
        /// <remarks>控制不同时候的拍子所占时长（拍子可转换为时间）</remarks>
        public BpmGroup BpmGroup;

        /// <summary>变速组</summary>
        /// <remarks>必定存在一个相对 1 速的变速组，不可编辑或删除</remarks>
        public List<SpeedGroupData> SpeedGroupDatas;

        /// <summary>谱面音符数据</summary>
        public List<BaseChartNoteData> Notes;

        /// <summary>
        /// 谱面轨道数据，需要在加载谱面时转换
        /// </summary>
        public List<ChartTrackData> TrackDatas;

        public ChartData(int readyBeat = 4, BpmGroup bpmGroup = null, List<SpeedGroupData> speedGroupDatas = null,
            List<BaseChartNoteData> notes = null, List<ChartTrackData> trackDatas = null)
        {
            ReadyBeat = readyBeat;
            BpmGroup = bpmGroup ?? new BpmGroup();
            SpeedGroupDatas = speedGroupDatas ??
                              new List<SpeedGroupData>()
                              {
                                  new SpeedGroupData(SpeedGroupType.Relative, new BezierCurve())
                              };
            Notes = notes ?? new List<BaseChartNoteData>();
            TrackDatas = trackDatas ?? new List<ChartTrackData>();
        }
    }
}
