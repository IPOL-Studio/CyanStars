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

            renderer.QuoteLevel++;

            try
            {
                int contentNestingLevel = renderer.NestingLevel + renderer.QuoteLevel;
                bool isFirstQuote = renderer.QuoteLevel == 1;

                if (isFirstQuote)
                {
                    if (renderer.NestingLevel > 0)
                    {
                        // note: 当前只有 list 会修改全局 markdown 嵌套层级
                        // 可以认定当前 quote block 是 list 的子项
                        // 直接先给一点 Spacing 防止两个块元素靠得太近
                        renderer.EnsureSpacing("6");
                    }

                    renderer.PushTag("color", renderer.Config.QuoteColorHex, valuePrefix: "#");
                }

                var contentIndent = TextMeshProFormatUtils.FormatNumber(renderer.GetIndentValue(contentNestingLevel));

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
                        WriteQuoteMarker(renderer, renderer.QuoteLevel, renderer.NestingLevel, false);

                        if (block is ParagraphBlock paragraph)
                        {
                            // 引用块内的段落：内部换行时在新的一行重复输出 mark
                            // 基本上复制了 paragraph block inlines 的处理逻辑
                            // 如果 paragraph renderer 有更改，记得检查这里
                            WriteParagraphInlines(renderer, paragraph, renderer.NestingLevel, contentIndent);
                        }
                        else
                        {
                            renderer.PushTag("indent", contentIndent, valueSuffix: "em")
                                    .Write(block);
                            renderer.TryPopTag(out _);
                        }
                    }

                    TryEnsureQuoteEmptyLine(renderer, obj, renderer.NestingLevel, i);
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

        private void TryEnsureQuoteEmptyLine(TextMeshProRenderer renderer, QuoteBlock obj,
                                             int lineNestingLevel, int i)
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

            // 如果渲染空白的引用块(相邻引用块的间距部分)
            // 就往空引用块的同一行输出一个透明字符
            // 确保 TMP 不会丢弃通过空格渲染的引用块 mark 部分
            if (renderer.Config.QuoteWidth > 0)
            {
                WriteQuoteMarker(renderer, renderer.QuoteLevel, lineNestingLevel, true);
                renderer.PushTag("color", "#00000000");
                renderer.Write('*');
                renderer.PopTag(1);
            }

            renderer.PopTag(1);
            renderer.WriteLine();
        }

        private void WriteQuoteMarker(TextMeshProRenderer renderer, int count, int lineNestingLevel, bool isBlankLine)
        {
// <indent={quoteLevel*indent-width/2}em><mspace={width}em><mark={color}> </mark></mspace></indent>

            if (renderer.Config.QuoteWidth <= 0)
            {
                return;
            }

            bool isOverrideSize = isBlankLine && renderer.Config.QuoteSpacing > 1;
            var nestingIndent = renderer.Config.NestingIndent;

            for (int i = 0; i < count; i++)
            {
                var indent = i * nestingIndent - renderer.HalfQuoteMarkerWidth + renderer.GetIndentValue(lineNestingLevel);
                renderer.PushTag("indent", TextMeshProFormatUtils.FormatNumber(indent), valueSuffix: "em")
                        .PushTag("mspace", TextMeshProFormatUtils.FormatNumber(renderer.QuoteMarkerWidth), valueSuffix: "em");

                if (isOverrideSize)
                {
                    renderer.PushTag("size", renderer.QuoteSpacing, valueSuffix: "em");
                }

                renderer.PushTag("mark", renderer.Config.QuoteColorHex, valuePrefix: "#")
                        .WriteRaw(' ');
                renderer.PopTag(isOverrideSize ? 4 : 3);

                if (i < count - 1)
                {
                    renderer.WriteRaw(' ');
                }
            }
        }

        private void WriteParagraphInlines(TextMeshProRenderer renderer, ParagraphBlock paragraph,
                                           int lineNestingLevel, string contentIndent)
        {
            renderer.PushTag("indent", contentIndent, valueSuffix: "em");

            var root = paragraph.Inline;
            if (root != null)
            {
                for (Inline child = root.FirstChild; child != null; child = child.NextSibling)
                {
                    if (child is LineBreakInline lineBreak)
                    {
                        if (child.NextSibling == null)
                        {
                            continue;
                        }

                        if (!lineBreak.IsHard)
                        {
                            renderer.WriteRaw(' ');
                            continue;
                        }

                        // 硬换行：新的一行重复输出 mark
                        renderer.TryPopTag(out _);
                        renderer.WriteLine();
                        WriteQuoteMarker(renderer, renderer.QuoteLevel, lineNestingLevel, false);
                        renderer.PushTag("indent", contentIndent, valueSuffix: "em");
                    }
                    else
                    {
                        renderer.Write(child);
                    }
                }
            }

            renderer.TryPopTag(out _);
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
