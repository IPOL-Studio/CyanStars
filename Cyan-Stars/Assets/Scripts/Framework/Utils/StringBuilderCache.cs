using System.Collections.Generic;
using System.Text;

namespace CyanStars.Framework.Utils
{
    public class StringBuilderCache
    {
        private const int CacheCapacity = 8;

        public int InitCapacity { get; set; } = 64;

        public int MaximumRetainedCapacity { get; set; } = 1024;

        private Stack<StringBuilder> builders = new Stack<StringBuilder>(CacheCapacity);

        public StringBuilder Get()
        {
            return builders.Count > 0 ? builders.Pop() : new StringBuilder(InitCapacity);
        }

        public bool Release(StringBuilder item)
        {
            if (builders.Count >= CacheCapacity || item.Capacity > MaximumRetainedCapacity)
                return false;

            item.Clear();
            builders.Push(item);
            return true;
        }
    }
}
