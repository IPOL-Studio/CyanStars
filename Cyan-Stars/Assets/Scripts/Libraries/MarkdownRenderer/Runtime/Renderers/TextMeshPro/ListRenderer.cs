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

            var compact = renderer.IsCompactParagraph;
            renderer.IsCompactParagraph = !obj.IsLoose;

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
                renderer.IsCompactParagraph = compact;
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
            renderer.PushTag("indent", (depth - 1).ToString(), valueSuffix: "em")
                    .Write(marker)
                    .TryPopTag(out _);
            renderer.PushTag("indent", (depth + contentIndentOffset).ToString(), valueSuffix: "em");
            
            bool isClosed = false;

            foreach (var block in item)
            {
                if (block is ListBlock or QuoteBlock)
                {
                    if (!isClosed)
                    {
                        renderer.TryPopTag(out _);
                        isClosed = true;
                    }

                    renderer.EnsureLine();
                    renderer.Write(block);
                    continue;
                }

                renderer.Write(block);
                renderer.TryPopTag(out _);
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
