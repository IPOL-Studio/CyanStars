using System;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines
{
    public class LiteralInlineRenderer : MarkdownObjectRenderer<TextMeshProRenderer, LiteralInline>
    {
        protected override void Write(TextMeshProRenderer renderer, LiteralInline obj)
        {
            var span = obj.Content.AsSpan();
            if (span.Length == 1 && span[0] == '\\')
            {
                renderer.Write(@"\\");
            }
            else if (span.Length == 2 && span[0] == '\\' && span[1] == '\\')
            {
                renderer.Write(@"\\\\");
            }
            else
            {
                renderer.Write(ref obj.Content);
            }

        }
    }
}