// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro
{
    public class HeadingRenderer : MarkdownObjectRenderer<TextMeshProRenderer, HeadingBlock>
    {
        private static readonly string[] HeadingTextSizes = new string[]
        {
            "200%",
            "150%",
            "117%",
            "100%",
            "83%",
            "67%",
        };

        protected override void Write(TextMeshProRenderer renderer, HeadingBlock obj)
        {
            int index = obj.Level - 1;

            if ((uint)index >= (uint)HeadingTextSizes.Length)
            {
                renderer.WriteLeafInline(obj);
            }
            else
            {
                renderer.PushTag("size", HeadingTextSizes[index])
                        .WriteLeafInline(obj)
                        .TryPopTag(out _);
            }

            renderer.FinishBlock(true);
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