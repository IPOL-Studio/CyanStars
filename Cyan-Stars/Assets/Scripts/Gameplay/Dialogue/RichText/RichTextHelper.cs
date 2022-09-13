using System.Collections.Generic;

namespace CyanStars.Gameplay.Dialogue
{
    public static class RichTextHelper
    {
        private static readonly HashSet<string> SupportedAttributes = new HashSet<string>
        {
            "b",  // bold
            "i",  // italic
            "u",  // underline
            "s",  // strikethrough
            "sub",
            "sup",
            "color"
        };

        private static readonly Dictionary<string, IRichTextAttributeValueParser> AttributeValueParserDict =
            new Dictionary<string, IRichTextAttributeValueParser>
            {
                { "color", new ColorAttributeParser() },
                { "u", new TextOverlayLineAttributeParser("u", RichTextAttributeValueKeys.UnderlineColor) },
                { "s", new TextOverlayLineAttributeParser("s", RichTextAttributeValueKeys.StrikethroughColor) }
            };

        public const string NoParseLeftAttr = "<noparse>";
        public const string NoParseRightAttr = "</noparse>";

        public static bool IsAttributeSupported(string attr) => SupportedAttributes.Contains(attr);

        public static bool TryGetAttributeValueParser(string attr, out IRichTextAttributeValueParser parser) =>
            AttributeValueParserDict.TryGetValue(attr, out parser);
    }
}
