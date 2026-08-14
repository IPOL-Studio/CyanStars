using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer.Extensions.AtParagraph
{
    public sealed class AtParagraphInline : LinkInline
    {
        public AtParagraphInline(string paragraph)
        {
            Paragraph = paragraph;
            Label = "@" + paragraph;
            Url = string.Empty;
        }

        public string Paragraph { get; }
    }
}
