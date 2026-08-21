using CyanStars.MarkdownRenderer.Renderers;
using CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines;
using Markdig;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;

namespace CyanStars.MarkdownRenderer.Extensions.StaffAt
{
    public sealed class StaffAtExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.InlineParsers.Contains<StaffAtInlineParser>())
            {
                pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new StaffAtInlineParser());
            }
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is TextMeshProRenderer textMeshProRenderer &&
                textMeshProRenderer.ObjectRenderers.FindExact<StaffAtInlineRenderer>() == null)
            {
                textMeshProRenderer.ObjectRenderers.InsertBefore<LinkInlineRenderer>(new StaffAtInlineRenderer());
            }
        }
    }
}
