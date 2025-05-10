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
        /// <summary>
        /// 音符链表
        /// </summary>
        public LinkedList<BaseNote> Notes = new LinkedList<BaseNote>();

        public NoteClip(float startTime, float endTime, NoteTrack owner) : base(
            startTime, endTime, owner)
        {
        }

        public void InsertNote(BaseNote note)
        {
            if (Notes.Count == 0)
            {
                Notes.AddFirst(note);
                return;
            }

            var current = Notes.First;
            while (current != null && current.Value.CurLogicTime <= note.CurLogicTime)
            {
                current = current.Next;
            }

            if (current == null)
            {
                Notes.AddLast(note);
            }
            else
            {
                Notes.AddBefore(current, note);
            }
        }

        public override void OnEnter()
        {
            GameRoot.Event.AddListener(EventConst.MusicGameEndEvent, OnMusicGameEnd);
            GameRoot.Event.AddListener(InputEventArgs.EventName, OnInput);
        }

        public override void OnUpdate(float currentTime, float previousTime)
        {
            if (GameRoot.GetDataModule<MusicGameModule>().IsAutoMode)
            {
                var node = Notes.Last;
                while (node != null)
                {
                    node.Value.OnUpdateInAutoMode(currentTime);
                    node = node.Previous;
                }
            }
            else
            {
                var node = Notes.Last;
                while (node != null)
                {
                    node.Value.OnUpdate(currentTime);
                    node = node.Previous;
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

            foreach (var note in Notes)
            {
                if (note.CanReceiveInput() && note.IsInInputRange(args.RangeMin, args.RangeMin + args.RangeWidth))
                {
                    note.OnInput(args.Type);
                    break; // 一次输入信号只发给一个 note
                }
            }
        }
    }
}
