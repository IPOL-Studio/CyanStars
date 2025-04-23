using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CyanStars.Gameplay.Chart
{
    /// <summary>
    /// 谱包中单个难度的谱面数据
    /// </summary>
    [Serializable]
    public class ChartData
    {
        /// <summary>在第一个 BPM 组开始前播放几次预备拍音效</summary>
        /// <remarks>必须大于等于0，一般为4，预备拍的时间间隔取决于第一个 BPM 组的 bpm</remarks>
        public int ReadyBeat;

        /// <summary>bpm 组</summary>
        /// <remarks>控制不同时候的拍子所占时长（拍子可转换为时间）</remarks>
        public BpmGroups BpmGroups;

        /// <summary>变速组</summary>
        /// <remarks>必定存在一个相对 1 速的变速组，不可编辑或删除</remarks>
        public List<SpeedGroupData> SpeedGroups;

        /// <summary>谱面音符数据</summary>
        public List<BaseChartNoteData> Notes;

        /// <summary>
        /// 谱面轨道数据，需要在加载谱面时转换
        /// </summary>
        public List<ChartTrackData> TrackDatas;
    }
}
