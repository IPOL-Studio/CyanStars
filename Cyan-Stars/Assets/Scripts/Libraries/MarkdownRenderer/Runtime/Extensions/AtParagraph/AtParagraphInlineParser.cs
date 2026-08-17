#nullable enable

using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace CyanStars.MarkdownRenderer.Extensions.AtParagraph
{
    public sealed class AtParagraphInlineParser : InlineParser
    {
        public AtParagraphInlineParser()
        {
            OpeningCharacters = new[] { '[' };
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            var copied = slice;
            if (InternalMatch(processor, ref copied))
            {
                slice = copied;
                return true;
            }
            return false;
        }

        private bool InternalMatch(InlineProcessor processor, ref StringSlice slice)
        {
            int openingBracketIndex = slice.Start;

            if (slice.PeekCharExtra(1) != '@' ||
                !LinkHelper.TryParseLabel(ref slice, out string? label, out SourceSpan labelSpan) ||
                label.Length <= 1)
            {
                return false;
            }

            var saved = slice;
            if (slice.CurrentChar == '(' &&
                LinkHelper.TryParseInlineLink(ref slice, out _, out _, out _, out _))
            {
                return false;
            }

            slice = saved;
            if (IsStandardReferenceLink(processor, ref slice, label))
            {
                return false;
            }

            slice = saved;
            processor.GetSourcePosition(openingBracketIndex, out int line, out int column);
            int endPosition = slice.Start - 1;

            var inline = new AtParagraphInline
            {
                Label = label,
                Paragraph = label.Substring(1),
                Span = new SourceSpan(
                    processor.GetSourcePosition(openingBracketIndex),
                    processor.GetSourcePosition(endPosition)),
                Line = line,
                Column = column,
                IsClosed = true,
            };

            inline.AppendChild(new LiteralInline
            {
                Content = new StringSlice(label),
                Span = new SourceSpan(
                    processor.GetSourcePosition(labelSpan.Start),
                    processor.GetSourcePosition(labelSpan.End)),
                Line = line,
                Column = column + 1,
                IsClosed = true,
            });

            processor.Inline = inline;
            return true;
        }

        private static bool IsStandardReferenceLink(InlineProcessor processor, ref StringSlice slice, string label)
        {
            if (slice.CurrentChar != '[')
            {
                return processor.Document.ContainsLinkReferenceDefinition(label);
            }

            var referenceSlice = slice;
            if (!LinkHelper.TryParseLabel(ref referenceSlice, true, out string? referenceLabel, out _))
            {
                return processor.Document.ContainsLinkReferenceDefinition(label);
            }

            if (string.IsNullOrEmpty(referenceLabel))
            {
                return processor.Document.ContainsLinkReferenceDefinition(label);
            }

            if (processor.Document.ContainsLinkReferenceDefinition(referenceLabel))
            {
                return true;
            }

            return referenceLabel[0] != '@';
        }
    }
}
