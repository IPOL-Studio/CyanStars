using System;
using System.Collections.Generic;
using CyanStars.Framework;
using CyanStars.Framework.Timeline;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音符片段
    /// </summary>
    public class NoteClip : BaseClip<NoteTrack>
    {
        // /// <summary>
        // /// 音符图层列表
        // /// </summary>
        // private List<NoteLayer> layers = new List<NoteLayer>();

        /// <summary>
        /// 音符列表
        /// </summary>
        public List<BaseNoteR> Notes = new List<BaseNoteR>();

        public NoteClip(float startTime, float endTime, NoteTrack owner) : base(
            startTime, endTime, owner)
        {
        }

        public void AddNote(BaseNoteR note)
        {
            Notes.Add(note);
        }

        // /// <summary>
        // /// 添加音符图层
        // /// </summary>
        // public void AddLayer(NoteLayer layer)
        // {
        //     layers.Add(layer);
        // }

        public override void OnEnter()
        {
            GameRoot.Event.AddListener(EventConst.MusicGameEndEvent, OnMusicGameEnd);
            GameRoot.Event.AddListener(InputEventArgs.EventName, OnInput);
        }

        public override void OnUpdate(float currentTime, float previousTime)
        {
            if (GameRoot.GetDataModule<MusicGameModule>().IsAutoMode)
            {
                for (int i = Notes.Count - 1; i >= 0; i--)
                {
                    Notes[i].OnUpdateInAutoMode(currentTime);
                }
            }
            else
            {
                for (int i = Notes.Count - 1; i >= 0; i--)
                {
                    Notes[i].OnUpdate(currentTime);
                }
            }
        }

        private void OnMusicGameEnd(object sender, EventArgs e)
        {
            GameRoot.Event.RemoveListener(EventConst.MusicGameEndEvent, OnMusicGameEnd);
            GameRoot.Event.RemoveListener(InputEventArgs.EventName, OnInput);
        }

        private void OnInput(object sender, EventArgs e)
        {
            InputEventArgs args = (InputEventArgs)e;

            if (Notes.Count == 0)
            {
                return;
            }

            List<BaseNoteR> list = GetValidNotes(args.RangeMin, args.RangeWidth);

            if (list.Count == 0)
            {
                return;
            }

            if (list.Count > 1)
            {
                list.Sort((x, y) =>
                {
                    if (x is INotePos && y is INotePos)
                    {
                        if (Math.Abs(x.CurLogicTime - y.CurLogicTime) > float.Epsilon)
                        {
                            //第一优先级是离玩家的距离
                            return x.CurLogicTime.CompareTo(y.CurLogicTime);
                        }

                        //第二优先级是离屏幕中间的距离
                        return Math.Abs(((INotePos)x).Pos - NoteData.MiddlePos)
                            .CompareTo(Math.Abs(((INotePos)y).Pos - NoteData.MiddlePos));
                    }
                    else
                    {
                        // Break 音符只比较时间距离
                        return x.CurLogicTime.CompareTo(y.CurLogicTime);
                    }
                });
            }

            //一次输入信号 只发给一个note处理 避免同时有多个note响应
            list[0].OnInput(args.Type);
        }

        /// <summary>
        /// 获取可接收输入且在输入映射范围内的音符列表
        /// </summary>
        private List<BaseNoteR> GetValidNotes(float rangeMin, float rangeWidth)
        {
            List<BaseNoteR> cachedList = new List<BaseNoteR>();

            foreach (var note in Notes)
            {
                if (note.CanReceiveInput() && note.IsInInputRange(rangeMin, rangeMin + rangeWidth))
                {
                    cachedList.Add(note);
                }
            }

            return cachedList;
        }
    }
}
