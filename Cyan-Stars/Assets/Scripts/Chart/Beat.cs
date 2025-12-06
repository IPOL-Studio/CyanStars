using System;
using UnityEngine;

namespace CyanStars.Chart
{
    public readonly struct Beat : IEquatable<Beat>
    {
        /// <summary>带分数拍子的整数部分</summary>
        public readonly int IntegerPart;

        /// <summary>带分数拍子的分子</summary>
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

        public static bool operator ==(Beat left, Beat right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Beat left, Beat right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Beat left, Beat right)
        {
            return left.ToFloat() < right.ToFloat();
        }

        public static bool operator >(Beat left, Beat right)
        {
            return left.ToFloat() > right.ToFloat();
        }
    }
}
