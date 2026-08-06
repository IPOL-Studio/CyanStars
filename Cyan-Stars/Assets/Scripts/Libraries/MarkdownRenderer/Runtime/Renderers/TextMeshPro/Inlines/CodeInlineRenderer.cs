using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines
{
    public class CodeInlineRenderer : MarkdownObjectRenderer<TextMeshProRenderer, CodeInline>
    {
        protected override void Write(TextMeshProRenderer renderer, CodeInline obj)
        {
            renderer.Write('<');
            renderer.WriteRaw("mark=#");
            renderer.WriteRaw(renderer.Config.CodeBlockBackgroundColorHex);
            renderer.Write('>');
            renderer.Write(obj.Content);
            renderer.Write("</mark>");
        }
    }
}
