using System;
using System.Collections.Generic;
using CyanStars.MarkdownRenderer.Extensions.AtParagraph;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using UnityEngine;
using UnityEngine.Events;

namespace CyanStars.MarkdownRenderer
{
    public class TextMeshProMarkdownAtInfoCollector : MonoBehaviour
    {
        #nullable enable
        public readonly struct AtInfo
        {
            public readonly string Content;
            public readonly string? Link;

            public AtInfo(string content, string? link)
            {
                Content = content;
                Link = link;
            }
        }
        #nullable disable

        [SerializeField] private UnityEvent<IReadOnlyList<AtInfo>> onAtInfoCollected = new UnityEvent<IReadOnlyList<AtInfo>>();

        public UnityEvent<IReadOnlyList<AtInfo>> OnAtInfoCollected => onAtInfoCollected;

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

            var atInfo = new List<AtInfo>();
            CollectAtInfo(document, atInfo);
            onAtInfoCollected.Invoke(atInfo);
        }

        private static void CollectAtInfo(ContainerBlock block, List<AtInfo> atInfo)
        {
            foreach (var child in block)
            {
                if (child is ContainerBlock containerBlock)
                {
                    CollectAtInfo(containerBlock, atInfo);
                }
                else if (child is LeafBlock leafBlock && leafBlock.Inline != null)
                {
                    CollectAtInfo(leafBlock.Inline, atInfo);
                }
            }
        }

        private static void CollectAtInfo(ContainerInline container, List<AtInfo> atInfo)
        {
            foreach (var inline in container.FindDescendants<AtParagraphInline, LinkInline>())
            {
                if (inline is AtParagraphInline atParagraphInline)
                {
                    atInfo.Add(new AtInfo(atParagraphInline.Paragraph, atParagraphInline.Url));
                    continue;
                }

                if (TryGetAtInfo((LinkInline)inline, out var info))
                {
                    atInfo.Add(info);
                    continue;
                }
            }
        }

        private static bool TryGetAtInfo(LinkInline inline, out AtInfo info)
        {
            if (!inline.IsShortcut && inline.LocalLabel == LocalLabel.None &&
                inline.FirstChild is LiteralInline literalInline && literalInline.NextSibling == null)
            {
                var content = literalInline.Content.ToString();
                if (content.StartsWith('@') && content.Length > 1)
                {
                    info = new AtInfo(content.Substring(1), inline.Url);
                    return true;
                }
            }

            info = default;
            return false;
        }
    }
}