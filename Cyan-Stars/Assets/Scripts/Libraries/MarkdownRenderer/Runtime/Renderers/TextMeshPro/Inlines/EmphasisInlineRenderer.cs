using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines
{
    public class EmphasisInlineRenderer : MarkdownObjectRenderer<TextMeshProRenderer, EmphasisInline>
    {
        protected override void Write(TextMeshProRenderer renderer, EmphasisInline obj)
        {
            string tag = obj.DelimiterCount == 2 ? "b" : "i";
            renderer.PushTag(tag);
            renderer.WriteChildren(obj);
            renderer.TryPopTag(out _);
        }
    }
}
