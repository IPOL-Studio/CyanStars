#nullable enable

using CyanStars.MarkdownRenderer.Utils;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer.Renderers.TextMeshPro
{
    public class QuoteBlockRenderer : MarkdownObjectRenderer<TextMeshProRenderer, QuoteBlock>
    {
        protected override void Write(TextMeshProRenderer renderer, QuoteBlock obj)
        {
            renderer.EnsureLine();
            bool isFirstQuote = renderer.QuoteLevel == 0;
            int contentNestingLevel = renderer.NestingLevel + renderer.QuoteLevel;

            renderer.QuoteLevel++;

            try
            {

                if (isFirstQuote)
                {
                    // if (renderer.NestingLevel > 0)
                    // {
                    //     // note: 当前只有 list 会修改全局 markdown 嵌套层级
                    //     // 可以认定当前 quote block 是 list 的子项
                    //     // 直接先给一点 Spacing 防止两个块元素靠得太近
                    //     renderer.EnsureSpacing("6");
                    // }

                    renderer.PushTag("color", renderer.Config.QuoteColorHex, valuePrefix: "#");
                }

                var contentIndentValue = renderer.GetIndentValue(contentNestingLevel) + renderer.Config.QuoteBlockMargin;
                var contentIndent = contentIndentValue == 0 ? null : TextMeshProFormatUtils.FormatNumber(contentIndentValue);

                for (int i = 0; i < obj.Count; i++)
                {
                    if (i > 0)
                    {
                        renderer.EnsureLine();
                    }

                    Block block = obj[i];

                    if (block is QuoteBlock innerQuoteBlock)
                    {
                        Write(renderer, innerQuoteBlock);
                    }
                    else
                    {
                        if (contentIndent is not null)
                        {
                            renderer.PushTag("indent", contentIndent, valueSuffix: "em")
                                    .Write(block);
                            renderer.TryPopTag(out _);
                        }
                        else
                        {
                            renderer.Write(block);
                        }
                    }

                    TryEnsureQuoteEmptyLine(renderer, obj, i);
                }

                if (isFirstQuote)
                {
                    renderer.TryPopTag(out _);
                }

                if (contentNestingLevel == 1)
                {
                    renderer.FinishBlock(true);
                }
            }
            finally
            {
                renderer.QuoteLevel--;
            }
        }

        private void TryEnsureQuoteEmptyLine(TextMeshProRenderer renderer, QuoteBlock obj, int i)
        {
            if (renderer.Config.QuoteSpacing <= 0)
            {
                return;
            }

            var block = obj[i];
            if (i >= obj.Count - 1 || block is not ParagraphBlock)
            {
                return;
            }

            Block nextBlock = obj[i + 1];
            if (nextBlock is not ParagraphBlock && nextBlock is not QuoteBlock)
            {
                return;
            }

            renderer.PushTag("line-height", renderer.QuoteSpacing, valueSuffix: "em");
            renderer.EnsureLine();

            renderer.PopTag(1);
            renderer.WriteLine();
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
