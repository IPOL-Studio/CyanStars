using System;
using UnityEngine;

namespace CyanStars.MarkdownRenderer
{
    [Serializable]
    public class TextMeshProRenderConfig : IEquatable<TextMeshProRenderConfig>
    {
        private static TextMeshProRenderConfig defaultConfig;
        public static TextMeshProRenderConfig DefaultConfig => defaultConfig ??= CreateDefault();

        public Color CodeBlockBackgroundColor;
        public Color AtColor;
        public Color LinkColor;
        public Color QuoteColor;
        public string LinkPrefix;
        public string UnorderedListMarker;
        public float UnorderedListMarkerWidth;
        [Range(0,  4)] public float NestingIndent;
        [Range(0, 10)] public float QuoteBlockMargin;
        [Range(0, 10)] public float QuoteSpacing;
        [Range(0, 10)] public float BlockFakeMarginBottom;
        public FinishBlockBehavior FinishBlockBehavior;

        public string CodeBlockBackgroundColorHex => ColorUtility.ToHtmlStringRGBA(CodeBlockBackgroundColor);
        public string AtColorHex => ColorUtility.ToHtmlStringRGBA(AtColor);
        public string LinkColorHex => ColorUtility.ToHtmlStringRGBA(LinkColor);
        public string QuoteColorHex => ColorUtility.ToHtmlStringRGBA(QuoteColor);

        public TextMeshProRenderConfig()
        {
        }

        public TextMeshProRenderConfig(TextMeshProRenderConfig other)
        {
            this.CodeBlockBackgroundColor = other.CodeBlockBackgroundColor;
            this.AtColor = other.AtColor;
            this.LinkColor = other.LinkColor;
            this.QuoteColor = other.QuoteColor;
            this.LinkPrefix = other.LinkPrefix;
            this.UnorderedListMarker = other.UnorderedListMarker;
            this.UnorderedListMarkerWidth = other.UnorderedListMarkerWidth;
            this.NestingIndent = other.NestingIndent;
            this.QuoteBlockMargin = other.QuoteBlockMargin;
            this.QuoteSpacing = other.QuoteSpacing;
            this.BlockFakeMarginBottom = other.BlockFakeMarginBottom;
            this.FinishBlockBehavior = other.FinishBlockBehavior;
        }

        private static TextMeshProRenderConfig CreateDefault() => new()
        {
            CodeBlockBackgroundColor = new(0.533f, 0.533f, 0.533f, 0.87f),
            AtColor                  = new(1, 0.841f, 0.078f, 0.87f),
            LinkColor                = new(1, 0.841f, 0.078f, 0.87f),
            QuoteColor               = new(0.6f, 0.6f, 0.6f, 0.87f),
            LinkPrefix               = "__md_link__",
            UnorderedListMarker      = "\u2011",
            UnorderedListMarkerWidth = 0.5f,
            NestingIndent            = 1,
            QuoteBlockMargin         = 0,
            QuoteSpacing             = 0.5f,
            BlockFakeMarginBottom    = 0.5f,
            FinishBlockBehavior      = FinishBlockBehavior.FakeMargin
        };

        public bool Equals(TextMeshProRenderConfig other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;

            return CodeBlockBackgroundColor == other.CodeBlockBackgroundColor
                && AtColor == other.AtColor
                && LinkColor == other.LinkColor
                && QuoteColor == other.QuoteColor
                && LinkPrefix == other.LinkPrefix
                && UnorderedListMarker == other.UnorderedListMarker
                && Mathf.Approximately(UnorderedListMarkerWidth, other.UnorderedListMarkerWidth)
                && Mathf.Approximately(NestingIndent, other.NestingIndent)
                && Mathf.Approximately(QuoteBlockMargin, other.QuoteBlockMargin)
                && Mathf.Approximately(QuoteSpacing, other.QuoteSpacing)
                && Mathf.Approximately(BlockFakeMarginBottom, other.BlockFakeMarginBottom)
                && FinishBlockBehavior == other.FinishBlockBehavior;
        }
    }

    public enum FinishBlockBehavior
    {
        None,
        FakeMargin,
        EmptyLine
    }
}
