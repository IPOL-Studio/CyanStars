using System;
using CyanStars.Gameplay.MusicGame;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CyanStars.Gameplay.Chart
{
    public enum BreakNotePos
    {
        Left,
        Right
    }


    [Serializable]
    public class BaseChartNoteData
    {
        /// <summary>音符类型</summary>
        public NoteType Type;

        /// <summary>
        /// 引用的变速组下标
        /// </summary>
        /// <remarks>从 0 开始，如果变速组在编辑器内发生变化，刷新所有 Note 的引用</remarks>
        public int SpeedGroupIndex;

        /// <summary>正解提示音</summary>
        /// <remarks>
        /// 在音符达到判定时间时，无论是否有输入都播放音效，
        /// 玩家可选择是否让谱师设定的音效覆盖默认收藏品音效
        /// </remarks>
        [CanBeNull]
        public string CorrectAudioName;

        /// <summary>打击音</summary>
        /// <remarks>
        /// 玩家有输入后才播放音效，
        /// 玩家可选择是否让谱师设定的音效覆盖默认收藏品音效
        /// </remarks>
        [CanBeNull]
        public string HitAudioName;

        /// <summary>在哪一拍判定</summary>
        /// <remarks>此值转换为时间后减去 offset 为相对于音乐开始的时间</remarks>
        public Beat JudgeBeat;

        /// <summary>
        /// 可被判定
        /// </summary>
        /// <remarks>
        /// 为 false 时，音符不接收判定，仅用于表演
        /// </remarks>
        public bool JudgeAble = true;

        /// <summary>
        /// 可被展示
        /// </summary>
        /// <remarks>
        /// 为 false 时，音符不展示，仅用于搭配其他效果时的表演
        /// </remarks>
        public bool ViewAble = true;
    }

    [Serializable]
    public class TapChartNoteData : BaseChartNoteData
    {
        /// <summary>音符左侧端点在水平轨道上的位置比例</summary>
        /// <remarks>范围 0~0.8（音符宽 0.2）</remarks>
        public float Pos;
    }

    [Serializable]
    public class HoldChartNoteData : BaseChartNoteData
    {
        /// <summary>
        /// 音符尾引用的变速组
        /// </summary>
        public int HoldEndSpeedGroupIndex;

        /// <summary>音符左侧端点在水平轨道上的位置比例</summary>
        /// <remarks>范围 0~0.8（音符宽 0.2）</remarks>
        public float Pos;

        /// <summary>长按音符结束判定拍</summary>
        /// <remarks>必须大于 JudgeBeat</remarks>
        public Beat EndJudgeBeat;
    }

    [Serializable]
    public class DragChartNoteData : BaseChartNoteData
    {
        /// <summary>音符左侧端点在水平轨道上的位置比例</summary>
        /// <remarks>范围 0~0.8（音符宽 0.2）</remarks>
        public float Pos;
    }

    [Serializable]
    public class ClickChartNoteData : BaseChartNoteData
    {
        /// <summary>音符左侧端点在水平轨道上的位置比例</summary>
        /// <remarks>范围 0~0.8（音符宽 0.2）</remarks>
        public float Pos;
    }

    [Serializable]
    public class BreakChartNoteData : BaseChartNoteData
    {
        /// <summary>Break 音符位于哪条轨道</summary>
        public BreakNotePos BreakNotePos;
    }
}
