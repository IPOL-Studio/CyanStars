using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace CatTimeline
{
    /// <summary>
    /// 时间轴轨道基类
    /// </summary>
    public abstract class BaseTrack
    {
        
        /// <summary>
        /// 持有此轨道的时间轴
        /// </summary>
        public Timeline Owner
        {
            get;
            set;
        }

        /// <summary>
        /// 片段列表
        /// </summary>
        protected List<BaseClip> clips = new List<BaseClip>();

        /// <summary>
        /// 当前运行片段的索引
        /// </summary>
        protected int curClipIndex;
        
        /// <summary>
        /// 轨道是否被启用
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        } = true;
        
        
        /// <summary>
        /// 添加片段
        /// </summary>
        public virtual void AddClip(BaseClip clip)
        {
            clips.Add(clip);
        }

        /// <summary>
        /// 排序片段
        /// </summary>
        public virtual void SortClip()
        {
            clips.Sort((x, y) =>
            {
                return x.StartTime.CompareTo(y.StartTime);
            });
        }

        /// <summary>
        /// 更新轨道
        /// </summary>
        public virtual void Update(float currentTime,float previousTime)
        {
            if (!Enabled)
            {
                return;
            }

            if (curClipIndex == clips.Count)
            {
                return;
            }

            //保证每次只处理第一个clip
            BaseClip clip = clips[curClipIndex];

            if (currentTime >= clip.StartTime && previousTime < clip.StartTime )
            {
                //进入片段
                clip.OnEnter();
            }

            if (currentTime >= clip.StartTime && currentTime <= clip.EndTime)
            {
                //更新片段
                clip.Update(currentTime,previousTime);
            }

            if (currentTime > clip.EndTime && previousTime <= clip.EndTime)
            {
                //离开片段
                clip.OnExit();
                curClipIndex++;
            }
        }
    }

}
