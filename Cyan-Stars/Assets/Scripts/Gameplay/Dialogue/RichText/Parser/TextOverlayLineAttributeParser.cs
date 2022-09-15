using System.Collections.Generic;
using System.Text;
using CyanStars.Framework.Utils;

namespace CyanStars.Gameplay.Dialogue
{
    public class TextOverlayLineAttributeParser : IRichTextAttributeValueParser
    {
        private string attr;
        private string colorKey;

        public RichTextAttributeValueParseOptionType ParseOption => RichTextAttributeValueParseOptionType.Optional;

        public TextOverlayLineAttributeParser(string attr, string colorKey)
        {
            this.attr = attr;
            this.colorKey = colorKey;
        }

        public bool AppendAttributeTo(StringBuilder sb, Dictionary<string, string> attrValues)
        {
            if (attrValues.TryGetValue(colorKey, out var color) &&
                ColorHelper.TryParseHtmlString(color, out var colorHex))
            {
                sb.AppendFormat("<{0} color={1}>", attr, colorHex);
                return true;
            }

            sb.Append('<').Append(attr).Append('>');
            return true;
        }
    }
}
