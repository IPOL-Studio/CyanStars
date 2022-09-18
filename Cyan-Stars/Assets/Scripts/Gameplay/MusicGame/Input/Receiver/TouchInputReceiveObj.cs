using System;
using System.Collections;
using System.Collections.Generic;
using CyanStars.Framework;
using CyanStars.Framework.Timer;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 移动端触摸输入接收物体
    /// </summary>
    public class TouchInputReceiveObj : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerEnterHandler,IPointerExitHandler
    {

        [SerializeField]
        private int id;

        [SerializeField]
        private float rangeMin;

        [SerializeField]
        private float rangeWidth;

        private bool isTouchDown;

        private void Awake()
        {
            GameRoot.Timer.AddUpdateTimer(OnUpdate);
        }

        private void OnDestroy()
        {
            GameRoot.Timer.RemoveUpdateTimer(OnUpdate);
        }

        private void OnUpdate(float deltaTime,object userdata)
        {
            if (isTouchDown)
            {
                Dispatch(InputType.Press);
            }
        }

        public void SetData(int id,float rangeMin,float rangeWidth)
        {
            this.id = id;
            this.rangeMin = rangeMin;
            this.rangeWidth = rangeWidth;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isTouchDown)
            {
                return;
            }

            isTouchDown = true;
            Dispatch(InputType.Down);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isTouchDown)
            {
                return;
            }

            isTouchDown = false;
            Dispatch(InputType.Up);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isTouchDown)
            {
                return;
            }

            isTouchDown = true;
            Dispatch(InputType.Down);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isTouchDown)
            {
                return;
            }

            isTouchDown = false;
            Dispatch(InputType.Up);
        }


        private void Dispatch(InputType type)
        {
            GameRoot.Event.Dispatch(InputEventArgs.EventName,this,InputEventArgs.Create(type,id,rangeMin,rangeWidth));
        }


    }
}

