using CyanStars.MarkdownRenderer.Extensions.AtParagraph;
using Markdig.Renderers;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines
{
    public sealed class AtParagraphInlineRenderer : MarkdownObjectRenderer<TextMeshProRenderer, AtParagraphInline>
    {
        protected override void Write(TextMeshProRenderer renderer, AtParagraphInline obj)
        {
            // TODO: 确定 at 条目的 link tag spec
            renderer.PushTag("color", renderer.Config.LinkColorHex, valuePrefix: "#")
                    .PushTag("u");
            renderer.WriteChildren(obj);
            renderer.PopTag(2);
        }
    }
}