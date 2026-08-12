using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines
{
    public class CodeInlineRenderer : MarkdownObjectRenderer<TextMeshProRenderer, CodeInline>
    {
        protected override void Write(TextMeshProRenderer renderer, CodeInline obj)
        {
            renderer.PushTag("mark", renderer.Config.CodeBlockBackgroundColorHex, valuePrefix: "#")
                    .PushTag("noparse")
                    .Write(obj.Content)
                    .TryPopTag(out _);
            renderer.TryPopTag(out _);
        }
    }
}
