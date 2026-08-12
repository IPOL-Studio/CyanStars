using CyanStars.MarkdownRenderer.Parsers;
using Markdig.Renderers;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines
{
    public sealed class AtParagraphInlineRenderer : MarkdownObjectRenderer<TextMeshProRenderer, AtParagraphInline>
    {
        protected override void Write(TextMeshProRenderer renderer, AtParagraphInline obj)
        {
            renderer.PushTag("link", obj.Url, valuePrefix: renderer.Config.AtColorHex);
            renderer.WriteChildren(obj);
            renderer.TryPopTag(out _);
        }
    }
}