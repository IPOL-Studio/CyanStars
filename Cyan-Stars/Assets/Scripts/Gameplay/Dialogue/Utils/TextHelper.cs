using System;
using System.Collections.Generic;
using System.Text;
using CyanStars.Framework.Utils;

namespace CyanStars.Gameplay.Dialogue
{
    public static class TextHelper
    {
        private static readonly HashSet<char> NeedSkipChars = new HashSet<char>
        {
            '\n'
        };

        private static readonly HashSet<string> SupportedAttributes = new HashSet<string>
        {
            "b",  // bold
            "i",  // italic
            "u",  // underline
            "s",  // strikethrough
            "color"
        };

        private static readonly HashSet<string> RequireValueAttributes = new HashSet<string>
        {
            "color"
        };

        public const string NoParseLeftAttr = "<noparse>";
        public const string NoParseRightAttr = "</noparse>";

        public static bool IsAttributeSupported(string attr) => SupportedAttributes.Contains(attr);

        public static bool IsAttributeRequireValue(string attr) => RequireValueAttributes.Contains(attr);

        public static bool CombineRequireValueLeftAttribute(string attr, string value, StringBuilder sb)
        {
            if (string.IsNullOrEmpty(attr) || string.IsNullOrWhiteSpace(attr))
            {
                return false;
            }

            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            sb.AppendFormat("<{0}=\"{1}\">", attr, value);
            return true;
        }

        public static bool IsSkipChar(char c)
        {
            return NeedSkipChars.Contains(c);
        }
    }
}
