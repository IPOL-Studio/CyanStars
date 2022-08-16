using System;
using CyanStars.Framework.Pool;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 输入事件
    /// </summary>
    public class InputEventArgs : EventArgs,IReference
    {
        public const string EventName = nameof(InputEventArgs);

        /// <summary>
        /// 输入类型
        /// </summary>
        public InputType Type { get; private set; }

        /// <summary>
        /// 输入编号
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// 输入范围最小值
        /// </summary>
        public float RangeMin { get; private set; }

        /// <summary>
        /// 输入范围宽度
        /// </summary>
        public float RangeWidth { get; private set; }



        public static InputEventArgs Create(InputType type,int id,float rangeMin,float rangeWidth)
        {
            InputEventArgs eventArgs = ReferencePool.Get<InputEventArgs>();
            eventArgs.Type = type;
            eventArgs.ID = id;
            eventArgs.RangeMin = rangeMin;
            eventArgs.RangeWidth = rangeWidth;

            return eventArgs;
        }

        public void Clear()
        {
            Type = default;
            ID = default;
            RangeMin = default;
            RangeWidth = default;
        }
    }
}
