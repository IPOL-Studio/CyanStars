using System;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines
{
    public class CodeInlineRenderer : MarkdownObjectRenderer<TextMeshProRenderer, CodeInline>
    {
        protected override void Write(TextMeshProRenderer renderer, CodeInline obj)
        {
            renderer.PushTag("mark", renderer.Config.CodeBlockBackgroundColorHex, valuePrefix: "#")
                    .PushTag("noparse");
            WriteContent(renderer, obj.Content.AsSpan());
            renderer.TryPopTag(out _);
            renderer.TryPopTag(out _);
        }

        private void WriteContent(TextMeshProRenderer renderer, ReadOnlySpan<char> content)
        {
            if (content.IsEmpty)
            {
                return;
            }

            foreach (var c in content)
            {
                if (c == '\\')
                {
                    renderer.WriteRaw(@"\\");
                }
                else if (c == '\n')
                {
                    renderer.Write(c);
                }
                else
                {
                    renderer.WriteRaw(c);
                }
            }
        }
    }
}
