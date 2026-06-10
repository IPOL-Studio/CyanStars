#nullable enable

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars
{
    /// <summary>
    /// 挂在多行 TMP_InputField 上，用于在换行时接管 layoutElement 的 preferredHeight 计算，以防结尾换行符不计入高度
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    [RequireComponent(typeof(LayoutElement))]
    public class InputFieldRefreshHelper : MonoBehaviour
    {
        [SerializeField]
        private float verticalPadding = 30f;

        private TMP_InputField tmpInputField = null!;
        private LayoutElement layoutElement = null!;

        private void Awake()
        {
            tmpInputField = GetComponent<TMP_InputField>();
            layoutElement = GetComponent<LayoutElement>();
        }

        private void Start() => CalculateHight(tmpInputField.text); // 初始化调用一次

        private void OnEnable() => tmpInputField.onValueChanged.AddListener(CalculateHight);
        private void OnDisable() => tmpInputField.onValueChanged.RemoveListener(CalculateHight);

        private void CalculateHight(string text)
        {
            // 末尾是换行符和空格等不可见字符的情况时，临时追加一个字符用于高度计算。
            string calcText = text;
            if (calcText.EndsWith("\n") || calcText.EndsWith(" "))
                calcText += ".";

            float textWidth = tmpInputField.textComponent.rectTransform.rect.width;
            float textHeight = tmpInputField.textComponent.GetPreferredValues(calcText, textWidth, 0).y;
            textHeight += verticalPadding;
            layoutElement.preferredHeight = Mathf.Max(layoutElement.minHeight, textHeight);
        }
    }
}
