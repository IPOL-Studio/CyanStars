namespace CyanStars.Gameplay.MusicGame
{
    public static class EventConst
    {
        /// <summary>
        /// 音游开始事件
        /// </summary>
        public const string MusicGameStartEvent = nameof(MusicGameStartEvent);

        /// <summary>
        /// 音游结束事件
        /// </summary>
        public const string MusicGameEndEvent = nameof(MusicGameEndEvent);

        /// <summary>
        /// 音游数据刷新事件
        /// </summary>
        public const string MusicGameDataRefreshEvent = nameof(MusicGameDataRefreshEvent);

        /// <summary>
        /// 音游暂停事件
        /// </summary>
        public const string MusicGamePauseEvent = nameof(MusicGamePauseEvent);

        /// <summary>
        /// 音游恢复事件
        /// </summary>
        public const string MusicGameResumeEvent = nameof(MusicGameResumeEvent);

        /// <summary>
        /// 音游退出事件
        /// </summary>
        public const string MusicGameExitEvent = nameof(MusicGameExitEvent);
    }
}
