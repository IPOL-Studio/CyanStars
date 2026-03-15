using System;
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
        public bool Enabled { get; set; } = true; // TODO: 在 timeline 中途启用或停用轨道可能会导致 bug

        /// <summary>
        /// 更新时遍历clip的开始索引，从上次更新时最前面的有效clip开始
        /// </summary>
        private int startIndex;

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
        /// 时间轴播放时每帧更新轨道
        /// </summary>
        public virtual void OnPlayingUpdate(TimelineContext ctx)
        {
            if (!Enabled)
            {
                return;
            }

            bool flag = false;

            for (int i = startIndex; i < Clips.Count; i++)
            {
                IClip clip = Clips[i];

                if (ctx.IsMusicGameMode && !clip.Valid)
                {
                    //已经exit过了
                    continue;
                }

                bool needEnter = ctx.CurrentTime >= clip.StartTime && ctx.PreviousTime < clip.StartTime;
                if (needEnter)
                {
                    //进入片段
                    clip.OnEnter(ctx);
                }

                bool needUpdate = ctx.CurrentTime >= clip.StartTime && ctx.CurrentTime <= clip.EndTime;
                if (needUpdate)
                {
                    //更新片段
                    clip.OnUpdate(ctx);
                }

                bool needExit = ctx.CurrentTime > clip.EndTime && ctx.PreviousTime <= clip.EndTime;
                if (needExit)
                {
                    //退出片段
                    clip.OnExit(ctx);
                }

                if (!needEnter && !needUpdate && !needExit)
                {
                    return;
                }
                else
                {
                    if (!flag)
                    {
                        flag = true;

                        //将下次update的startIndex设置为本次最前面的有效clip
                        startIndex = i;
                    }
                }
            }
        }

        public virtual void OnTimeJump(TimelineContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}
