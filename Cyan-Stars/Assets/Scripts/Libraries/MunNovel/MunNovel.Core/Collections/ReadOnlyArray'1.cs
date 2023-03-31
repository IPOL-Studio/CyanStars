using System;
using System.Collections;
using System.Collections.Generic;

namespace MunNovel.Collections
{
    public partial class ReadOnlyArray<T> : IReadOnlyList<T>
    {
        private readonly T[] _array;

        private ReadOnlyArray(T[] array)
        {
            _array = array;
        }

        public static ReadOnlyArray<T> From(T[] array)
        {
            return array != null
                ? new ReadOnlyArray<T>(array)
                : throw new ArgumentNullException("warp target can not be null", nameof(array));
        }

        public T this[int index] => _array[index];

        public int Count => _array.Length;

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(_array);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _array.GetEnumerator();
        }
    }

    public partial class ReadOnlyArray<T>
    {
        public struct Enumerator : IEnumerator<T>
        {
            private readonly T[] _array;
            private int _index;

            public Enumerator(T[] array)
            {
                _array = array;
                _index = -1;
            }

            public T Current => _array[_index];

            object IEnumerator.Current => _array[_index];

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                int next = _index + 1;
                if (next < _array.Length)
                {
                    _index = next;
                    return true;
                }
                _index = _array.Length;
                return false;
            }

            public void Reset()
            {
                _index = -1;
            }
        }
    }

    public static class ArrayExtension
    {
        public static ReadOnlyArray<T> AsReadOnlyArray<T>(this T[] self)
        {
            return ReadOnlyArray<T>.From(self);
        }
    }
}
