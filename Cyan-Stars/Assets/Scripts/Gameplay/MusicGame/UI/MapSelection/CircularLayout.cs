using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CyanStars.Gameplay.MusicGame
{

    [RequireComponent(typeof(ScrollRect))]
    public class CircularLayout : MonoBehaviour
    {
        [Header("控件列表")]
        public List<MapItem> Items;

        [Header("半径")]
        public float Radius;

        [Header("间距")]
        public float Padding;

        [Header("开始角度")]
        public float StartAngle;

        [Header("结束角度")]
        public float EndAngle;

        [Header("当前偏移角度(已旋转角度)")]
        public float OffsetAngle;

        private ScrollRect scrollRect;

        void Awake()
        {
            scrollRect = transform.GetComponent<ScrollRect>();

            float paddingAngle = (Radius != 0) ? (Padding / (2 * Mathf.PI * Radius) * 360) : 0;
            
            scrollRect.onValueChanged.AddListener((Vector2 value)=>{
                OffsetAngle = StartAngle + value.y * (EndAngle - StartAngle - Items.Count * paddingAngle); 
                Debug.Log(OffsetAngle);
            });
        }

        void Update()
        {
            float curAngle = OffsetAngle;
            float paddingAngle = (Radius != 0) ? (Padding / (2 * Mathf.PI * Radius) * 360) : 0;

            for(int i = 0; i < Items.Count; i ++)
            {
                if (curAngle > EndAngle || curAngle < StartAngle)
                {
                    Items[i].gameObject.SetActive(false);
                }
                else
                {
                    Items[i].gameObject.SetActive(true);
                    Items[i].transform.position = transform.position + new Vector3(
                        Mathf.Sin(curAngle * Mathf.Deg2Rad) * Radius,
                        Mathf.Cos(curAngle * Mathf.Deg2Rad) * Radius,
                        0
                    );

                    // float centerAngle = (StartAngle + EndAngle) / 2f;
                    // float distanceFromCenter = Mathf.Abs((curAngle - centerAngle) / (EndAngle - StartAngle));
                    // float alpha = 1f - distanceFromCenter;
                    // Items[i].SetAlpha(alpha);
                }

                curAngle += paddingAngle;
            }
        }

        public void AddItem(MapItem item)
        {
            Items.Add(item);
        }

    }

}