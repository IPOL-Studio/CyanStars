using System.Collections.Generic;
using System.Text;

namespace CyanStars.Gameplay.Dialogue.Parser
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
            if (attrValues.TryGetValue(colorKey, out var color)&&
                !string.IsNullOrEmpty(color) && !string.IsNullOrWhiteSpace(color))
            {
                sb.AppendFormat("<{0} color={1}>", attr, color);
                return true;
            }

            sb.Append('<').Append(attr).Append('>');
            return true;
        }
    }
}
