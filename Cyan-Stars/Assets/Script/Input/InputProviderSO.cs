using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Input
{
    public abstract class InputProviderSO : ScriptableObject
    {
        /// <summary>
        /// Get current valid inputs
        /// </summary>
        /// <returns>float - pos(0-1)  </returns>
        /// <returns>InputState - state  </returns>
        /// <returns>string - tag</returns>
        public IEnumerable<(float, InputState, string)> Inputs => GetInputIter();
        
        
        protected abstract IEnumerable<(float, InputState, string)> GetInputIter();
    }

    public enum InputState
    {
        Down, Up
    }
}