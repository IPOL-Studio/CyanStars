using System;
using System.Collections.Generic;
using CyanStars.Framework;

using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音符图层
    /// </summary>
    public class NoteLayer
    {
        /// <summary>
        /// 图层数据
        /// </summary>
        private NoteLayerData layerData;

        /// <summary>
        /// 当前时轴
        /// </summary>
        private int curTimeAxisIndex;


        /// <summary>
        /// 音符列表
        /// </summary>
        private List<BaseNote> notes = new List<BaseNote>();

        private List<BaseNote> cachedList = new List<BaseNote>();

        public NoteLayer(NoteLayerData layerData)
        {
            this.layerData = layerData;
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
        /// 更新当前时轴索引
        /// </summary>
        private void UpdateCurTimeAxisIndex(float currentTime)
        {
            if (curTimeAxisIndex == layerData.TimeAxisDatas.Count - 1)
            {
                //最后一个timeAxis 不计算了
                return;
            }

            NoteTimeAxisData curTimeAxis = layerData.TimeAxisDatas[curTimeAxisIndex];

            if ((currentTime * 1000) >= curTimeAxis.EndTime)
            {
                curTimeAxisIndex++;
            }
        }




        public void Update(float currentTime, float previousTime, float mapSpeed)
        {
            if (notes.Count == 0)
            {
                return;
            }

            //更新时轴
            UpdateCurTimeAxisIndex(currentTime);

            //计算当前视图层时间
            float curViewTime = CalCurViewTime(currentTime,mapSpeed);
            if (GameRoot.GetDataModule<MusicGameModule>().IsAutoMode) //如果是AutoMode
            {
                for (int i = notes.Count - 1; i >= 0; i--)
                {
                    notes[i].OnUpdateInAutoMode(currentTime,curViewTime); //使用AutoMode的OnUpdate
                }
            }
            else
            {
                for (int i = notes.Count - 1; i >= 0; i--)
                {
                    notes[i].OnUpdate(currentTime,curViewTime); //正常的OnUpdate
                }
            }
        }

        /// <summary>
        /// 计算当前视图层时间
        /// </summary>
        private float CalCurViewTime(float currentTime, float mapSpeed)
        {
            NoteTimeAxisData curTimeAxis = layerData.TimeAxisDatas[curTimeAxisIndex];
            int timeLength = curTimeAxis.EndTime - curTimeAxis.StartTime;
            int targetTime = (int)(currentTime * 1000) - curTimeAxis.StartTime;
            float finalCoefficient = mapSpeed * curTimeAxis.Coefficient;

            int easingValue = EasingFunction.CalTimeAxisEasingValue(curTimeAxis.EasingType, finalCoefficient,
                targetTime, timeLength);

            int curViewTime = curTimeAxis.ViewStartTime + easingValue;
            return curViewTime / 1000f;
        }

        public void OnInput(InputType type,float rangeMin,float rangeWidth)
        {
            if (notes.Count == 0)
            {
                return;
            }

            List<BaseNote> list = GetValidNotes(rangeMin,rangeWidth);

            if (list.Count == 0)
            {
                return;
            }

            if (list.Count > 1)
            {
                list.Sort((x, y) =>
                {
                    if (Math.Abs(x.CurLogicTime - y.CurLogicTime) > float.Epsilon)
                    {
                        //第一优先级是离玩家的距离
                        return x.CurLogicTime.CompareTo(y.CurLogicTime);
                    }

                    //第二优先级是离屏幕中间的距离
                    return Math.Abs(x.Pos - NoteData.MiddlePos).CompareTo(Math.Abs(y.Pos - NoteData.MiddlePos));
                });
            }


            //一次输入信号 只发给一个note处理 避免同时有多个note响应
            list[0].OnInput(type);
        }

        /// <summary>
        /// 获取可接收输入且在输入映射范围内的音符列表
        /// </summary>
        private List<BaseNote> GetValidNotes(float rangeMin,float rangeWidth)
        {
            cachedList.Clear();

            for (int i = 0; i < notes.Count; i++)
            {
                BaseNote note = notes[i];
                if (note.CanReceiveInput() && note.IsInInputRange(rangeMin, rangeMin + rangeWidth))
                {
                    cachedList.Add(note);
                }
            }

            return cachedList;
        }
    }
}
