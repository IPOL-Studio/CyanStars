// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using Markdig.Renderers;
using Markdig.Syntax;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro
{
    public class HtmlBlockRenderer : MarkdownObjectRenderer<TextMeshProRenderer, HtmlBlock>
    {
        protected override void Write(TextMeshProRenderer renderer, HtmlBlock obj)
        {
            var slices = obj.Lines.Lines;
            if (!(slices is null))
            {
                for (int i = 0; i < slices.Length; i++)
                {
                    ref var slice = ref slices[i].Slice;
                    if (slice.Text is null)
                    {
                        break;
                    }

                    renderer.Write(slice.AsSpan());
                    renderer.WriteLine();
                }
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
