using System;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines
{
    public class LiteralInlineRenderer : MarkdownObjectRenderer<TextMeshProRenderer, LiteralInline>
    {
        protected override void Write(TextMeshProRenderer renderer, LiteralInline obj)
        {
            renderer.WriteLiteral(ref obj.Content);
        }
    }
}