using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines
{
    public class CodeInlineRenderer : MarkdownObjectRenderer<TextMeshProRenderer, CodeInline>
    {
        protected override void Write(TextMeshProRenderer renderer, CodeInline obj)
        {
            renderer.WriteRaw('<');
            renderer.WriteRaw("mark=#");
            renderer.WriteRaw(renderer.Config.CodeBlockBackgroundColorHex);
            renderer.WriteRaw('>');
            renderer.WriteRaw("<noparse>");
            renderer.Write(obj.Content);
            renderer.WriteRaw("</noparse>");
            renderer.WriteRaw("</mark>");
        }
    }
}
