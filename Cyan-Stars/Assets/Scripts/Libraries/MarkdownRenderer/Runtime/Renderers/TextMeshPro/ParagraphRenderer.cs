// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using Markdig.Renderers;
using Markdig.Syntax;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro
{
    public class ParagraphRenderer : MarkdownObjectRenderer<TextMeshProRenderer, ParagraphBlock>
    {
        protected override void Write(TextMeshProRenderer renderer, ParagraphBlock obj)
        {
            renderer.WriteLeafInline(obj);
            renderer.TryEnsureLineIfNotSkip(true);
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
