using Markdig.Renderers;
using Markdig.Syntax;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro
{
    public class ListRenderer : MarkdownObjectRenderer<TextMeshProRenderer, ListBlock>
    {
        protected override void Write(TextMeshProRenderer renderer, ListBlock obj)
        {
            renderer.EnsureLine();
            renderer.PushNestingLevel();
            try
            {
                if (obj.IsOrdered)
                {
                    WriteOrderedList(renderer, obj);
                }
                else
                {
                    WriteUnorderedList(renderer, obj);
                }
            }
            finally
            {
                renderer.PopNestingLevel();
            }
        }

        private void WriteOrderedList(TextMeshProRenderer renderer, ListBlock obj)
        {
            int start = 1;
            if (!(obj.OrderedStart is null))
            {
                int.TryParse(obj.OrderedStart, out start);
            }

            int index = start;
            foreach (ListItemBlock item in obj)
            {
                renderer.EnsureLine();
                WriteListItem(renderer, item, $"{index++}.", 0);
            }

            if (renderer.NestingLevel == 1)
            {
                renderer.EnsureLine();
            }
        }

        private void WriteUnorderedList(TextMeshProRenderer renderer, ListBlock obj)
        {
            var unorderedListMarker = renderer.Config.UnorderedListMarker;
            foreach (ListItemBlock item in obj)
            {
                renderer.EnsureLine();
                WriteListItem(renderer, item, unorderedListMarker, -0.5);
            }

            if (renderer.NestingLevel == 1)
            {
                renderer.EnsureLine();
            }
        }

        private void WriteListItem(TextMeshProRenderer renderer, ListItemBlock item, string marker, double contentIndentOffset)
        {
            int depth = renderer.NestingLevel;
            renderer.Write("<indent=").Write((depth - 1).ToString()).Write("em>")
                    .Write(marker)
                    .Write("</indent>")
                    .Write("<indent=").Write((depth + contentIndentOffset).ToString()).Write("em>");
            
            bool isClosed = false;

            foreach (var block in item)
            {
                if (block is ListBlock or QuoteBlock)
                {
                    if (!isClosed)
                    {
                        renderer.Write("</indent>");
                        isClosed = true;
                    }

                    renderer.EnsureLine();
                    renderer.Write(block);
                    continue;
                }

                renderer.SkipNextEnsureLine = true;
                renderer.Write(block);
                renderer.Write("</indent>");
                isClosed = true;
            }

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
