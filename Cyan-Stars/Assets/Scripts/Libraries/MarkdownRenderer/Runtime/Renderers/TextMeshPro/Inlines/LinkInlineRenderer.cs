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
            renderer.WriteRaw("<u>");
            renderer.WriteRaw("<link=");

            var linkPrefix = renderer.Config.LinkPrefix;
            if (!string.IsNullOrEmpty(linkPrefix) || !string.IsNullOrWhiteSpace(linkPrefix))
            {
                renderer.WriteRaw(linkPrefix);
            }

            renderer.WriteRaw(obj.Url);
            renderer.WriteRaw(">");
            renderer.WriteChildren(obj);
            renderer.WriteRaw("</link>");
            renderer.WriteRaw("</u>");
            renderer.WriteRaw("</color>");
        }
    }
}