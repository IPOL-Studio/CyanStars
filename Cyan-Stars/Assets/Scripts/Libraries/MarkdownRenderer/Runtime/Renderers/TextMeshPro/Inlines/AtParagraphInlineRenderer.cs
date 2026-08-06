using CyanStars.MarkdownRenderer.Parsers;
using Markdig.Renderers;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines
{
    public sealed class AtParagraphInlineRenderer : MarkdownObjectRenderer<TextMeshProRenderer, AtParagraphInline>
    {
        protected override void Write(TextMeshProRenderer renderer, AtParagraphInline obj)
        {
            renderer.WriteRaw("<color=#");
            renderer.WriteRaw(renderer.Config.AtColorHex);
            renderer.WriteRaw(">");
            renderer.WriteChildren(obj);
            renderer.WriteRaw("</color>");
        }
    }
}