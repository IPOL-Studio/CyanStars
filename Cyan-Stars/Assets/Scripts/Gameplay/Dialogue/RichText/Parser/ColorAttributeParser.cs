using System.Collections.Generic;
using System.Text;
using CyanStars.Framework.Utils;

namespace CyanStars.Gameplay.Dialogue
{
    public class ColorAttributeParser : IRichTextAttributeValueParser
    {
        public RichTextAttributeValueParseOptionType ParseOption => RichTextAttributeValueParseOptionType.Required;

        public bool AppendAttributeTo(StringBuilder sb, Dictionary<string, string> attrValues)
        {
            if (attrValues.TryGetValue(RichTextAttributeValueKeys.Color, out var value) &&
                ColorHelper.TryParseHtmlString(value, out var color))
            {
                sb.AppendFormat("<color={0}>", color);
                return true;
            }

            return false;
        }
    }
}
