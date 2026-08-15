using Markdig.Renderers;
using Markdig.Syntax;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro
{
    public class ParagraphRenderer : MarkdownObjectRenderer<TextMeshProRenderer, ParagraphBlock>
    {
        protected override void Write(TextMeshProRenderer renderer, ParagraphBlock obj)
        {
            renderer.WriteLeafInline(obj);
            renderer.FinishBlock(!renderer.IsCompactParagraph);
        }
    }

    // public class ThematicBreakRenderer : MarkdownObjectRenderer<TextMeshProRenderer, ThematicBreakBlock>
    // {
    //     protected override void Write(TextMeshProRenderer renderer, ThematicBreakBlock obj)
    //     {
    //         renderer.EnsureLine();
    //         renderer.WriteLine("<hr/>");
    //         renderer.EnsureLine();
    //     }
    // }

}
