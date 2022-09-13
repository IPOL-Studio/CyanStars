using System.Runtime.CompilerServices;
using System.Text;
using CyanStars.Framework;
using Unity.Collections;

namespace CyanStars.Gameplay.Dialogue
{
    public class RichText
    {
        public string LeftAttributes { get; }
        public string RightAttributes { get; }
        public string Text { get; }

        private RichText(string text) : this(text, string.Empty, string.Empty)
        {
        }

        private RichText(string text, string leftAttributes, string rightAttributes)
        {
            Text = text;
            LeftAttributes = leftAttributes;
            RightAttributes = rightAttributes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static RichText FromText(string text, bool isNoParseText)
        {
            return isNoParseText
                ? new RichText(text, RichTextHelper.NoParseLeftAttr, RichTextHelper.NoParseRightAttr)
                : new RichText(text);
        }

        public static RichText FromRichTextData(RichTextData data)
        {
            bool isNoParseText = data.Text.IndexOf('<') >= 0;

            if ((data.Attributes?.Count ?? 0) <= 0)
            {
                return FromText(data.Text, isNoParseText);
            }

            var cache = GameRoot.GetDataModule<DialogueModule>().StringBuilderCache;
            var sb = cache.Get();
            var flags = new NativeArray<bool>(data.Attributes.Count, Allocator.Temp);
            RichText result;

            if (ParseLeftAttributes(data, sb, flags, isNoParseText))
            {
                var leftAttributes = sb.ToString();
                sb.Clear();

                ParseRightAttributes(data, sb, flags, isNoParseText);
                var rightAttributes = sb.ToString();

                result = new RichText(data.Text, leftAttributes, rightAttributes);
            }
            else
            {
                result = FromText(data.Text, isNoParseText);
            }

            cache.Release(sb);
            flags.Dispose();

            return result;
        }

        private static bool ParseLeftAttributes(RichTextData data, StringBuilder sb, NativeArray<bool> flags, bool isNoParseText)
        {
            bool isAppended = false;

            for (int i = 0; i < data.Attributes.Count; i++)
            {
                string attr = data.Attributes[i];
                if (!RichTextHelper.IsAttributeSupported(attr))
                    continue;

                if (RichTextHelper.TryGetAttributeValueParser(attr, out var parser))
                {
                    if (ParseLeftAttributeWithParser(data, attr, sb, parser))
                    {
                        flags[i] = true;
                        isAppended = true;
                    }
                }
                else
                {
                    flags[i] = true;
                    isAppended = true;
                    sb.Append('<').Append(attr).Append('>');
                }
            }

            sb.Append(isNoParseText && isAppended ? RichTextHelper.NoParseLeftAttr : string.Empty);
            return isAppended;
        }

        private static void ParseRightAttributes(RichTextData data, StringBuilder sb, NativeArray<bool> flags, bool isNoParseText)
        {
            sb.Append(isNoParseText ? RichTextHelper.NoParseRightAttr : string.Empty);
            for (int i = data.Attributes.Count - 1; i >= 0; i--)
            {
                if (flags[i])
                {
                    sb.Append("</").Append(data.Attributes[i]).Append('>');
                }
            }
        }

        private static bool ParseLeftAttributeWithParser(RichTextData data, string attr, StringBuilder sb, IRichTextAttributeValueParser parser)
        {
            bool hasAttrValues = (data.AttributeValues?.Count ?? 0) > 0;

            if (parser.ParseOption == RichTextAttributeValueParseOptionType.Required)
            {
                if (hasAttrValues && parser.AppendAttributeTo(sb, data.AttributeValues))
                {
                    return true;
                }
            }
            else
            {
                if (hasAttrValues)
                {
                    return parser.AppendAttributeTo(sb, data.AttributeValues);
                }
                sb.Append('<').Append(attr).Append('>');
                return true;
            }

            return false;
        }
    }
}
