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
        // 音符链表，应该保证按照判定时间从小到大排序
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
            _ = note ?? throw new ArgumentNullException(nameof(note));

            if (notes.Count == 0 || note.JudgeTime >= notes.Last.Value.JudgeTime)
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

            foreach (var node in new NoteNodeEnumeratorProxy(notes))
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
            if (notes is null)
                return;

            // 使用了自定义的枚举器，在 foreach 时应该也不影响删除当前正在访问的节点
            if (currentNoteNode != null && currentNoteNode.Value == note)
            {
                notes.Remove(currentNoteNode);
                currentNoteNode = null;
            }
            else
            {
                notes.Remove(note);
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

            if (notes.Count == 0)
                return;

            foreach (var node in new NoteNodeEnumeratorProxy(notes))
            {
                var note = node.Value;
                currentNoteNode = node;
                if (note.CanReceiveInput() && note.IsInInputRange(args.RangeMin, args.RangeMin + args.RangeWidth))
                {
                    note.OnInput(args.Type);
                    break; // 一次输入信号只发给一个 note
                }
            }

            currentNoteNode = null;
        }

        private readonly struct NoteNodeEnumeratorProxy : IEnumerable<LinkedListNode<BaseNote>>
        {
            private readonly LinkedList<BaseNote> Notes;
            public NoteNodeEnumeratorProxy(LinkedList<BaseNote> notes) => this.Notes = notes;
            public readonly IEnumerator<LinkedListNode<BaseNote>> GetEnumerator() => new NoteNodeEnumerator(Notes);
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private struct NoteNodeEnumerator : IEnumerator<LinkedListNode<BaseNote>>
        {
            private LinkedList<BaseNote> notes;
            private LinkedListNode<BaseNote> currentNode;
            private LinkedListNode<BaseNote> nextNode;

            public NoteNodeEnumerator(LinkedList<BaseNote> notes)
            {
                this.notes = notes;
                this.currentNode = null;
                this.nextNode = null;
            }

            public LinkedListNode<BaseNote> Current => currentNode;
            object System.Collections.IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                // 初始状态
                if (currentNode is null)
                {
                    currentNode = notes.First;
                    nextNode = currentNode?.Next;
                }
                // 当前节点被删除了
                else if (currentNode.List != notes)
                {
                    if (nextNode != null && nextNode.List != notes)
                    {
                        throw new InvalidOperationException("NoteClip 的 notes 枚举器状态被完全破坏，不可恢复");
                    }

                    currentNode = nextNode;
                    nextNode = currentNode?.Next;
                }
                // next 节点被删除了，这不是一个设计上应该出现的情况，但遍历还可以进行
                else if (currentNode.Next != nextNode)
                {
                    UnityEngine.Debug.LogWarning("当前正在通过 foreach 遍历 note clip 的 note 链表，但上一次遍历到的节点的 next 被删除了，这个情况不属于预期用例，需要检查");

                    currentNode = nextNode;
                    nextNode = currentNode?.Next;
                }
                // 正常情况，继续往下走
                else
                {
                    currentNode = nextNode;
                    nextNode = currentNode?.Next;
                }

                return currentNode != null;
            }

            public void Reset()
            {
                currentNode = null;
                nextNode = null;
            }
        }
    }
}
