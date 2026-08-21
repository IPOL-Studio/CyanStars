using CyanStars.MarkdownRenderer.Renderers;
using Markdig.Renderers;

namespace CyanStars.MarkdownRenderer.Extensions.StaffAt
{
    public sealed class StaffAtInlineRenderer : MarkdownObjectRenderer<TextMeshProRenderer, StaffAtInline>
    {
        protected override void Write(TextMeshProRenderer renderer, StaffAtInline obj)
        {
            // TODO: 确定 at 条目的 link tag spec
            renderer.PushTag("color", renderer.Config.AtColorHex, valuePrefix: "#")
                    .PushTag("u");
            renderer.WriteLiteral(ref obj.Content);
            renderer.PopTag(2);
        }
    }
}