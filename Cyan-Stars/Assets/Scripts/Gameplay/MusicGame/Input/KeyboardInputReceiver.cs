using System.Collections.Generic;
using UnityEngine;
using CyanStars.Framework;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 键盘输入接收器
    /// </summary>
    [DefaultExecutionOrder(-100)]  // 保证键盘输入在Timeline Update之前
    public class KeyboardInputReceiver : MonoBehaviour
    {
        private bool canUpdate;
        private InputMapData inputMapData;
        private HashSet<KeyCode> pressedKeySet = new HashSet<KeyCode>();

        public void SetInputMapData(InputMapData data)
        {
            inputMapData = data;
            canUpdate = inputMapData != null;
        }

        private void Update()
        {
            if (canUpdate)
            {
                CheckKeyboardInput();
            }
        }

        private void CheckKeyboardInput()
        {
            for (int i = 0; i < inputMapData.Items.Count; i++)
            {
                InputMapData.Item item = inputMapData.Items[i];
                if (Input.GetKeyDown(item.Key))
                {
                    pressedKeySet.Add(item.Key);
                    Dispatch(InputEventArgs.Create((int)item.Key, InputType.Down, item.RangeMin, item.RangeWidth));
                    continue;
                }

                if (Input.GetKey(item.Key))
                {
                    Dispatch(InputEventArgs.Create((int)item.Key, InputType.Press, item.RangeMin, item.RangeWidth));
                    continue;
                }

                if (pressedKeySet.Remove(item.Key))
                {
                    Dispatch(InputEventArgs.Create((int)item.Key, InputType.Up, item.RangeMin, item.RangeWidth));
                }
            }
        }

        private void Dispatch(InputEventArgs e)
        {
            GameRoot.Event.Dispatch(InputEventArgs.EventName, this, e);
        }
    }
}
