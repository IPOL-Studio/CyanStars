using System.Collections.Generic;

namespace CyanStars.Framework.Timeline
{
    /// <summary>
    /// 时间轴轨道基类
    /// </summary>
    public abstract class BaseTrack
    {
        /// <summary>
        /// 片段处理模式
        /// </summary>
        public enum ClipProcessMode
        {
            /// <summary>
            /// 每次只处理一个片段（适合片段不会重叠的情况）
            /// </summary>
            Single,
            
            /// <summary>
            /// 每次都处理所有片段（适合片段会重叠的情况）
            /// </summary>
            All,
        }
        
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
        protected List<IClip> clips = new List<IClip>();

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
        /// 片段更新模式
        /// </summary>
        protected virtual ClipProcessMode Mode => ClipProcessMode.Single;

        /// <summary>
        /// 添加片段
        /// </summary>
        public virtual void AddClip(IClip clip)
        {
            clips.Add(clip);
        }

        /// <summary>
        /// 排序片段
        /// </summary>
        public virtual void SortClip()
        {
            clips.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
        }

        /// <summary>
        /// 更新轨道
        /// </summary>
        public virtual void OnUpdate(float currentTime,float previousTime)
        {
            if (!Enabled)
            {
                return;
            }

            switch (Mode)
            {
                case ClipProcessMode.Single:
                    
                    if (curClipIndex == clips.Count)
                    {
                        return;
                    }
        
                    //只处理当前的片段
                    IClip clip = clips[curClipIndex];

                    if (currentTime >= clip.StartTime && previousTime < clip.StartTime )
                    {
                        //进入片段
                        clip.OnEnter();
                    }

                    if (currentTime >= clip.StartTime && currentTime <= clip.EndTime)
                    {
                        //更新片段
                        clip.OnUpdate(currentTime,previousTime);
                    }
            
                    if (currentTime > clip.EndTime && previousTime <= clip.EndTime)
                    {
                        //离开片段
                        clip.OnExit();
                        curClipIndex++;
                
                        //clipIndex更新了 需要重新Update一遍 保证不漏掉新clip的enter和exit
                        OnUpdate(currentTime,previousTime);
                    }
                    
                    break;
                
                
                case ClipProcessMode.All:
                    
                    for (int i = 0; i < clips.Count; i++)
                    {
                        clip = clips[i];
            
                        if (currentTime >= clip.StartTime && previousTime < clip.StartTime )
                        {
                            //进入片段
                            clip.OnEnter();
                        }

                        if (currentTime >= clip.StartTime && currentTime <= clip.EndTime)
                        {
                            //更新片段
                            clip.OnUpdate(currentTime,previousTime);
                        }
            
                        if (currentTime > clip.EndTime && previousTime <= clip.EndTime)
                        {
                            //离开片段
                            clip.OnExit();
                        }
                    }
                    
                    break;
            }
            
          
        }
    }

}
