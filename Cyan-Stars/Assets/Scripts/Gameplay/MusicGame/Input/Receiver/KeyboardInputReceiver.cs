using System.Collections.Generic;
using CyanStars.Framework;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 键盘输入接收器
    /// </summary>
    public class KeyboardInputReceiver : BaseInputReceiver
    {
        private readonly HashSet<KeyCode> PressedKeySet = new HashSet<KeyCode>();

        public KeyboardInputReceiver(InputMapData data) : base(data)
        {
        }

        public override void StartReceive()
        {
            GameRoot.Timer.AddUpdateTimer(OnUpdate);
        }

        public override void EndReceive()
        {
            GameRoot.Timer.RemoveUpdateTimer(OnUpdate);
        }

        private void OnUpdate(float deltaTime)
        {
            foreach (InputMapData.Item item in InputMapData.Items)
            {
                if (Input.GetKeyDown(item.Key))
                {
                    PressedKeySet.Add(item.Key);
                    Dispatch(InputType.Down,(int)item.Key,item.RangeMin,item.RangeWidth);
                    continue;
                }

                if (Input.GetKey(item.Key))
                {
                    Dispatch(InputType.Press,(int)item.Key,item.RangeMin,item.RangeWidth);
                    continue;
                }

                if (PressedKeySet.Remove(item.Key))
                {
                    Dispatch(InputType.Up,(int)item.Key,item.RangeMin,item.RangeWidth);
                }
            }
        }

        private void Dispatch(InputType type,int id,float rangeMin,float rangeWidth)
        {
            GameRoot.Event.Dispatch(InputEventArgs.EventName,this,InputEventArgs.Create(type,id,rangeMin,rangeWidth));
        }


    }
}
