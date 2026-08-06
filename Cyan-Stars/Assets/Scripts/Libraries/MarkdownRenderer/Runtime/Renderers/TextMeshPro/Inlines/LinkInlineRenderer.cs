using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines
{
    public class LinkInlineRenderer : MarkdownObjectRenderer<TextMeshProRenderer, LinkInline>
    {
        protected override void Write(TextMeshProRenderer renderer, LinkInline obj)
        {
            renderer.WriteRaw("<color=#");
            renderer.WriteRaw(renderer.Config.LinkColorHex);
            renderer.WriteRaw(">");
            renderer.WriteChildren(obj);
            renderer.WriteRaw("</color>");
        }
    }
}