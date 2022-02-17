using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Input
{
    public abstract class InputProviderSO : ScriptableObject
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>float - pos(0-1)  </returns>
        /// <returns>InputState - state  </returns>
        /// <returns>string - tag</returns>
        public abstract IEnumerable<(float, InputState, string)> GetInputEnumerator();
    }

    public enum InputState
    {
        Down, Up
    }
}