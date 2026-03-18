namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// 时间轴片段基类
    /// </summary>
    public abstract class BaseClip<T> : IClip where T : BaseTrack
    {
        protected BaseClip(float startTime, float endTime, T owner)
        {
            StartTime = startTime;
            EndTime = endTime;
            Owner = owner;
            Valid = true;
        }

        /// <summary>
        /// 持有此片段的轨道
        /// </summary>
        BaseTrack IClip.Owner => Owner;

        /// <summary>
        /// 持有此片段的轨道
        /// </summary>
        public T Owner { get; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public float StartTime { get; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public float EndTime { get; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool Valid { get; private set; }

        /// <summary>
        /// 进入此片段时调用
        /// </summary>
        public virtual void OnEnter(IReadOnlyTimelineContext ctx)
        {
        }

        /// <summary>
        /// 离开此片段时调用
        /// </summary>
        public virtual void OnExit(IReadOnlyTimelineContext ctx)
        {
            Valid = false;
        }

        /// <summary>
        /// 更新片段
        /// </summary>
        public virtual void OnUpdate(IReadOnlyTimelineContext ctx)
        {
        }

        /// <summary>
        /// 跳入/跳出/整段跳过
        /// </summary>
        public virtual void OnSkip(IReadOnlyTimelineContext ctx)
        {
            if (EndTime <= ctx.CurrentTime)
                Valid = false;
        }
    }
}
