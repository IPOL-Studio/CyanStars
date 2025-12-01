using System;

namespace CyanStars.Chart
{
    public readonly struct Beat
    {
        /// <summary>带分数拍子的整数部分</summary>
        public readonly int IntegerPart;

        /// <summary>带分数拍子的分子</summary>
        public readonly int Numerator;

        /// <summary>带分数拍子的分母，也作为节拍的细分精度</summary>
        public readonly int Denominator;

        /// <summary>Beat 的构造参数</summary>
        /// <param name="integerPart">拍子的整数部分</param>
        /// <param name="numerator">拍子的小数部分的分数</param>
        /// <param name="denominator">拍子的小数部分的分母（细分精度），如果为 0，numerator 也视为0（只取 integerPart 部分）</param>
        /// <remarks>上述三个值都必须大于等于 0</remarks>
        public Beat(int integerPart, int numerator, int denominator)
        {
            IntegerPart = integerPart;
            Numerator = numerator;
            Denominator = denominator;
            Verify();
        }

        /// <summary>校验 Beat 的三个参数是否都大于等于 0</summary>
        /// <returns>数据合法性</returns>
        public void Verify()
        {
            if (IntegerPart < 0)
            {
                throw new ArgumentException("Beat 的整数部分必须大于等于 0");
            }

            if (Numerator < 0)
            {
                throw new ArgumentException("Beat 的分子必须大于等于 0");
            }

            if (Denominator <= 0)
            {
                throw new ArgumentException("Beat 的分母必须大于 0");
            }

            if (Numerator >= Denominator)
            {
                throw new AggregateException("Beat 的分子必须小于分母");
            }
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
    }
}
