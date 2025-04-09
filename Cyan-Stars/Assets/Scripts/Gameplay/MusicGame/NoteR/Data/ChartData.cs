using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CyanStars.Gameplay.MusicGame
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
        public List<BpmGroup> BpmGroups;

        /// <summary>变速组</summary>
        /// <remarks>必定存在一个相对 1 速的变速组，不可编辑或删除</remarks>
        public List<SpeedGroupData> SpeedGroups;

        /// <summary>谱面音符数据</summary>
        public List<BaseChartNoteData> Notes;

        /// <summary>谱面难度</summary>
        /// <remarks>为空时只在编辑器内可见，游戏内不加载；其他难度最多在一个谱包中各有0或1个</remarks>
        [CanBeNull]
        public ChartDifficulty? Difficulty;

        /// <summary>谱面定数</summary>
        /// <remarks>内置谱面此值应该是一个 [1, 20] 之间的整数，社区谱随意</remarks>
        public string Level;

        /// <summary>
        /// 事件条件，用于在游戏过程中动态控制轨道中事件是否触发
        /// </summary>
        public List<ChartTrackCondition> Conditions;

        /// <summary>
        /// 谱面轨道数据，需要在加载谱面时转换
        /// </summary>
        public List<ChartTrackData> TrackDatas;
    }
}
