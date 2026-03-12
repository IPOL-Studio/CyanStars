#nullable enable

namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// timeline 播放时上下文
    /// </summary>
    /// <remarks>
    /// 提供给 track 和 clip 来区分谱面游玩或调试，以便采用不同逻辑。
    /// 作为方法参数传给 track和 clip 时，可考虑使用 in 关键词。
    /// </remarks>
    public struct TimelineContext
    {
        /// <summary>
        /// timeline 总长度 (s)
        /// </summary>
        public readonly float Length;

        /// <summary>
        /// 当前是否在音游模式内播放
        /// </summary>
        /// <remarks>
        /// true = 在音游模式下播放，timeline 的时间应该是连贯的
        /// false = 在制谱器模式下播放，允许暂停、时间跳变、倍速、倒带
        /// </remarks>
        public readonly bool IsMusicGameMode;


        /// <summary>
        /// 当前是否正在播放
        /// </summary>
        /// <remarks>
        /// false = 正在暂停
        /// </remarks>
        public bool IsPlaying;

        /// <summary>
        /// 当前的播放倍速
        /// </summary>
        /// <remarks>允许为 0 或负数，代表逻辑播放但不增加时间，或倒带播放</remarks>
        public float PlaybackSpeed;

        /// <summary>
        /// timeline 上次更新时的时间 (s)
        /// </summary>
        /// <remarks>
        /// track 和 clip 不应该使用此值来计算差值时间，应该直接通过 CurrentTime 计算目标时间点的值，以兼容制谱器模式下时间变化。
        /// </remarks>
        public float PreviousTime;

        /// <summary>
        /// timeline 当前时间 (s)
        /// </summary>
        /// <remarks>此值可能小于 PreviousTime，见于 timeline 倒放的情况</remarks>
        public float CurrentTime;


        public TimelineContext(
            bool isMusicGameMode,
            float length,
            bool isPlaying = false,
            float playbackSpeed = 1f,
            float previousTime = -float.Epsilon,
            float currentTime = -float.Epsilon
        )
        {
            IsMusicGameMode = isMusicGameMode;
            IsPlaying = isPlaying;
            PlaybackSpeed = playbackSpeed;
            Length = length;
            PreviousTime = previousTime;
            CurrentTime = currentTime;
        }
    }
}
