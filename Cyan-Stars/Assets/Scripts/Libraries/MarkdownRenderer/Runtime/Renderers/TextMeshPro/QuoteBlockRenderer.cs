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
                    renderer.PushTag("color", renderer.Config.QuoteColorHex, valuePrefix: "#");
                }

                var lineNestingLevelString = renderer.NestingLevel.ToString();

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
                        renderer.PushTag("indent", lineNestingLevelString, valueSuffix: "em");
                        WriteQuoteMarker(renderer, renderer.QuoteLevel);
                        renderer.TryPopTag(out _);

                        if (block is ParagraphBlock paragraph)
                        {
                            // 引用块内的段落：内部换行时在新的一行重复输出 mark
                            // 基本上复制了 paragraph block inlines 的处理逻辑
                            // 如果 paragraph renderer 有更改，记得检查这里
                            WriteParagraphInlines(renderer, paragraph, lineNestingLevelString, contentNestingLevel);
                        }
                        else
                        {
                            renderer.PushTag("indent", contentNestingLevel.ToString(), valueSuffix: "em")
                                    .Write(block);
                            renderer.TryPopTag(out _);
                        }
                    }

                    TryEnsureQuoteEmptyLine(renderer, obj, lineNestingLevelString, i);
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
                                             string lineNestingLevelString, int i)
        {
            var block = obj[i];
            if (i >= obj.Count - 1 || block is not ParagraphBlock || obj[i + 1] is not ParagraphBlock)
            {
                return;
            }

            renderer.EnsureLine();
            renderer.PushTag("indent", lineNestingLevelString, valueSuffix: "em");
            WriteQuoteMarker(renderer, renderer.QuoteLevel);
            renderer.TryPopTag(out _);
            renderer.EnsureLine();
        }

        private void WriteQuoteMarker(TextMeshProRenderer renderer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                renderer.Write(renderer.Config.QuoteMarker);
            }
        }

        private void WriteParagraphInlines(TextMeshProRenderer renderer, ParagraphBlock paragraph,
                                           string lineNestingLevelString, int contentNestingLevel)
        {
            renderer.PushTag("indent", contentNestingLevel.ToString(), valueSuffix: "em");

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
                        renderer.PushTag("indent", lineNestingLevelString, valueSuffix: "em");
                        WriteQuoteMarker(renderer, renderer.QuoteLevel);
                        renderer.TryPopTag(out _);
                        renderer.PushTag("indent", contentNestingLevel.ToString(), valueSuffix: "em");
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
