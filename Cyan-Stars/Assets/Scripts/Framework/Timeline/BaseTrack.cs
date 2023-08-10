using System.Collections.Generic;

namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// 时间轴轨道基类
    /// </summary>
    public abstract class BaseTrack
    {

        /// <summary>
        /// 持有此轨道的时间轴
        /// </summary>
        public Timeline Owner { get; set; }

        /// <summary>
        /// 片段列表
        /// </summary>
        protected List<IClip> Clips = new List<IClip>();

        /// <summary>
        /// 轨道是否被启用
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 添加片段
        /// </summary>
        public virtual void AddClip(IClip clip)
        {
            Clips.Add(clip);
        }

        /// <summary>
        /// 排序片段
        /// </summary>
        public virtual void SortClip()
        {
            Clips.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
        }

        /// <summary>
        /// 更新轨道
        /// </summary>
        public virtual void OnUpdate(float currentTime, float previousTime)
        {
            if (!Enabled)
            {
                return;
            }

            for (int i = 0; i < Clips.Count; i++)
            {
                IClip clip = Clips[i];

                if (!clip.Valid)
                {
                    //已经exit过了
                    continue;
                }

                bool needEnter = currentTime >= clip.StartTime && previousTime < clip.StartTime;
                if (needEnter)
                {
                    //进入片段
                    clip.OnEnter();
                }

                bool needUpdate = currentTime >= clip.StartTime && currentTime <= clip.EndTime;
                if (needUpdate)
                {
                    //更新片段
                    clip.OnUpdate(currentTime, previousTime);
                }

                bool needExit = currentTime > clip.EndTime && previousTime <= clip.EndTime;
                if (needExit)
                {
                    //退出片段
                    clip.OnExit();
                }

                if (!needEnter && !needUpdate && !needExit)
                {
                    return;
                }

            }
        }
    }
}
