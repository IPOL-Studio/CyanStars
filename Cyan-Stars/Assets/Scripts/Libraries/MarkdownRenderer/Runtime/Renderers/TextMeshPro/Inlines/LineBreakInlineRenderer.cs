using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines
{
    public class LineBreakInlineRenderer : MarkdownObjectRenderer<TextMeshProRenderer, LineBreakInline>
    {
        protected override void Write(TextMeshProRenderer renderer, LineBreakInline obj)
        {
            if (renderer.IsLastInContainer)
            {
                return;
            }

            if (obj.IsHard)
            {
                renderer.WriteRaw("<br>");
            }
            else
            {
                renderer.WriteRaw(' ');
            }
        }
    }
}
