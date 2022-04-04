namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// 时间轴片段接口
    /// </summary>
    public interface IClip
    {
        /// <summary>
        /// 持有此轨道的时间轴
        /// </summary>
        public BaseTrack Owner { get; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public float StartTime { get; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public float EndTime { get; }

        /// <summary>
        /// 进入此片段
        /// </summary>
        public void OnEnter();

        /// <summary>
        /// 更新此片段
        /// </summary>
        public void OnUpdate(float currentTime, float previousTime);

        /// <summary>
        /// 离开此片段
        /// </summary>
        public void OnExit();
    }
}
