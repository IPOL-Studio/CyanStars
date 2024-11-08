using System;
using System.Collections.Generic;

namespace CyanStars.Framework.Timer
{
    internal sealed class TimerListContainer<T>
    {
        public struct UpdateHandle : IDisposable
        {
            private TimerListContainer<T> container;

            public UpdateHandle(TimerListContainer<T> container)
            {
                this.container = container;
                this.container.isHandled = true;
            }

            public void Dispose()
            {
                container.OnFinishWait();
                container.isHandled = false;
            }
        }

        private struct ValueWrapper : IEquatable<ValueWrapper>
        {
            public T Value;
            public bool Invalid;

            public bool Equals(ValueWrapper other)
            {
                // 这里的 is 模式仅做检查，对 Value 做强转是故意的，不要改动
                return Invalid == other.Invalid &&
                       Value is IEquatable<T>
                    ? ((IEquatable<T>)Value).Equals(other.Value)
                    : Value.Equals(other.Value);
            }

            public override bool Equals(object obj) => obj is ValueWrapper other && Equals(other);
            public override int GetHashCode() => Value?.GetHashCode() ?? 0;
        }

        private List<ValueWrapper> list = new List<ValueWrapper>();
        private bool isHandled;
        private int removedCount;

        /// <summary>
        /// 目前保留的 wrapper 数量，不代表可用的 item 数量
        /// <para><see cref="TryGetValue"/> 以确认并获取特定可用的 item</para>
        /// </summary>
        public int Count => list.Count;

        public bool Contains(T item) => list.Contains(new ValueWrapper { Value = item });
        public void Add(T item) => list.Add(new ValueWrapper { Value = item });

        public void Remove(T item)
        {
            var index = list.IndexOf(new ValueWrapper { Value = item });
            if (index >= 0)
            {
                RemoveWrapperAt(index);
            }
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= list.Count)
                throw new IndexOutOfRangeException(nameof(index));

            RemoveWrapperAt(index);
        }

        private void RemoveWrapperAt(int index)
        {
            if (isHandled)
            {
                if (index >= 0)
                {
                    list[index] = new ValueWrapper { Invalid = true };
                    removedCount++;
                }
            }
            else
            {
                list.RemoveAt(index);
            }
        }

        public bool TryGetValue(int index, out T value)
        {
            if (index >= 0 && index < list.Count)
            {
                var wrapper = list[index];
                if (!wrapper.Invalid)
                {
                    value = wrapper.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// 在遍历更新前调用以将 remove 延迟到 dispose 时统一执行
        /// </summary>
        /// <para>Remove 依然会释放对 item 的引用，但是保留 item wrapper</para>
        /// <exception cref="InvalidOperationException">存在没有释放的 handle</exception>
        public UpdateHandle Handle()
        {
            if (isHandled)
                throw new InvalidOperationException("Timer is already handled, please dispose before handle");

            return new UpdateHandle(this);
        }

        private void OnFinishWait()
        {
            if (removedCount == 0)
            {
                return;
            }

            if (removedCount == Count)
            {
                list.Clear();
            }
            else
            {
                list.RemoveAll(RemovePredicate);
            }

            removedCount = 0;
        }

        private static readonly Predicate<ValueWrapper> RemovePredicate = item => item.Invalid;
    }
}
