#nullable enable

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Utils
{
    /// <summary>
    /// 自定义 ScrollRect，用于分离鼠标拖拽和滚轮的控制和静默更新
    /// </summary>
    public class CustomScrollRect : ScrollRect
    {
        #region 分离鼠标拖拽与滚轮滚动

        /// <summary>
        /// 是否允许鼠标/触屏拖动
        /// </summary>
        public bool IsDragEnabled { get; set; } = true;

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsDragEnabled)
                return;

            base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!IsDragEnabled)
                return;

            base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!IsDragEnabled)
                return;

            base.OnEndDrag(eventData);
        }

        #endregion

        #region 拦截变化事件


        /// <summary>
        /// 拦截 NormalizedPosition 变化的通知时间标记
        /// </summary>
        private bool suppressEventInLateUpdate = false;

        /// <summary>
        /// 空事件，让 LateUpdate 无法向实际的订阅发出通知，缓存以避免频繁实例化。
        /// </summary>
        private readonly ScrollRectEvent EmptyEventCache = new();

        /// <summary>
        /// 设置归一化位置，但不触发 onValueChanged 事件
        /// </summary>
        public void SetNormalizedPositionWithoutNotify(Vector2 value)
        {
            this.normalizedPosition = value;
            suppressEventInLateUpdate = true;
        }

        /// <summary>
        /// 重写 LateUpdate，拦截 ScrollRect 内部的事件触发
        /// </summary>
        protected override void LateUpdate()
        {
            if (suppressEventInLateUpdate)
            {
                // 暂存原本的事件回调对象，替换为一个新的空事件。这样基类在执行 Invoke() 时，实际上调用的是空事件，不会通知到外部。
                var originalEvent = this.onValueChanged;
                this.onValueChanged = EmptyEventCache;

                base.LateUpdate();

                this.onValueChanged = originalEvent;
                suppressEventInLateUpdate = false;
            }
            else
            {
                base.LateUpdate();
            }
        }

        #endregion
    }
}
