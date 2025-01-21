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
        /// <summary>谱面难度</summary>
        /// <remarks>为空时只在编辑器内可见，游戏内不加载；其他难度最多在一个谱包中各有0或1个</remarks>
        [CanBeNull]
        public ChartDifficulty? Difficulty;

        /// <summary>谱面定数</summary>
        /// <remarks>内置谱面此值应该是一个[1,20]之间的整数，社区谱随意，大于等于0f就行</remarks>
        [CanBeNull]
        public float? Level;

        /// <summary>谱面结束时间，单位 ms</summary>
        /// <remarks>通常来说是音乐时长 + offset</remarks>
        public float EndTime;

        /// <summary>谱面向前偏移量，单位 ms</summary>
        /// <remarks>即在谱面开始多久后才播放音乐，可为负数</remarks>
        public float Offset;

        /// <summary>bpm 组</summary>
        /// <remarks>控制不同时候的拍子所占时长（拍子可转换为时间）</remarks>
        [CanBeNull]
        public List<BpmGroup> BpmGroups;

        /// <summary>变速组</summary>
        /// <remarks>必定存在一个相对 1 速的变速组，不可编辑或删除</remarks>
        public List<SpeedGroup> SpeedGroups;

        /// <summary>谱面音符数据</summary>
        [CanBeNull]
        public List<BaseChartNote> Notes;

        // public List<ChartEvent>? Events; // TODO:谱面事件
        // public Lise<ChartCondition>? Conditions; // TODO:事件条件
    }
}
