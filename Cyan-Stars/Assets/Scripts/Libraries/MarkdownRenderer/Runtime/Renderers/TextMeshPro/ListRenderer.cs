// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using Markdig.Renderers;
using Markdig.Syntax;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro
{
    public class ListRenderer : MarkdownObjectRenderer<TextMeshProRenderer, ListBlock>
    {
        private int nestingLevel;
        private const string CloseTag = "</indent>";

        protected override void Write(TextMeshProRenderer renderer, ListBlock obj)
        {
            renderer.EnsureLine();
            nestingLevel++;
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
                nestingLevel--;
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

            if (nestingLevel == 1)
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

            if (nestingLevel == 1)
            {
                renderer.EnsureLine();
            }
        }

        private void WriteListItem(TextMeshProRenderer renderer, ListItemBlock item, string marker, double contentIndentOffset)
        {
            renderer.Write("<indent=").Write((nestingLevel - 1).ToString()).Write("em>")
                    .Write(marker)
                    .Write("</indent>")
                    .Write("<indent=").Write((nestingLevel + contentIndentOffset).ToString()).Write("em>");
            bool isIndentOpen = true;

            foreach (var block in item)
            {
                if (block is ListBlock list)
                {
                    renderer.Write(CloseTag);
                    isIndentOpen = false;
                    renderer.EnsureLine();
                    renderer.Write(list);
                    continue;
                }

                renderer.SkipNextEnsureLine = true;
                renderer.Write(block);
            }

            if (isIndentOpen)
            {
                renderer.Write(CloseTag);
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
