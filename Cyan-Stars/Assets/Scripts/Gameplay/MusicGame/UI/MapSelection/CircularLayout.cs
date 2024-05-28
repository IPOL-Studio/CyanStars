using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace CyanStars.Gameplay.MusicGame
{

    [RequireComponent(typeof(ScrollRect))]
    [RequireComponent(typeof(RectTransform))]
    public class CircularLayout : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IScrollHandler
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

        [Header("初始偏移角度")]
        public float OffsetAngle;

        [Header("水平缩放（非1时为椭圆）")]
        public float ScaleX = 1;

        [Header("使用滚轮切换的冷却时间")]
        public float SelectByScrollingDelta = 0.5f;

        private ScrollRect scrollRect;

        /// <summary>
        /// 第一个item的角度
        /// </summary>
        private float curFirstItemAngle;

        /// <summary>
        /// 是否正在被拖动
        /// </summary>
        private bool isDragging = false;

        /// <summary>
        /// 是否已经吸附
        /// </summary>
        private bool anchored = true;

        private float lastMoveTIme = Mathf.Infinity;

        private float lastScrollTime = -Mathf.Infinity;

        void Awake()
        {
            scrollRect = transform.GetComponent<ScrollRect>();
            scrollRect.scrollSensitivity = 0;  // 禁用鼠标滚轮

            curFirstItemAngle = OffsetAngle;

            float paddingAngle = (Radius != 0) ? (Padding / (2 * Mathf.PI * Radius) * 360) : 0;

            scrollRect.onValueChanged.AddListener((Vector2 value) => {
                float itemsTotalAngle = (Items.Count - 1) * paddingAngle;   // 所有item整体所占的角度
                float centerAngle = (StartAngle + EndAngle) / 2;            // 圆环中央的角度
                curFirstItemAngle = centerAngle - (1 - value.y) * itemsTotalAngle;
                lastMoveTIme = Time.unscaledTime;
            });
        }

        void Update()
        {
            float curItemAngle = curFirstItemAngle;
            float paddingAngle = (Radius != 0) ? (Padding / (2 * Mathf.PI * Radius) * 360) : 0;

            for(int i = 0; i < Items.Count; i ++)
            {
                if (curItemAngle > EndAngle || curItemAngle < StartAngle)
                {
                    Items[i].gameObject.SetActive(false);
                }
                else
                {
                    Items[i].gameObject.SetActive(true);
                    Items[i].transform.position = transform.position + new Vector3(
                        Mathf.Sin(curItemAngle * Mathf.Deg2Rad) * Radius * ScaleX,
                        Mathf.Cos(curItemAngle * Mathf.Deg2Rad) * Radius,
                        0
                    );

                    float centerAngle = (StartAngle + EndAngle) / 2;
                    float distanceToCenter = Mathf.Abs(curItemAngle - centerAngle);
                    float alpha;
                    if (Mathf.Abs(distanceToCenter) < 20)
                    {
                        alpha = 1;
                    }
                    else  // distanceToCenter in [20, centerAngle - StartAngle]
                    {
                        alpha = 1 - Mathf.Pow((distanceToCenter - 20) / (centerAngle - StartAngle - 20), 0.5f);
                    }
                    Items[i].SetAlpha(alpha);
                }

                curItemAngle += paddingAngle;
            }

            if (!anchored && !isDragging && Time.unscaledTime - lastMoveTIme > Time.unscaledDeltaTime)
            {
                scrollRect.StopMovement();
                AnchorCentralItem();
            }
        }

        /// <summary>
        /// 添加控件
        /// </summary>
        public void AddItem(MapItem item)
        {
            Items.Add(item);
            // content.height和Items.Count成正比，以保证滚动速率不表
            scrollRect.content.sizeDelta = new Vector2(60, 100 * Items.Count);
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void ResetItems()
        {
            curFirstItemAngle = OffsetAngle;
            Items.Clear();
        }

        public int GetCurrentCentralItemIndex()
        {
            float centerAngle = (StartAngle + EndAngle) / 2;  // 计算出圆环中心的角度
            float paddingAngle = (Radius != 0) ? (Padding / (2 * Mathf.PI * Radius) * 360) : 0;  // 计算item之间的角度间隔
            // 计算当前最靠近圆环中心的item的index
            /*
            使用Mathf.Max和Mathf.Min将index约束到[0, Items.Count - 1]
            防止index在ScrollRect的Movement Type为Elastic（允许在滑动到尽头时有一定的弹性）时index取值不合法
            */
            return Mathf.Max(Mathf.Min(Mathf.RoundToInt((centerAngle - curFirstItemAngle) / paddingAngle), Items.Count - 1), 0);
        }

        /// <summary>
        /// 吸附到最靠近圆环中央的item
        /// </summary>
        public void AnchorCentralItem()
        {
            anchored = true;
            int curCentralItemIndex = GetCurrentCentralItemIndex();
            MoveToItemAt(curCentralItemIndex);
        }

        /// <summary>
        /// 移动某个指定item到圆环中央
        /// </summary>
        public void MoveToItemAt(int targetItemIndex)
        {
            float targetPositionY =
                (scrollRect.content.sizeDelta.y - GetComponent<RectTransform>().sizeDelta.y) *
                targetItemIndex / (Items.Count - 1);

            // 如果item是第一个或最后一个，并且当前content的PosY超出了合法范围，则会自动回弹，不需要再启动MoveTo
            if ((targetItemIndex == 0 && scrollRect.content.anchoredPosition.y <= targetPositionY) ||
                (targetItemIndex == Items.Count - 1 && scrollRect.content.anchoredPosition.y >= targetPositionY))
            {
                return;
            }

            Vector2 targetPosition = new Vector2(0, targetPositionY);

            StartCoroutine(MoveTo(targetPosition));
        }

        IEnumerator MoveTo(Vector2 targetPosition)
        {
            Vector2 deltaPosition = targetPosition - scrollRect.content.anchoredPosition;
            int step = 10;

            for (int i = 0; i < step; i ++)
            {
                scrollRect.content.anchoredPosition += deltaPosition * 1 / step;
                yield return null;
            }

            scrollRect.content.anchoredPosition = targetPosition;
        }

        public void OnBeginDrag(PointerEventData pointEventData)
        {
            isDragging = true;
            anchored = false;
        }

        public void OnEndDrag(PointerEventData pointEventData)
        {
            isDragging = false;
        }

        public void OnScroll(PointerEventData pointEventData)
        {
            if (Time.unscaledTime - lastScrollTime < SelectByScrollingDelta) return;

            lastScrollTime = Time.unscaledTime;
            int newMapIndex = GetCurrentCentralItemIndex() - (int)Mathf.Max(Mathf.Min(pointEventData.scrollDelta.y, 1), -1);
            if (0 <= newMapIndex && newMapIndex < Items.Count)
            {
                Items[newMapIndex].Select();
            }
        }
    }
}
