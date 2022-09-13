using System.Collections.Generic;
using System.Text;

namespace CyanStars.Gameplay.Dialogue.Parser
{
    public class ColorAttributeParser : IRichTextAttributeValueParser
    {
        public RichTextAttributeValueParseOptionType ParseOption => RichTextAttributeValueParseOptionType.Required;

        public bool AppendAttributeTo(StringBuilder sb, Dictionary<string, string> attrValues)
        {
            if (attrValues.TryGetValue(RichTextAttributeValueKeys.Color, out var value) &&
                !string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value))
            {
                sb.AppendFormat("<color={0}>", value);
                return true;
            }

            return false;
        }
    }
}
