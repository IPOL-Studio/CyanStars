using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 触摸输入接收器
    /// </summary>
    public class TouchInputReceiver : BaseInputReceiver
    {
        private const float Width = 30f;
        private TouchInputReceiveObj[] objs = new TouchInputReceiveObj[14];

        public TouchInputReceiver(InputMapData data) : base(data)
        {
        }

        /// <summary>
        /// 创建触屏输入接收对象
        /// </summary>
        public void CreateReceiveObj(GameObject prefab,Transform parent)
        {
            for (int i = 0; i < 12; i++)
            {
                GameObject obj = Object.Instantiate(prefab,default,default,parent);
                obj.name = "TouchInputReceiveObj_" + i;
                InputMapData.Item item = InputMapData.Items[i];
                Vector3 pos = default;
                pos.x = item.RangeMin * Width;
                obj.transform.localPosition = pos;

                objs[i] = obj.GetComponent<TouchInputReceiveObj>();
                objs[i].SetData((int)item.Key,item.RangeMin,item.RangeWidth);

            }

            objs[12] = parent.Find("TouchInputReceiveObj_BreakLeft").GetComponent<TouchInputReceiveObj>();
            objs[13] = parent.Find("TouchInputReceiveObj_BreakRight").GetComponent<TouchInputReceiveObj>();
        }

        public override void StartReceive()
        {
            foreach (TouchInputReceiveObj obj in objs)
            {
                obj.gameObject.SetActive(true);
            }
        }

        public override void EndReceive()
        {
            foreach (TouchInputReceiveObj obj in objs)
            {
                obj.ResetTouch();
                obj.gameObject.SetActive(false);
            }
        }
    }
}
