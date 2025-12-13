using System;
using UnityEngine;

namespace CyanStars.Chart
{
    public readonly struct Beat : IEquatable<Beat>, IComparable<Beat>
    {
        /// <summary>带分数拍子的整数部分</summary>
        public readonly int IntegerPart;

        /// <summary>
        /// 带分数拍子的分子
        /// <para>当 <see cref="Denominator"/> 为 0 时，分子应始终视为 0</para>
        /// </summary>
        public readonly int Numerator;

        /// <summary>带分数拍子的分母，也作为节拍的细分精度</summary>
        public readonly int Denominator;

        /// <summary>构造并验证 Beat</summary>
        /// <param name="integerPart">拍子的整数部分</param>
        /// <param name="numerator">拍子的小数部分的分数</param>
        /// <param name="denominator">拍子的小数部分的分母（细分精度），如果为 0，numerator 也视为0（只取 integerPart 部分）</param>
        /// <param name="beat">返回的 Beat，验证失败返回 default</param>
        public static bool TryCreateBeat(int integerPart, int numerator, int denominator, out Beat beat)
        {
            beat = new Beat(integerPart, numerator, denominator);
            if (Verify(beat))
            {
                return true;
            }
            else
            {
                beat = default;
                return false;
            }
        }

        /// <summary>Beat 结构体的构造参数重载</summary>
        private Beat(int integerPart, int numerator, int denominator)
        {
            IntegerPart = integerPart;
            Numerator = numerator;
            Denominator = denominator;
        }

        /// <summary>校验 Beat 的三个参数是否都有效</summary>
        /// <returns>数据合法性</returns>
        private static bool Verify(Beat beat)
        {
            if (beat.IntegerPart < 0)
            {
                Debug.LogError("Beat 的整数部分必须大于等于 0");
                return false;
            }

            if (beat.Numerator < 0)
            {
                Debug.LogError("Beat 的分子必须大于等于 0");
                return false;
            }

            if (beat.Denominator <= 0)
            {
                Debug.LogError("Beat 的分母必须大于 0");
                return false;
            }

            if (beat.Numerator >= beat.Denominator)
            {
                Debug.LogError("Beat 的分子必须小于分母");
                return false;
            }

            return true;
        }


        /// <summary>将 Beat 转换为小数表示的拍子</summary>
        public float ToFloat()
        {
            if (Denominator == 0)
            {
                return IntegerPart;
            }

            return IntegerPart + (float)Numerator / Denominator;
        }


        public bool Equals(Beat other)
        {
            return IntegerPart == other.IntegerPart && Numerator == other.Numerator && Denominator == other.Denominator;
        }

        /// <summary>
        /// 对两个 beat 进行等价性比较，返回 0 时代表比较对象等价，不代表按字段相等
        /// <remarks>可使用 <see cref="Equals"/> 或 <see cref="op_Equality"/> <see cref="op_Inequality"/> 进行按字段相等性比较</remarks>
        /// </summary>
        public int CompareTo(Beat other)
        {
            if (IntegerPart != other.IntegerPart)
                return IntegerPart.CompareTo(other.IntegerPart);

            if (Denominator == other.Denominator)
                return Denominator != 0 ? Numerator.CompareTo(other.Numerator) : 0;

            ulong n1 = (ulong)Numerator * (ulong)other.Denominator;
            ulong n2 = (ulong)other.Numerator * (ulong)Denominator;

            return n1.CompareTo(n2);
        }

        public override bool Equals(object obj)
        {
            return obj is Beat other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = IntegerPart;
                hashCode = (hashCode * 397) ^ Numerator;
                hashCode = (hashCode * 397) ^ Denominator;
                return hashCode;
            }
        }

        /// <summary>
        /// 按字段相等性比较
        /// </summary>
        public static bool operator ==(Beat left, Beat right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 按字段相等性比较
        /// </summary>
        public static bool operator !=(Beat left, Beat right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Beat left, Beat right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(Beat left, Beat right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// 返回分数部分约分后的最简形式
        /// </summary>
        public Beat Simplify()
        {
            if (Denominator == 0 || Numerator == 0)
            {
                return new Beat(IntegerPart, 0, 0);
            }

            int a = Numerator;
            int b = Denominator;
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            int gcd = a;

            return new Beat(IntegerPart, Numerator / gcd, Denominator / gcd);
        }
    }
}
