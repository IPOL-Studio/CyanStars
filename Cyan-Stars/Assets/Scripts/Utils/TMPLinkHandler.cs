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
        None,

        /// <summary>
        /// 把整个 link 参数作为 Http/Https URL 在浏览器中打开。
        /// </summary>
        OpenHttpUrl
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
        private List<string> openHttpUrlPrefixes = new();

        [SerializeField]
        [Tooltip("link 参数未匹配任何前缀时的兜底行为。")]
        private FallbackActionType fallbackAction = FallbackActionType.None;

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
                if (!string.IsNullOrWhiteSpace(url) && IsHttpUrl(url, out string validUrl1))
                    Application.OpenURL(validUrl1);
                return;
            }

            if (fallbackAction == FallbackActionType.OpenHttpUrl && IsHttpUrl(linkId, out string validUrl2))
                Application.OpenURL(validUrl2);
        }

        /// <summary>
        /// 校验传入的 URL 是否为 http:// 或 https:// 形式，规范 url 前后与中间的空格，返回规范化的 url
        /// </summary>
        private static bool IsHttpUrl(string url, out string validUrl)
        {
            validUrl = string.Empty;

            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out Uri? uri) || uri == null)
                return false;

            if ((uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
                string.IsNullOrEmpty(uri.Host))
            {
                return false;
            }

            validUrl = uri.AbsoluteUri; // 空格会变成 %20，前后空格被去掉
            return true;
        }

        /// <summary>
        /// 获取最长的匹配
        /// </summary>
        private string? FindLongestMatchingPrefix(string linkId)
        {
            string? matchedPrefix = null;

            foreach (string prefix in openHttpUrlPrefixes)
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
