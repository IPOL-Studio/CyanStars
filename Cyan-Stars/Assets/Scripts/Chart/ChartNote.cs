#nullable enable

using JetBrains.Annotations;

namespace CyanStars.Chart
{
    public enum BreakNotePos
    {
        Left,
        Right
    }

    public interface IChartNoteNormalPos
    {
        /// <summary>音符左侧端点在水平轨道上的位置比例</summary>
        /// <remarks>范围 0~0.8（音符宽 0.2）</remarks>
        public float Pos { get; set; }
    }

    public abstract class BaseChartNoteData
    {
        /// <summary>音符类型</summary>
        public NoteType Type;

        /// <summary>
        /// 引用的变速组下标
        /// </summary>
        /// <remarks>从 0 开始，如果变速组在制谱器内发生变化，刷新所有 Note 的引用</remarks>
        public int SpeedTemplateIndex;

        /// <summary>
        /// 音符的变速 Offset，用于协调多个音符的整体变速效果
        /// TODO: 实装计算逻辑
        /// </summary>
        public int SpeedTemplateOffset = 0;

        /// <summary>正解提示音</summary>
        /// <remarks>
        /// 在音符达到判定时间时，无论是否有输入都播放音效，
        /// 玩家可选择是否让谱师设定的音效覆盖默认收藏品音效
        /// </remarks>
        public string? CorrectAudioName;

        /// <summary>打击音</summary>
        /// <remarks>
        /// 玩家有输入后才播放音效，
        /// 玩家可选择是否让谱师设定的音效覆盖默认收藏品音效
        /// </remarks>
        public string? HitAudioName;

        /// <summary>在哪一拍判定</summary>
        /// <remarks>此值转换为时间后减去 offset 为相对于音乐开始的时间</remarks>
        public Beat JudgeBeat;

        /// <summary>
        /// 可被判定
        /// </summary>
        /// <remarks>
        /// 为 false 时，音符不接收判定，仅用于表演 TODO: 在 GamePlay 实装此内容
        /// </remarks>
        public bool JudgeAble = true;

        /// <summary>
        /// 可被展示
        /// </summary>
        /// <remarks>
        /// 为 false 时，音符不展示，仅用于搭配其他效果时的表演 TODO: 在 GamePlay 实装此内容
        /// </remarks>
        public bool ViewAble = true;

        public BaseChartNoteData(NoteType type,
                                 Beat judgeBeat,
                                 int speedTemplateIndex = 0,
                                 int speedTemplateOffset = 0,
                                 string? correctAudioName = null,
                                 string? hitAudioName = null,
                                 bool judgeAble = true,
                                 bool viewAble = true)
        {
            Type = type;
            JudgeBeat = judgeBeat;
            SpeedTemplateIndex = speedTemplateIndex;
            SpeedTemplateOffset = speedTemplateOffset;
            CorrectAudioName = correctAudioName;
            HitAudioName = hitAudioName;
            JudgeAble = judgeAble;
            ViewAble = viewAble;
        }
    }

    public class TapChartNoteData : BaseChartNoteData, IChartNoteNormalPos
    {
        public float Pos { get; set; }

        public TapChartNoteData(float pos,
                                Beat judgeBeat,
                                int speedTemplateIndex = 0,
                                int speedTemplateOffset = 0,
                                string? correctAudioName = null,
                                string? hitAudioName = null,
                                bool judgeAble = true,
                                bool viewAble = true)
            : base(NoteType.Tap, judgeBeat, speedTemplateIndex, speedTemplateOffset, correctAudioName, hitAudioName, judgeAble, viewAble)
        {
            Pos = pos;
        }
    }

    public class HoldChartNoteData : BaseChartNoteData, IChartNoteNormalPos
    {
        public float Pos { get; set; }

        /// <summary>
        /// 音符尾引用的变速组
        /// </summary>
        public int HoldEndSpeedTemplateIndex;


        /// <summary>长按音符结束判定拍</summary>
        /// <remarks>必须大于 JudgeBeat</remarks>
        public Beat EndJudgeBeat;

        public HoldChartNoteData(float pos,
                                 Beat judgeBeat,
                                 Beat endJudgeBeat,
                                 int speedTemplateIndex = 0,
                                 int holdEndSpeedTemplateIndex = 0,
                                 int speedTemplateOffset = 0,
                                 string? correctAudioName = null,
                                 string? hitAudioName = null,
                                 bool judgeAble = true,
                                 bool viewAble = true)
            : base(NoteType.Hold, judgeBeat, speedTemplateIndex, speedTemplateOffset, correctAudioName, hitAudioName, judgeAble, viewAble)
        {
            HoldEndSpeedTemplateIndex = holdEndSpeedTemplateIndex;
            EndJudgeBeat = endJudgeBeat;
            Pos = pos;
        }
    }

    public class DragChartNoteData : BaseChartNoteData, IChartNoteNormalPos
    {
        public float Pos { get; set; }

        public DragChartNoteData(float pos,
                                 Beat judgeBeat,
                                 int speedTemplateIndex = 0,
                                 int speedTemplateOffset = 0,
                                 string? correctAudioName = null,
                                 string? hitAudioName = null,
                                 bool judgeAble = true,
                                 bool viewAble = true)
            : base(NoteType.Drag, judgeBeat, speedTemplateIndex, speedTemplateOffset, correctAudioName, hitAudioName, judgeAble, viewAble)
        {
            Pos = pos;
        }
    }

    public class ClickChartNoteData : BaseChartNoteData, IChartNoteNormalPos
    {
        public float Pos { get; set; }

        public ClickChartNoteData(float pos,
                                  Beat judgeBeat,
                                  int speedTemplateIndex = 0,
                                  int speedTemplateOffset = 0,
                                  string? correctAudioName = null,
                                  string? hitAudioName = null,
                                  bool judgeAble = true,
                                  bool viewAble = true)
            : base(NoteType.Click, judgeBeat, speedTemplateIndex, speedTemplateOffset, correctAudioName, hitAudioName, judgeAble, viewAble)
        {
            Pos = pos;
        }
    }

    public class BreakChartNoteData : BaseChartNoteData
    {
        /// <summary>Break 音符位于哪条轨道</summary>
        public BreakNotePos BreakNotePos;

        public BreakChartNoteData(BreakNotePos breakNotePos,
                                  Beat judgeBeat,
                                  int speedTemplateIndex = 0,
                                  int speedTemplateOffset = 0,
                                  string? correctAudioName = null,
                                  string? hitAudioName = null,
                                  bool judgeAble = true,
                                  bool viewAble = true)
            : base(NoteType.Break, judgeBeat, speedTemplateIndex, speedTemplateOffset, correctAudioName, hitAudioName, judgeAble, viewAble)
        {
            BreakNotePos = breakNotePos;
        }
    }
}
