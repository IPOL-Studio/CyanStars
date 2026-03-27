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
        // 音符链表，应该保证按照判定时间从大到小排序
        // 即判定时间越早的应该在链表后面
        private LinkedList<BaseNote> notes = new LinkedList<BaseNote>();

        // 在同一时刻应该只有一个 foreach 在遍历 notes 链表
        // 基于这个设计假设，缓存当前正在遍历的音符节点，方便进行 O(1) 的 Remove 操作
        private LinkedListNode<BaseNote> currentNoteNode;

        public NoteClip(float startTime, float endTime, NoteTrack owner) :
            base(startTime, endTime, owner)
        {
        }

        public void InsertNote(BaseNote note)
        {
            // 为了保证在 update 时存在的 remove 操作的性能
            // 插入时应从 Last -> First 方向
            // 同时确保插入时的相对顺序
            if (notes.Count == 0 || note.JudgeTime >= notes.First.Value.JudgeTime)
            {
                notes.AddFirst(note);
                return;
            }

            LinkedListNode<BaseNote> current = notes.Last;
            while (note.JudgeTime >= current.Value.JudgeTime)
                current = current.Previous;

            notes.AddAfter(current, note);
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

            foreach (var node in new ReverseNoteNodeEnumeratorProxy(notes))
            {
                var note = node.Value;
                currentNoteNode = node;
                if (isAutoMode)
                    note.OnUpdateInAutoMode(currentTime, isSkip);
                else
                    note.OnUpdate(currentTime, isSkip);
            }

            currentNoteNode = null;
        }

        public void RemoveNote(BaseNote note)
        {
            // 设计预设遍历方向总是从 last -> first
            // 所以应该只需要直接转发 Remove 操作

            if (currentNoteNode != null && currentNoteNode.Value == note)
            {
                notes.Remove(currentNoteNode);
                currentNoteNode = null;
            }
            else
                notes.Remove(note);
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

            foreach (var node in new ReverseNoteNodeEnumeratorProxy(notes))
            {
                var note = node.Value;
                currentNoteNode = node;
                if (note.CanReceiveInput() && note.IsInInputRange(args.RangeMin, args.RangeMin + args.RangeWidth))
                {
                    note.OnInput(args.Type);
                    break; // 一次输入信号只发给一个 note
                }
            }
        }

        private readonly struct ReverseNoteNodeEnumeratorProxy : IEnumerable<LinkedListNode<BaseNote>>
        {
            private readonly LinkedList<BaseNote> Notes;
            public ReverseNoteNodeEnumeratorProxy(LinkedList<BaseNote> notes) => this.Notes = notes;
            public readonly IEnumerator<LinkedListNode<BaseNote>> GetEnumerator() => new ReverseNoteNodeEnumerator(Notes);
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private struct ReverseNoteNodeEnumerator : IEnumerator<LinkedListNode<BaseNote>>
        {
            private LinkedList<BaseNote> notes;
            private LinkedListNode<BaseNote> currentNode;

            public ReverseNoteNodeEnumerator(LinkedList<BaseNote> notes)
            {
                this.notes = notes;
                this.currentNode = null;
            }

            public LinkedListNode<BaseNote> Current => currentNode;
            object System.Collections.IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                currentNode = currentNode is null ? notes.Last : currentNode.Previous;
                return !(currentNode is null);
            }

            public void Reset() => currentNode = null;
        }
    }
}
