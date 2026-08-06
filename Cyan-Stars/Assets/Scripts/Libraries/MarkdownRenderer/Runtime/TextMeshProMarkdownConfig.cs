using System;
using UnityEngine;

namespace CyanStars.MarkdownRenderer
{
    public interface ITextMeshProMarkdownConfigProvider
    {
        TextMeshProMarkdownConfig Config { get; }
    }

    [Serializable]
    public class TextMeshProMarkdownConfig
    {
        private static TextMeshProMarkdownConfig defaultConfig;
        public static TextMeshProMarkdownConfig DefaultConfig => defaultConfig ??= CreateDefault();

        public Color CodeBlockBackgroundColor;
        public Color AtColor;
        public Color LinkColor;
        public string UnorderedListMarker;

        public string CodeBlockBackgroundColorHex => ColorUtility.ToHtmlStringRGB(CodeBlockBackgroundColor);
        public string AtColorHex => ColorUtility.ToHtmlStringRGB(AtColor);
        public string LinkColorHex => ColorUtility.ToHtmlStringRGB(LinkColor);

        public TextMeshProMarkdownConfig()
        {
        }

        public TextMeshProMarkdownConfig(TextMeshProMarkdownConfig other)
        {
            this.CodeBlockBackgroundColor = other.CodeBlockBackgroundColor;
            this.AtColor = other.AtColor;
            this.LinkColor = other.LinkColor;
            this.UnorderedListMarker = other.UnorderedListMarker;
        }


        private static Color ParseColor(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : default;
        }

        private static TextMeshProMarkdownConfig CreateDefault() => new()
        {
            CodeBlockBackgroundColor = new(r: 0.533f, g: 0.533f, b: 0.533f),
            AtColor                  = new(1, 0.841f, 0.078f, 0.87f),
            LinkColor                = new(1, 0.841f, 0.078f, 0.87f),
            UnorderedListMarker      = "\u2011"
        };
    }
}