#nullable enable

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Markdig.Renderers;
using CyanStars.MarkdownRenderer.Renderers.TextMeshPro;
using CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines;

namespace CyanStars.MarkdownRenderer.Renderers
{
    public class TextMeshProRenderer : TextRendererBase<TextMeshProRenderer>
    {
        public TextMeshProMarkdownConfig Config { get; set; } = TextMeshProMarkdownConfig.DefaultConfig;

        public bool SkipNextEnsureLine { get; set; }

        public TextMeshProRenderer(TextWriter writer) : base(writer)
        {
            // Block renderers
            AddRenderers(
                new ListRenderer(),
                new HeadingRenderer(),
                new HtmlBlockRenderer(),
                new ParagraphRenderer()
                //new QuoteBlockRenderer()
            );

            // Inline renderers
            AddRenderers(
                new CodeInlineRenderer(),
                new EmphasisInlineRenderer(),
                new LinkInlineRenderer(),
                new LiteralInlineRenderer()
            );
        }

        private void AddRenderers(params IMarkdownObjectRenderer[] renderers)
        {
            ObjectRenderers.AddRange(renderers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteRaw(string? content) => Writer.Write(content);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteRaw(char content) => Writer.Write(content);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteRaw(ReadOnlySpan<char> content) => Writer.Write(content);

        public bool TryEnsureLineIfNotSkip(bool isConsumingSkip)
        {
            if (SkipNextEnsureLine)
            {
                if (isConsumingSkip)
                {
                    SkipNextEnsureLine = false;
                }
                return false;
            }
            EnsureLine();
            return true;
        }

        public void ResetRecordedProps() => base.Reset();
    }
}
