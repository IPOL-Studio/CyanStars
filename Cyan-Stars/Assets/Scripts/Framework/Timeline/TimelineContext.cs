#nullable enable

namespace CyanStars.Framework.Timeline
{
    public interface IReadOnlyTimelineContext
    {
        public float Length { get; }
        public bool IsMusicGameMode { get; }


        public float PlaybackSpeed { get; }
        public double PreviousTime { get; }
        public double CurrentTime { get; }
    }

    /// <summary>
    /// timeline 播放时上下文
    /// </summary>
    /// <remarks>
    /// 提供给 track 和 clip 来区分谱面游玩或调试，以便采用不同逻辑。
    /// </remarks>
    public class TimelineContext : IReadOnlyTimelineContext
    {
        /// <summary>
        /// timeline 总长度 (s)
        /// </summary>
        public float Length { get; }

        /// <summary>
        /// 当前是否在音游模式内播放
        /// </summary>
        /// <remarks>
        /// true = 在音游模式下播放，timeline 的时间应该是连贯的
        /// false = 在制谱器模式下播放，允许暂停、时间跳变、倍速、倒带
        /// </remarks>
        public bool IsMusicGameMode { get; }


        /// <summary>
        /// 当前的播放倍速
        /// </summary>
        /// <remarks>允许为 0 或负数，代表逻辑播放但不增加时间，或倒带播放</remarks>
        public float PlaybackSpeed { get; set; }

        /// <summary>
        /// timeline 上次更新时的时间 (s)
        /// </summary>
        /// <remarks>
        /// track 和 clip 不应该使用此值来计算差值时间，应该直接通过 CurrentTime 计算目标时间点的值，以兼容制谱器模式下时间变化。
        /// </remarks>
        public double PreviousTime { get; set; }

        /// <summary>
        /// timeline 当前时间 (s)
        /// </summary>
        /// <remarks>此值可能小于 PreviousTime，见于 timeline 倒放的情况</remarks>
        public double CurrentTime { get; set; }


        public TimelineContext(
            bool isMusicGameMode,
            float length,
            float playbackSpeed = 1f,
            double previousTime = -double.Epsilon,
            double currentTime = -double.Epsilon
        )
        {
            IsMusicGameMode = isMusicGameMode;
            PlaybackSpeed = playbackSpeed;
            Length = length;
            PreviousTime = previousTime;
            CurrentTime = currentTime;
        }
    }
}
