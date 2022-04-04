namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// 时间轴片段基类
    /// </summary>
    public abstract class BaseClip<T> : IClip where T : BaseTrack
    {
        protected BaseClip(float startTime, float endTime ,T owner)
        {
            StartTime = startTime;
            EndTime = endTime;
            Owner = owner;
        }
        

        /// <summary>
        /// 持有此片段的轨道
        /// </summary>
        BaseTrack IClip.Owner => Owner;

        /// <summary>
        /// 持有此片段的轨道
        /// </summary>
        public T Owner
        {
            get;
        }
        
        /// <summary>
        /// 开始时间
        /// </summary>
        public float StartTime
        {
            get;
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        public float EndTime
        {
            get;
        }


        /// <summary>
        /// 进入此片段时调用
        /// </summary>
        public virtual void OnEnter()
        {
            
        }

        /// <summary>
        /// 离开此片段时调用
        /// </summary>
        public virtual void OnExit()
        {
            
        }
        
        /// <summary>
        /// 更新片段
        /// </summary>
        public virtual void OnUpdate(float currentTime,float previousTime)
        {
            
        }
    }
}


