#nullable enable

using CyanStars.MarkdownRenderer.Utils;
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

                if (renderer.NestingLevel == 1)
                {
                    renderer.FinishBlock(true);
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
            var (markerIndent, contentIndent) = GetListItemIndents(renderer, 1f);
            foreach (ListItemBlock item in obj)
            {
                renderer.EnsureLine();
                WriteListItem(renderer, item, $"{index++}.", markerIndent, contentIndent);
            }
        }

        private void WriteUnorderedList(TextMeshProRenderer renderer, ListBlock obj)
        {
            var unorderedListMarker = renderer.Config.UnorderedListMarker;
            var (markerIndent, contentIndent) = GetListItemIndents(renderer, renderer.Config.UnorderedListMarkerWidth);

            foreach (ListItemBlock item in obj)
            {
                renderer.EnsureLine();
                WriteListItem(renderer, item, unorderedListMarker, markerIndent, contentIndent);
            }
        }

        private void WriteListItem(TextMeshProRenderer renderer, ListItemBlock item, string marker, string? markerIndent, string? contentIndent)
        {
            if (markerIndent is not null)
            {
                renderer.PushTag("indent", markerIndent, valueSuffix: "em")
                        .Write(marker)
                        .TryPopTag(out _);
            }
            else
            {
                renderer.Write(marker);
            }

            if (contentIndent is not null)
            {
                renderer.PushTag("indent", contentIndent, valueSuffix: "em");
            }

            
            bool isClosed = false;

            foreach (var block in item)
            {
                if (block is ListBlock or QuoteBlock)
                {
                    if (!isClosed)
                    {
                        if (contentIndent is not null)
                        {
                            renderer.TryPopTag(out _);
                        }
                        isClosed = true;
                    }

                    renderer.EnsureLine();
                    renderer.Write(block);
                    continue;
                }

                renderer.Write(block);
                if (contentIndent is not null)
                {
                    renderer.TryPopTag(out _);
                }
                isClosed = true;
            }
        }

        private (string? markerIndent, string? contentIndent) GetListItemIndents(TextMeshProRenderer renderer, float contentIndentOffset)
        {
            var markerIndentValue = renderer.GetIndentValue(renderer.NestingLevel - 1);
            var contentIndentValue = markerIndentValue + contentIndentOffset;
            var markerIndent = markerIndentValue <= 0 ? null : TextMeshProFormatUtils.FormatNumber(markerIndentValue);
            var contentIndent = contentIndentValue <= 0 ? null : TextMeshProFormatUtils.FormatNumber(contentIndentValue);
            return (markerIndent, contentIndent);
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
