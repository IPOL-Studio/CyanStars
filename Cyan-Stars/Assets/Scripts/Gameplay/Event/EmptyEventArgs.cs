using System;
using CyanStars.Framework.RefrencePool;

namespace CyanStars.Gameplay.Event
{
    public class EmptyEventArgs : EventArgs,IReference
    {
        public static EmptyEventArgs Create()
        {
            EmptyEventArgs eventArgs = ReferencePool.Get<EmptyEventArgs>();
            return eventArgs;
        }

        public void Clear()
        {

        }
    }
}
