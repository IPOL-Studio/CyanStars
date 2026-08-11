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
            bool savedSkipNextEnsureLine = renderer.SkipNextEnsureLine;

            try
            {
                int totalDepth = renderer.NestingLevel + renderer.QuoteLevel;
                bool isFirstQuote = renderer.QuoteLevel == 1;

                if (isFirstQuote)
                {
                    renderer.Write("<color=#").Write(renderer.Config.QuoteColorHex).Write(">");
                }

                renderer.Write("<indent=").Write(renderer.NestingLevel.ToString()).Write("em>");
                for (int i = 0; i < renderer.QuoteLevel; i++)
                {
                    renderer.Write(renderer.Config.QuoteMarker);
                }
                renderer.Write("</indent>");

                foreach (var block in obj)
                {
                    if (block is QuoteBlock innerQuoteBlock)
                    {
                        Write(renderer, innerQuoteBlock);
                    }
                    else
                    {
                        renderer.SkipNextEnsureLine = true;
                        renderer.Write("<indent=").Write(totalDepth.ToString()).Write("em>")
                                .Write(block);
                        renderer.Write("</indent>");
                        renderer.SkipNextEnsureLine = savedSkipNextEnsureLine;
                    }
                }

                if (isFirstQuote)
                {
                    renderer.Write("</color>");
                }

                if (totalDepth == 1)
                {
                    renderer.EnsureLine();
                }
            }
            finally
            {
                renderer.QuoteLevel--;
                renderer.SkipNextEnsureLine = savedSkipNextEnsureLine;
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
