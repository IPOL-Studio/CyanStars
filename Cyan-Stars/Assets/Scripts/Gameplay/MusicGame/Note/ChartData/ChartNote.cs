using System;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public enum ChartNoteType
    {
        Tap,
        Hold,
        Drag,
        Click,
        Break
    }

    public enum BreakNotePos
    {
        Left,
        Right
    }

    [Serializable]
    public class BaseChartNote
    {
        /// <summary>音符类型</summary>
        public ChartNoteType Type;

        /// <summary>在哪一拍判定</summary>
        /// <remarks>此值转换为时间后减去 offset 为相对于音乐开始的时间</remarks>
        public Vector3 JudgeBeat;

        /// <summary>引用的变速组</summary>
        public SpeedGroup SpeedGroup;
    }

    [Serializable]
    public class TapChartNote : BaseChartNote
    {
        /// <summary>音符左侧端点在水平轨道上的位置比例</summary>
        /// <remarks>范围 0~0.75（音符宽 0.25）</remarks>
        public float Pos;
    }

    [Serializable]
    public class HoldChartNote : BaseChartNote
    {
        /// <summary>音符左侧端点在水平轨道上的位置比例</summary>
        /// <remarks>范围 0~0.75（音符宽 0.25）</remarks>
        public float Pos;

        /// <summary>长按音符结束判定拍</summary>
        /// <remarks>必须大于 JudgeBeat</remarks>
        public Vector3 HoldEndBeat;
    }

    [Serializable]
    public class DragChartNote : BaseChartNote
    {
        /// <summary>音符左侧端点在水平轨道上的位置比例</summary>
        /// <remarks>范围 0~0.75（音符宽 0.25）</remarks>
        public float Pos;
    }

    [Serializable]
    public class ClickChartNote : BaseChartNote
    {
        /// <summary>音符左侧端点在水平轨道上的位置比例</summary>
        /// <remarks>范围 0~0.75（音符宽 0.25）</remarks>
        public float Pos;
    }

    [Serializable]
    public class BreakChartNote : BaseChartNote
    {
        /// <summary>Break 音符位于哪条轨道</summary>
        public BreakNotePos BreakNotePos;
    }
}
