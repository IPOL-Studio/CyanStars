using CyanStars.MarkdownRenderer.Renderers;
using CyanStars.MarkdownRenderer.Renderers.TextMeshPro.Inlines;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer.Parsers
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

    public sealed class AtParagraphInlineParser : InlineParser
    {
        public AtParagraphInlineParser()
        {
            OpeningCharacters = new[] { '[' };
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            var start = slice.Start;
            if (slice.CurrentChar != '[' || slice.PeekChar() != '@')
            {
                return false;
            }

            var closingBracketIndex = slice.Text.IndexOf(']', start + 2, slice.End - start - 1);
            if (closingBracketIndex < 0 || closingBracketIndex == start + 2 ||
                (closingBracketIndex < slice.End && (slice.Text[closingBracketIndex + 1] == '(' || slice.Text[closingBracketIndex + 1] == '[')))
            {
                return false;
            }

            var paragraph = slice.Text.Substring(start + 2, closingBracketIndex - start - 2);
            var sourceStart = processor.GetSourcePosition(start, out var line, out var column);
            var sourceEnd = processor.GetSourcePosition(closingBracketIndex);
            var labelStart = processor.GetSourcePosition(start + 1);

            var inline = new AtParagraphInline(paragraph)
            {
                IsClosed = true,
                Span = new SourceSpan(sourceStart, sourceEnd),
                Line = line,
                Column = column,
            };
            inline.AppendChild(new LiteralInline
            {
                Content = new StringSlice(slice.Text, start + 1, closingBracketIndex - 1),
                IsClosed = true,
                Span = new SourceSpan(labelStart, sourceEnd - 1),
                Line = line,
                Column = column + 1,
            });

            slice.Start = closingBracketIndex + 1;
            processor.Inline = inline;
            return true;
        }
    }

    public sealed class AtParagraphExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (pipeline.InlineParsers.FindExact<AtParagraphInlineParser>() == null)
            {
                pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new AtParagraphInlineParser());
            }
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is TextMeshProRenderer textMeshProRenderer &&
                textMeshProRenderer.ObjectRenderers.FindExact<AtParagraphInlineRenderer>() == null &&
                !textMeshProRenderer.ObjectRenderers.InsertBefore<LinkInlineRenderer>(new AtParagraphInlineRenderer()))
            {
                textMeshProRenderer.ObjectRenderers.Add(new AtParagraphInlineRenderer());
            }
        }
    }
}