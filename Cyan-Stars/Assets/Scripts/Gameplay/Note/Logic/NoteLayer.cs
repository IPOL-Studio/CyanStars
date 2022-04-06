using System;
using System.Collections.Generic;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Note;

namespace CyanStars.Gameplay.Note
{
    /// <summary>
    /// 音符图层
    /// </summary>
    public class NoteLayer
    {
        /// <summary>
        /// 时间速率范围
        /// </summary>
        private struct TimeSpeedRateRange
        {
            public TimeSpeedRateRange(float startTime, float speedRate)
            {
                this.startTime = startTime;
                this.speedRate = speedRate;
            }

            /// <summary>
            /// 开始时间
            /// </summary>
            public float startTime;

            /// <summary>
            /// 速率
            /// </summary>
            public float speedRate;

        }

        /// <summary>
        /// 时间速率范围列表
        /// </summary>
        private List<TimeSpeedRateRange> ranges = new List<TimeSpeedRateRange>();

        /// <summary>
        /// 当前时间速率范围索引
        /// </summary>
        private int curRangeIndex;

        /// <summary>
        /// 音符列表
        /// </summary>
        private List<BaseNote> notes = new List<BaseNote>();

        private List<BaseNote> cachedList = new List<BaseNote>();

        /// <summary>
        /// 添加时间速率
        /// </summary>
        public void AddTimeSpeedRate(float startTime, float speedRate)
        {
            ranges.Add(new TimeSpeedRateRange(startTime, speedRate));
        }

        /// <summary>
        /// 添加音符
        /// </summary>
        public void AddNote(BaseNote note)
        {
            notes.Add(note);
        }

        /// <summary>
        /// 移除音符
        /// </summary>
        public void RemoveNote(BaseNote note)
        {
            notes.Remove(note);
        }

        /// <summary>
        /// 刷新当前时间速率范围索引
        /// </summary>
        private void RefreshCurRangeIndex(float currentTime)
        {
            if (curRangeIndex == ranges.Count - 1)
            {
                //最后一个range 不计算了
                return;
                ;
            }

            //是否到达了下一个range的开始？
            float nextRangeStartTime = ranges[curRangeIndex + 1].startTime;
            if (currentTime >= nextRangeStartTime)
            {
                curRangeIndex++;
            }

        }


        /// <summary>
        /// 获取可接收输入且在输入映射范围内的音符列表
        /// </summary>
        private List<BaseNote> GetValidNotes(InputMapData.Item item)
        {
            cachedList.Clear();

            for (int i = 0; i < notes.Count; i++)
            {
                BaseNote note = notes[i];
                if (note.CanReceiveInput() && note.IsInRange(item.RangeMin, item.RangeMin + item.RangeWidth))
                {
                    cachedList.Add(note);
                }
            }

            return cachedList;
        }

        public void Update(float currentTime, float previousTime, float clipSpeed)
        {
            if (notes.Count == 0)
            {
                return;
            }

            RefreshCurRangeIndex(currentTime);

            //计算音符表现层的速度
            float noteViewSpeed = clipSpeed * ranges[curRangeIndex].speedRate;

            float deltaTime = currentTime - previousTime;

            for (int i = notes.Count - 1; i >= 0; i--)
            {
                if(GameManager.Instance.isAutoMode)notes[i].OnUpdateInAutoMode(deltaTime, noteViewSpeed);
                else notes[i].OnUpdate(deltaTime, noteViewSpeed);
            }
        }

        public void OnInput(InputType inputType, InputMapData.Item item)
        {
            if (notes.Count == 0)
            {
                return;
            }

            List<BaseNote> list = GetValidNotes(item);

            if (list.Count == 0)
            {
                return;
                ;
            }


            list.Sort((x, y) =>
            {
                if (Math.Abs(x.LogicTimer - y.LogicTimer) > float.Epsilon)
                {
                    //第一优先级是离玩家的距离
                    return x.LogicTimer.CompareTo(y.LogicTimer);
                }

                //第二优先级是离屏幕中间的距离
                return Math.Abs(x.Pos - NoteData.MiddlePos).CompareTo(Math.Abs(y.Pos - NoteData.MiddlePos));
            });

            //一次输入信号 只发给一个note处理 避免同时有多个note响应
            list[0].OnInput(inputType);
        }
    }
}
