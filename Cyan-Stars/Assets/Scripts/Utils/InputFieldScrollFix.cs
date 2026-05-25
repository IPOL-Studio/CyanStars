#nullable enable

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.Utils
{
    /// <summary>
    /// 挂在多行 TMP_InputField 上，以确保鼠标悬浮/拖动输入框时能正确传递给父级 ScrollRect，而不是被 TMP_InputField 消费掉
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class InputFieldScrollFix : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        private ScrollRect? parentScrollRect;

        private void Awake()
        {
            parentScrollRect = GetComponentInParent<ScrollRect>();

            if (parentScrollRect == null)
                Debug.LogWarning($"{nameof(parentScrollRect)} 为空，{nameof(InputFieldScrollFix)} 将无任何效果。");
        }

        public void OnScroll(PointerEventData eventData) => parentScrollRect?.OnScroll(eventData);
        public void OnBeginDrag(PointerEventData eventData) => parentScrollRect?.OnBeginDrag(eventData);
        public void OnDrag(PointerEventData eventData) => parentScrollRect?.OnDrag(eventData);
        public void OnEndDrag(PointerEventData eventData) => parentScrollRect?.OnEndDrag(eventData);
    }
}
