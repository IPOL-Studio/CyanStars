#nullable enable

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars
{
    /// <summary>
    /// 挂在多行 TMP_InputField 上，用于在换行时接管 layoutElement 的 preferredHeight 计算，以防结尾换行符不计入高度
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    [RequireComponent(typeof(LayoutElement))]
    public class InputFieldRefreshHelper : UIBehaviour
    {
        [SerializeField]
        private float verticalPadding = 30f;

        private TMP_InputField tmpInputField = null!;
        private LayoutElement layoutElement = null!;

        protected override void Awake()
        {
            base.Awake();
            tmpInputField = GetComponent<TMP_InputField>();
            layoutElement = GetComponent<LayoutElement>();
        }

        protected override void Start()
        {
            base.Start();
            CalculateHeight(tmpInputField.text); // 初始化调用一次
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            tmpInputField.onValueChanged.AddListener(CalculateHeight);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            tmpInputField.onValueChanged.RemoveListener(CalculateHeight);
        }


        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            if (tmpInputField != null && isActiveAndEnabled)
            {
                CalculateHeight(tmpInputField.text);
            }
        }


        private void CalculateHeight(string text)
        {
            float textWidth = tmpInputField.textComponent.rectTransform.rect.width;
            if (textWidth <= 0.01f)
                return; // TMP 输入框尚未完成初始化

            // 末尾是换行符和空格等不可见字符的情况时，临时追加一个字符用于高度计算。
            string calcText = text;
            if (calcText.EndsWith("\n") || calcText.EndsWith(" "))
                calcText += ".";

            float textHeight = tmpInputField.textComponent.GetPreferredValues(calcText, textWidth, 0).y;
            textHeight += verticalPadding;

            float newHeight = Mathf.Max(layoutElement.minHeight, textHeight);
            
            if (Mathf.Abs(layoutElement.preferredHeight - newHeight) > 0.1f)
                layoutElement.preferredHeight = newHeight;
        }
    }
}
