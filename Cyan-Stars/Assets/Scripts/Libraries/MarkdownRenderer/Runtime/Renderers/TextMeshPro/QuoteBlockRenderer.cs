using Markdig.Renderers;
using Markdig.Syntax;

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
                int totalDepth = renderer.NestingLevel + renderer.QuoteLevel;
                bool isFirstQuote = renderer.QuoteLevel == 1;

                if (isFirstQuote)
                {
                    renderer.PushTag("color", renderer.Config.QuoteColorHex, valuePrefix: "#");
                }

                renderer.PushTag("indent", renderer.NestingLevel.ToString(), valueSuffix: "em");
                for (int i = 0; i < renderer.QuoteLevel; i++)
                {
                    renderer.Write(renderer.Config.QuoteMarker);
                }
                renderer.TryPopTag(out _);

                foreach (var block in obj)
                {
                    if (block is QuoteBlock innerQuoteBlock)
                    {
                        Write(renderer, innerQuoteBlock);
                    }
                    else
                    {
                        renderer.PushTag("indent", totalDepth.ToString(), valueSuffix: "em")
                                .Write(block);
                        renderer.TryPopTag(out _);
                    }
                }

                if (isFirstQuote)
                {
                    renderer.TryPopTag(out _);
                }

                if (totalDepth == 1)
                {
                    renderer.EnsureLine();
                }
            }
            finally
            {
                renderer.QuoteLevel--;
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
