using System.Collections.Generic;
using System.Text;

namespace CyanStars.Gameplay.Dialogue
{
    public enum RichTextAttributeValueParseOptionType
    {
        Required,
        Optional
    }

    public interface IRichTextAttributeValueParser
    {
        RichTextAttributeValueParseOptionType ParseOption { get; }

        public bool AppendAttributeTo(StringBuilder sb, Dictionary<string, string> attrValues);
    }
}
