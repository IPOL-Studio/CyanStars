using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CyanStars.Framework;
using CyanStars.Framework.Timeline;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音符片段
    /// </summary>
    public class NoteClip : BaseClip<NoteTrack>
    {
        // 音符链表，应该保证按照判定时间从小到大排序
        private LinkedList<BaseNote> notes = new LinkedList<BaseNote>();
        private LinkedListNode<BaseNote> lastInvalidNode;

        public NoteClip(float startTime, float endTime, NoteTrack owner) :
            base(startTime, endTime, owner)
        {
        }

        public void InsertNote(BaseNote note)
        {
            if (notes.Count == 0 || note.JudgeTime < notes.First.Value.JudgeTime)
            {
                notes.AddFirst(note);
                return;
            }

            if (note.JudgeTime >= notes.Last.Value.JudgeTime)
            {
                notes.AddLast(note);
                return;
            }

            LinkedListNode<BaseNote> current = notes.First;
            while (note.JudgeTime >= current.Value.JudgeTime)
                current = current.Next;

            notes.AddBefore(current, note);
        }

        public override void OnEnter(IReadOnlyTimelineContext _)
        {
            GameRoot.Event.AddListener(EventConst.MusicGameEndEvent, OnMusicGameEnd);
            GameRoot.Event.AddListener(InputEventArgs.EventName, OnInput);
        }

        public override void OnUpdate(IReadOnlyTimelineContext ctx) => OnUpdate(ctx, false);
        public override void OnSkip(IReadOnlyTimelineContext ctx) => OnUpdate(ctx, true);

        private void OnUpdate(IReadOnlyTimelineContext ctx, bool isSkip)
        {
            bool isAutoMode = GameRoot.GetDataModule<MusicGamePlayingDataModule>().IsAutoMode;
            float currentTime = (float)ctx.CurrentTime;

            var node = lastInvalidNode is null ? notes.First : lastInvalidNode.Next;

            while (node != null)
            {
                var note = node.Value;
                var next = node.Next;

                if (note.IsValid)
                {
                    if (isAutoMode)
                        note.OnUpdateInAutoMode(currentTime, isSkip);
                    else
                        note.OnUpdate(currentTime, isSkip);
                }

                InvalidNodeIfNeed(node);
                node = next;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InvalidNodeIfNeed(LinkedListNode<BaseNote> node)
        {
            if (!node.Value.IsValid)
                InvalidNode(node);
        }

        private void InvalidNode(LinkedListNode<BaseNote> node)
        {
            if (lastInvalidNode is null)
            {
                if (node != notes.First)
                {
                    notes.Remove(node);
                    notes.AddFirst(node);
                }
            }
            else
            {
                if (node != lastInvalidNode.Next)
                {
                    notes.Remove(node);
                    notes.AddAfter(lastInvalidNode, node);
                }
            }

            lastInvalidNode = node;
        }

        private void OnMusicGameEnd(object sender, EventArgs e)
        {
            GameRoot.Event.RemoveListener(EventConst.MusicGameEndEvent, OnMusicGameEnd);
            GameRoot.Event.RemoveListener(InputEventArgs.EventName, OnInput);
        }

        private void OnInput(object sender, EventArgs e)
        {
            InputEventArgs args = (InputEventArgs)e;

            if (notes.Count == 0)
                return;

            var node = lastInvalidNode is null ? notes.First : lastInvalidNode.Next;

            while (node != null)
            {
                var note = node.Value;
                var next = node.Next;

                if (note.CanReceiveInput() && note.IsInInputRange(args.RangeMin, args.RangeMin + args.RangeWidth))
                {
                    note.OnInput(args.Type);
                    InvalidNodeIfNeed(node);
                    break; // 一次输入信号只发给一个 note
                }

                InvalidNodeIfNeed(node);
                node = next;
            }
        }
    }
}
