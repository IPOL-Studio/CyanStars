using UnityEngine;
using UnityEngine.EventSystems;
using CyanStars.Framework;

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
        private int id;

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
            id = eventData.pointerId;
            //Debug.Log("按下:" + keyItem.Key );

            Dispatch(InputType.Down);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isTouchDown)
            {
                return;
            }
            //Debug.Log("抬起:" + keyItem.Key );
            isTouchDown = false;
            id = eventData.pointerId;
            Dispatch(InputType.Up);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isTouchDown)
            {
                return;
            }

            isTouchDown = true;
            id = eventData.pointerId;
            //Debug.Log("进入:" + keyItem.Key );

            Dispatch(InputType.Down);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isTouchDown)
            {
                return;
            }
            //Debug.Log("离开:" + keyItem.Key );
            isTouchDown = false;
            id = eventData.pointerId;
            Dispatch(InputType.Up);
        }

        private void Update()
        {
            if (isTouchDown)
            {
                Dispatch(InputType.Press);
            }
        }

        private void Dispatch(InputType type)
        {
            GameRoot.Event.Dispatch(InputEventArgs.EventName, this, InputEventArgs.Create(id, type, keyItem.RangeMin, keyItem.RangeWidth));
        }
    }
}

