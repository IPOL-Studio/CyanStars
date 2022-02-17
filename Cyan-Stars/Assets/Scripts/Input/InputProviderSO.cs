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
        public IEnumerable<InputData> Inputs => GetInputIter();
        
        
        protected abstract IEnumerable<InputData> GetInputIter();
    }

    public enum InputState
    {
        Down, Up
    }

    public struct InputData
    {
        public float Pos;
        public InputState State;
        public string Tag;

        public InputData(float pos, InputState state, string tag)
        {
            this.Pos = pos;
            this.State = state;
            this.Tag = tag;
        }
    }
}