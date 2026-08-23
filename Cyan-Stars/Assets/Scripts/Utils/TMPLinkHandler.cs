#nullable enable

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CyanStars.Utils
{
    /// <summary>
    /// link 参数未匹配任何前缀时的兜底行为。
    /// </summary>
    public enum FallbackActionType
    {
        /// <summary>
        /// 不处理该链接。
        /// </summary>
        Null,

        /// <summary>
        /// 把整个 link 参数作为 URL 在浏览器中打开。
        /// </summary>
        OpenUrl
    }

    /// <summary>
    /// 处理同一物体上 <see cref="TextMeshProUGUI"/> 中富文本 <c>&lt;link&gt;</c> 的点击事件。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TMPLinkHandler : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        [Tooltip("link 参数以列表中任意前缀开头时，剥离该前缀后把剩余内容作为 URL 在浏览器中打开；空项会被忽略。")]
        private List<string> openUrlPrefixes = new();

        [SerializeField]
        [Tooltip("link 参数未匹配任何前缀时的兜底行为。")]
        private FallbackActionType fallbackAction = FallbackActionType.Null;

        private TextMeshProUGUI textComponent = null!;


        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(
                textComponent,
                eventData.position,
                eventData.pressEventCamera
            );

            if (linkIndex == -1)
                return;

            TMP_LinkInfo linkInfo = textComponent.textInfo.linkInfo[linkIndex];
            HandleLink(linkInfo.GetLinkID());
        }

        private void HandleLink(string linkId)
        {
            string? matchedPrefix = FindLongestMatchingPrefix(linkId);
            if (matchedPrefix != null)
            {
                string url = linkId.Substring(matchedPrefix.Length);
                if (!string.IsNullOrWhiteSpace(url))
                    Application.OpenURL(url);
                return;
            }

            if (fallbackAction == FallbackActionType.OpenUrl)
                Application.OpenURL(linkId);
        }

        /// <summary>
        /// 获取最长的匹配
        /// </summary>
        private string? FindLongestMatchingPrefix(string linkId)
        {
            string? matchedPrefix = null;

            foreach (string prefix in openUrlPrefixes)
            {
                if (string.IsNullOrEmpty(prefix))
                    continue;

                if (!linkId.StartsWith(prefix, StringComparison.Ordinal))
                    continue;

                if (matchedPrefix == null || prefix.Length > matchedPrefix.Length)
                    matchedPrefix = prefix;
            }

            return matchedPrefix;
        }
    }
}
