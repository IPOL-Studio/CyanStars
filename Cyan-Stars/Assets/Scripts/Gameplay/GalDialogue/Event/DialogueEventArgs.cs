using System;
using System.Collections;
using System.Collections.Generic;
using CyanStars.Framework.Pool;
using UnityEngine;

namespace CyanStars.Gameplay.Dialogue
{
    public class DialogueEventArgs : EventArgs, IReference
    {
        public int index;

        public static DialogueEventArgs Create(int index)
        {
            DialogueEventArgs eventArgs = new DialogueEventArgs();
            eventArgs.index = index;
            return eventArgs;
        }

        public void Clear()
        {

        }
    }
}
