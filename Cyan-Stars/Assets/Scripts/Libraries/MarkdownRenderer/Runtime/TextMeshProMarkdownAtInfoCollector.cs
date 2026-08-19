using System;
using System.Collections.Generic;
using CyanStars.MarkdownRenderer.Extensions.AtParagraph;
using CyanStars.MarkdownRenderer.Utils;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using UnityEngine;
using UnityEngine.Events;

namespace CyanStars.MarkdownRenderer
{
    public class TextMeshProMarkdownAtInfoCollector : MonoBehaviour
    {
        [SerializeField] private UnityEvent<IReadOnlyList<AtInfo>> onAtInfoCollected = new UnityEvent<IReadOnlyList<AtInfo>>();

        public UnityEvent<IReadOnlyList<AtInfo>> OnAtInfoCollected => onAtInfoCollected;

        private List<AtInfo> atInfoList = new List<AtInfo>(20);

        public void CollectAtInfo(MarkdownDocument document)
        {
            if (onAtInfoCollected == null || onAtInfoCollected.GetPersistentEventCount() == 0)
            {
                return;
            }

            if (document is null)
            {
                onAtInfoCollected.Invoke(Array.Empty<AtInfo>());
                return;
            }

            atInfoList.Clear();
            MarkdownUtils.CollectCysAtInfoNonAlloc(document, atInfoList);
            onAtInfoCollected.Invoke(atInfoList);
        }
    }
}