using System;
using System.Collections;
using System.Collections.Generic;
using CyanStars.Framework;

using UnityEngine;
using UnityEngine.EventSystems;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 移动端触摸输入接收器
    /// </summary>
    public class TouchInputReceiver : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerEnterHandler,IPointerExitHandler
    {

        [SerializeField]
        private InputMapData.Item keyItem;

        private bool isTouchDown;

        private void Awake()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                //非移动平台就销毁自身
                Destroy(gameObject);
            }
        }

        public void SetKeyItem(InputMapData.Item keyItem)
        {
            this.keyItem = keyItem;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isTouchDown)
            {
                return;
            }

            isTouchDown = true;
            //Debug.Log("按下:" + keyItem.Key );

            GameRoot.Event.Dispatch(InputEventArgs.EventName, this, InputEventArgs.Create(InputType.Down, keyItem));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isTouchDown)
            {
                return;
            }
            //Debug.Log("抬起:" + keyItem.Key );
            isTouchDown = false;
            GameRoot.Event.Dispatch(InputEventArgs.EventName, this, InputEventArgs.Create(InputType.Up, keyItem));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isTouchDown)
            {
                return;
            }

            isTouchDown = true;
            //Debug.Log("进入:" + keyItem.Key );

            GameRoot.Event.Dispatch(InputEventArgs.EventName, this, InputEventArgs.Create(InputType.Down, keyItem));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isTouchDown)
            {
                return;
            }
            //Debug.Log("离开:" + keyItem.Key );
            isTouchDown = false;
            GameRoot.Event.Dispatch(InputEventArgs.EventName, this, InputEventArgs.Create(InputType.Up, keyItem));
        }

        private void Update()
        {
            if (isTouchDown)
            {
                GameRoot.Event.Dispatch(InputEventArgs.EventName, this, InputEventArgs.Create(InputType.Press, keyItem));
            }
        }


    }
}

