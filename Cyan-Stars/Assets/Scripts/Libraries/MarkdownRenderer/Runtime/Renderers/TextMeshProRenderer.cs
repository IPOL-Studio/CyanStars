#nullable enable

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Markdig.Renderers;
using CyanStars.MarkdownRenderer.Renderers.TextMeshPro;
using CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines;
using System.Collections.Generic;

namespace CyanStars.MarkdownRenderer.Renderers
{
    public class TextMeshProRenderer : TextRendererBase<TextMeshProRenderer>
    {
        public TextMeshProMarkdownConfig Config { get; set; } = TextMeshProMarkdownConfig.DefaultConfig;

        public int NestingLevel { get; private set; }
        public int QuoteLevel { get; set; }
        public bool IsCompactParagraph { get; set; }


        // computed properties form config
        public string? BlockFakeMarginBottom { get; private set; }

        public readonly struct TmpTagItem
        {
            public readonly string TagName;
            public readonly string? Value;
            public readonly bool IsWrited;

            public TmpTagItem(string tagName, string? value, bool isWrited)
            {
                TagName = tagName;
                Value = value;
                IsWrited = isWrited;
            }
        }

        private Stack<TmpTagItem> tags;

        public TextMeshProRenderer(TextWriter writer) : base(writer)
        {
            // Block renderers
            AddRenderers(
                new ListRenderer(),
                new HeadingRenderer(),
                new HtmlBlockRenderer(),
                new ParagraphRenderer(),
                new QuoteBlockRenderer()
            );

            // Inline renderers
            AddRenderers(
                new CodeInlineRenderer(),
                new EmphasisInlineRenderer(),
                new LineBreakInlineRenderer(),
                new LinkInlineRenderer(),
                new LiteralInlineRenderer()
            );

            tags = new Stack<TmpTagItem>(32);
            ComputeConfig();
        }

        private void AddRenderers(params IMarkdownObjectRenderer[] renderers)
        {
            ObjectRenderers.AddRange(renderers);
        }

        public void FinishBlock(bool appendFakeMarginBottom)
        {
            if (IsLastInContainer || tags.Count > 0)
                return;

            if (appendFakeMarginBottom && !string.IsNullOrEmpty(BlockFakeMarginBottom))
            {
                EnsureFakeMarginBottom();
            }
            else
            {
                EnsureLine();
            }
        }

        private void EnsureFakeMarginBottom()
        {
            EnsureLine();
            WriteRaw("<line-height=");
            WriteRaw(BlockFakeMarginBottom);
            WriteRaw("em>");
            WriteLine();
            WriteRaw("</line-height>");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteRaw(string? content) => Writer.Write(content);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteRaw(char content) => Writer.Write(content);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteRaw(ReadOnlySpan<char> content) => Writer.Write(content);

        public void PushNestingLevel() => NestingLevel++;

        public void PopNestingLevel()
        {
            if (NestingLevel < 0)
            {
                throw new InvalidOperationException("Nesting level cannot be negative.");
            }
            NestingLevel--;
        }

        public void ResetRecordedProps()
        {
            NestingLevel = 0;
            QuoteLevel = 0;
            IsCompactParagraph = false;
            tags.Clear();
            base.Reset();
        }

        public TextMeshProRenderer PushTag(string tagName, string? value = null,
                                           string? valuePrefix = null, string? valueSuffix = null,
                                           bool isWrite = true)
        {
            tags.Push(new TmpTagItem(tagName, value, isWrite));

            if (!isWrite)
            {
                return this;
            }

            WriteRaw('<');
            WriteRaw(tagName);
            if (value != null)
            {
                WriteRaw('=');
                if (valuePrefix != null)
                {
                    WriteRaw(valuePrefix);
                }
                WriteRaw(value);
                if (valueSuffix != null)
                {
                    WriteRaw(valueSuffix);
                }
            }
            WriteRaw('>');
            return this;
        }

        public bool TryPopTag(out TmpTagItem tag)
        {
            bool success = tags.TryPop(out tag);
            if (tag.IsWrited)
            {
                WriteRaw("</");
                WriteRaw(tag.TagName);
                WriteRaw('>');
            }
            return success;
        }

        public void ComputeConfig()
        {
            BlockFakeMarginBottom = Config.BlockFakeMarginBottom <= 0
                ? string.Empty
                : (Math.Round(Config.BlockFakeMarginBottom * 1000) / 1000.0).ToString();
        }
    }
}
