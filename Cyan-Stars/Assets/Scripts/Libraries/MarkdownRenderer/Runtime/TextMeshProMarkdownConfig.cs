using System;
using UnityEngine;

namespace CyanStars.MarkdownRenderer
{
    public interface ITextMeshProMarkdownConfigProvider
    {
        TextMeshProMarkdownConfig Config { get; }
    }

    [Serializable]
    public class TextMeshProMarkdownConfig : IEquatable<TextMeshProMarkdownConfig>
    {
        private static TextMeshProMarkdownConfig defaultConfig;
        public static TextMeshProMarkdownConfig DefaultConfig => defaultConfig ??= CreateDefault();

        public Color CodeBlockBackgroundColor;
        public Color AtColor;
        public Color LinkColor;
        public Color QuoteColor;
        public string LinkPrefix;
        public string UnorderedListMarker;
        [Range(0,  2)] public double QuoteWidth;
        [Range(0, 10)] public double QuoteSpacing;
        [Range(0, 10)] public double BlockFakeMarginBottom;

        public string CodeBlockBackgroundColorHex => ColorUtility.ToHtmlStringRGBA(CodeBlockBackgroundColor);
        public string AtColorHex => ColorUtility.ToHtmlStringRGBA(AtColor);
        public string LinkColorHex => ColorUtility.ToHtmlStringRGBA(LinkColor);
        public string QuoteColorHex => ColorUtility.ToHtmlStringRGBA(QuoteColor);

        public TextMeshProMarkdownConfig()
        {
        }

        public TextMeshProMarkdownConfig(TextMeshProMarkdownConfig other)
        {
            this.CodeBlockBackgroundColor = other.CodeBlockBackgroundColor;
            this.AtColor = other.AtColor;
            this.LinkColor = other.LinkColor;
            this.QuoteColor = other.QuoteColor;
            this.LinkPrefix = other.LinkPrefix;
            this.UnorderedListMarker = other.UnorderedListMarker;
            this.QuoteWidth = other.QuoteWidth;
            this.QuoteSpacing = other.QuoteSpacing;
            this.BlockFakeMarginBottom = other.BlockFakeMarginBottom;
        }

        private static TextMeshProMarkdownConfig CreateDefault() => new()
        {
            CodeBlockBackgroundColor = new(0.533f, 0.533f, 0.533f, 0.5f),
            AtColor                  = new(1, 0.841f, 0.078f, 0.87f),
            LinkColor                = new(1, 0.841f, 0.078f, 0.87f),
            QuoteColor               = new(0.6f, 0.6f, 0.6f),
            LinkPrefix               = "__md_link__",
            UnorderedListMarker      = "\u2011",
            QuoteWidth               = 1,
            QuoteSpacing             = 0.5,
            BlockFakeMarginBottom    = 0.5,
        };

        public bool Equals(TextMeshProMarkdownConfig other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;

            return CodeBlockBackgroundColor == other.CodeBlockBackgroundColor
                && AtColor == other.AtColor
                && LinkColor == other.LinkColor
                && QuoteColor == other.QuoteColor
                && LinkPrefix == other.LinkPrefix
                && UnorderedListMarker == other.UnorderedListMarker
                && QuoteWidth == other.QuoteWidth
                && QuoteSpacing == other.QuoteSpacing
                && BlockFakeMarginBottom == other.BlockFakeMarginBottom;
        }
    }
}