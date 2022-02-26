using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public partial class MusicTimeline
{
    /// <summary>
    /// 图层
    /// </summary>
    public class Layer
    {
        /// <summary>
        /// 图层数据
        /// </summary>
        private LayerData data;
        
        /// <summary>
        /// 当前运行的片段的index
        /// </summary>
        private int curClipIndex;

        /// <summary>
        /// 音符列表
        /// </summary>
        public List<BaseNote> notes = new List<BaseNote>();
        
        private List<BaseNote> cachedList = new List<BaseNote>();

        public Layer(LayerData data)
        {
            this.data = data;
            CreateNotes();
        }

        /// <summary>
        /// 创建音符
        /// </summary>
        private void CreateNotes()
        {
            for (int i = 0; i < data.ClipDatas.Count; i++)
            {
                ClipData clipData = data.ClipDatas[i];
                for (int j = 0; j < clipData.NoteDatas.Count; j++)
                {
                    NoteData noteData = clipData.NoteDatas[j];
                    BaseNote note = CreateNote(noteData);
                    notes.Add(note);
                }
                
               
            }
        }

        /// <summary>
        /// 创建音符
        /// </summary>
        private BaseNote CreateNote(NoteData noteData)
        {
            BaseNote note = null;
            switch (noteData.Type)
            {
                case NoteType.Tap:
                    note = new TapNote();
                    break;
                case NoteType.Hold:
                    note = new HoldNote();
                    break;
                case NoteType.Drag:
                    note = new DragNote();
                    break;
                case NoteType.Click:
                    note = new ClickNote();
                    break;
                case NoteType.Break:
                    note = new BreakNote();
                    break;
            }
            note.SetData(noteData,this);

            return note;
        }

        /// <summary>
        /// 移除note
        /// </summary>
        public void RemoveNote(BaseNote note)
        {
            notes.Remove(note);
        }
        
        public void OnUpdate(float curTime,float deltaTime,float timelineSpeedRate)
        {
            if (notes.Count == 0)
            {
                return;
            }
            
            //根据当前timeline时间计算出当前正在运行的Clip，获得其对应速率
            RefreshCurClipIndex(curTime);
            
            //使用timeline速率和clip速率计算最终速率
            float clipSpeedRate = data.ClipDatas[curClipIndex].SpeedRate;
            float noteSpeedRate = timelineSpeedRate * clipSpeedRate;
                
            for (int i = notes.Count - 1; i >= 0; i--)
            {
                notes[i].OnUpdate(deltaTime,noteSpeedRate);
            }
        }

        /// <summary>
        /// 刷新当前片段索引
        /// </summary>
        private void RefreshCurClipIndex(float curTime)
        {
            if (curClipIndex == data.ClipDatas.Count - 1)
            {
                //最后一个clip了 不计算了
                return;;
            }

            //是否到达了下一个clip的时间范围？
            ClipData nextClip = data.ClipDatas[curClipIndex + 1];
            if (curTime >= nextClip.StartTime)
            {
                curClipIndex++;
            }
        }
        

        public void OnInput(InputType inputType, InputMapData.Item item)
        {
            if (notes.Count == 0)
            {
                return;
            }
            
            List<BaseNote> list = GetValidNotes();

            for (int i = 0; i < list.Count; i++)
            {
                BaseNote note = list[i];

                if (note.IsInRange(item.RangeMin,item.RangeMax))
                {
                    //可接收输入 并且 与输入映射到的范围有重合
                    //让音符响应输入
                    note.OnInput(inputType);
                }
            }
        }
    

        /// <summary>
        /// 获取可接收输入的音符列表
        /// </summary>
        private List<BaseNote> GetValidNotes()
        {
            cachedList.Clear();

            for (int i = 0; i < notes.Count; i++)
            {
                BaseNote note = notes[i];
                if (note.CanReceiveInput())
                {
                    cachedList.Add(note);
                }
            }

            return cachedList;
        }
    }
}
