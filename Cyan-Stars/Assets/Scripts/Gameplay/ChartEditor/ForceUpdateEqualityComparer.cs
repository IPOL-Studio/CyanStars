#nullable enable

using System.Collections.Generic;

namespace CyanStars.Gameplay.ChartEditor
{
    /// <summary>
    /// 永远返回 false 的相等比较器，用于 R3 VM->V 强制更新绑定值
    /// </summary>
    /// <remarks>对于可能触发 .ForceNotify() 的 reactiveProperty，需要在所有衍生的 readOnlyReactiveProperty 的 .ToReadOnlyReactiveProperty() 中传入此参数，以同步刷新</remarks>
    public class ForceUpdateEqualityComparer<T> : IEqualityComparer<T>
    {
        public static readonly ForceUpdateEqualityComparer<T> Instance = new();

        public bool Equals(T? x, T? y)
        {
            return false;
        }

        public int GetHashCode(T obj)
        {
            return obj?.GetHashCode() ?? 0;
        }
    }
}
