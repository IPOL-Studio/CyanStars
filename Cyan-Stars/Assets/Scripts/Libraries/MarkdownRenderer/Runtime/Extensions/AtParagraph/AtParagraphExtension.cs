using CyanStars.MarkdownRenderer.Renderers;
using CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines;
using Markdig;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;

namespace CyanStars.MarkdownRenderer.Extensions.AtParagraph
{
    public sealed class AtParagraphExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.InlineParsers.Contains<AtParagraphInlineParser>())
            {
                pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new AtParagraphInlineParser());
            }
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is TextMeshProRenderer textMeshProRenderer &&
                textMeshProRenderer.ObjectRenderers.FindExact<AtParagraphInlineRenderer>() == null)
            {
                textMeshProRenderer.ObjectRenderers.InsertBefore<LinkInlineRenderer>(new AtParagraphInlineRenderer());
            }
        }
    }
}
