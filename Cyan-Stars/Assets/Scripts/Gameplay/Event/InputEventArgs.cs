using System;
using CyanStars.Framework.RefrencePool;
using CyanStars.Gameplay.Input;

namespace CyanStars.Gameplay.Event
{
    /// <summary>
    /// 输入事件
    /// </summary>
    public class InputEventArgs : EventArgs,IReference
    {
        public const string EventName = nameof(InputEventArgs);

        public InputType Type { get; private set; }
        public InputMapData.Item Item { get; private set; }



        public static InputEventArgs Create(InputType type,InputMapData.Item item)
        {
            InputEventArgs eventArgs = ReferencePool.Get<InputEventArgs>();
            eventArgs.Type = type;
            eventArgs.Item = item;
            return eventArgs;
        }

        public void Clear()
        {
            Type = default;
            Item = default;
        }
    }
}
