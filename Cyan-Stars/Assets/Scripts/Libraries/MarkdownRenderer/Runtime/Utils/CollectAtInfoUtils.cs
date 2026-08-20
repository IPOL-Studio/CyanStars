#nullable enable

using System;
using System.Collections.Generic;
using CyanStars.MarkdownRenderer.Extensions.AtParagraph;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer.Utils
{
    public readonly struct AtInfo
    {
        public readonly string Content;
        public readonly string? Link;

        public AtInfo(string content, string? link)
        {
            Content = content;
            Link = link;
        }

        public static implicit operator AtInfo((string content, string? link) tuple) =>
            new(tuple.content, tuple.link);
    }

    internal static class CollectAtInfoUtils
    {
        public static int CollectAtInfo(ContainerBlock block, IList<AtInfo> atInfoList)
        {
            int count = 0;

            foreach (var child in block)
            {
                if (child is ContainerBlock containerBlock)
                {
                    count += CollectAtInfo(containerBlock, atInfoList);
                }
                else if (child is LeafBlock leafBlock && leafBlock.Inline != null)
                {
                    count += CollectAtInfo(leafBlock.Inline, atInfoList);
                }
            }
            return count;
        }

        public static int CollectAtInfo(ContainerBlock block, Span<AtInfo> span)
        {
            if (span.IsEmpty)
            {
                return 0;
            }

            int count = 0;
            foreach (var child in block)
            {
                if (child is ContainerBlock containerBlock)
                {
                    count += CollectAtInfo(containerBlock, span.Slice(count));
                }
                else if (child is LeafBlock leafBlock && leafBlock.Inline != null)
                {
                    count += CollectAtInfo(leafBlock.Inline, span.Slice(count));
                }

                if (count >= span.Length)
                {
                    break;
                }
            }
            return count;
        }

        private static int CollectAtInfo(ContainerInline container, IList<AtInfo> atInfoList)
        {
            int count = 0;
            foreach (var inline in container.FindDescendants<AtParagraphInline, LinkInline>())
            {
                if (inline is AtParagraphInline atParagraphInline)
                {
                    atInfoList.Add((atParagraphInline.Paragraph.ToString(), atParagraphInline.Url));
                    count++;
                    continue;
                }

                if (((LinkInline)inline).TryGetAtInfo(out var info))
                {
                    atInfoList.Add(info);
                    count++;
                }
            }
            return count;
        }

        private static int CollectAtInfo(ContainerInline container, Span<AtInfo> span)
        {
            int count = 0;
            foreach (var inline in container.FindDescendants<AtParagraphInline, LinkInline>())
            {
                if (inline is AtParagraphInline atParagraphInline)
                {
                    span[count] = (atParagraphInline.Paragraph.ToString(), atParagraphInline.Url);
                    count++;
                }
                else if (((LinkInline)inline).TryGetAtInfo(out var info))
                {
                    span[count] = info;
                    count++;
                }
                else 
                {
                    continue;
                }

                if (count >= span.Length)
                {
                    break;
                }
            }
            return count;
        }

        private static bool TryGetAtInfo(this LinkInline inline, out AtInfo info)
        {
            if (!inline.IsShortcut && inline.LocalLabel == LocalLabel.None &&
                inline.FirstChild is LiteralInline literalInline && literalInline.NextSibling == null)
            {
                var content = literalInline.Content.ToString();
                if (content.StartsWith('@') && content.Length > 1)
                {
                    info = (content.Substring(1), inline.Url);
                    return true;
                }
            }

            info = default;
            return false;
        }
    }
}
