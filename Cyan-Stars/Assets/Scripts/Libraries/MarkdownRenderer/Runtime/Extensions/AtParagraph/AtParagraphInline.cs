using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer.Extensions.AtParagraph
{
    public sealed class AtParagraphInline : LinkInline
    {
        public string Paragraph {get; set;}
    }
}
