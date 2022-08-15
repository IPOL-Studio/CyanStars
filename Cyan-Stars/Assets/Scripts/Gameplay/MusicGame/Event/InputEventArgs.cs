using System;
using CyanStars.Framework.Pool;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 输入事件
    /// </summary>
    public class InputEventArgs : EventArgs, IReference
    {
        public const string EventName = nameof(InputEventArgs);

        public int ID { get; private set; }

        public InputType Type { get; private set; }

        public  float RangeMin { get; private set; }

        public float RangeWidth { get; private set; }

        public static InputEventArgs Create(int id, InputType type, float rangeMin, float rangeWidth)
        {
            InputEventArgs eventArgs = ReferencePool.Get<InputEventArgs>();
            eventArgs.ID = id;
            eventArgs.Type = type;
            eventArgs.RangeMin = rangeMin;
            eventArgs.RangeWidth = rangeWidth;
            return eventArgs;
        }

        public void Clear()
        {
            ID = default;
            Type = default;
            RangeMin = default;
            RangeWidth = default;
        }
    }
}
