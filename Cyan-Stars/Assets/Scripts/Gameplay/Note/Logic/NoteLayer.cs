using System;
using System.Collections.Generic;
using CyanStars.Framework;
using CyanStars.Gameplay.Data;
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
        /// 音符时轴
        /// </summary>
        private struct NoteTimeAxis
        {
            public NoteTimeAxis(float startTime, float speedRate)
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
        /// 时轴列表
        /// </summary>
        private List<NoteTimeAxis> timeAxises = new List<NoteTimeAxis>();

        /// <summary>
        /// 当前时轴索引
        /// </summary>
        private int curTimeAxisIndex;

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
            timeAxises.Add(new NoteTimeAxis(startTime, speedRate));
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
        /// 刷新当前时轴索引
        /// </summary>
        private void RefreshCurRangeIndex(float currentTime)
        {
            if (curTimeAxisIndex == timeAxises.Count - 1)
            {
                //最后一个timeAxis 不计算了
                return;
            }

            //是否到达了下一个timeAxis的开始？
            float nextTimeAxisStartTime = timeAxises[curTimeAxisIndex + 1].startTime;
            if (currentTime >= nextTimeAxisStartTime)
            {
                curTimeAxisIndex++;
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
            float noteViewSpeed = clipSpeed * timeAxises[curTimeAxisIndex].speedRate;

            float deltaTime = currentTime - previousTime;

            if (GameRoot.GetDataModule<MusicGameModule>().IsAutoMode)//如果是AutoMode
            {
                for (int i = notes.Count - 1; i >= 0; i--)
                {
                    notes[i].OnUpdateInAutoMode(deltaTime, noteViewSpeed);//使用AutoMode的OnUpdate
                }
            }
            else
            {
                for (int i = notes.Count - 1; i >= 0; i--)
                {
                    notes[i].OnUpdate(deltaTime, noteViewSpeed);//正常的OnUpdate
                }
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
