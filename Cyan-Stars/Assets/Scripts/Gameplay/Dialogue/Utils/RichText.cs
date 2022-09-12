using System.Text;
using CyanStars.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace CyanStars.Gameplay.Dialogue
{
    public class RichText
    {
        public string LeftAttributes { get; } = string.Empty;
        public string RightAttributes { get; } = string.Empty;
        public string Text { get; }

        internal RichText(RichTextData data)
        {
            Text = data.Text;
            bool isNoParse = data.Text.IndexOf('<') >= 0;

            if ((data.Attributes?.Count ?? 0) <= 0)
            {
                if (isNoParse)
                {
                    LeftAttributes = TextHelper.NoParseLeftAttr;
                    RightAttributes = TextHelper.NoParseRightAttr;
                }
                return;
            }

            var cache = GameRoot.GetDataModule<DialogueModule>().StringBuilderCache;
            var sb = cache.Get();
            var flags = new NativeArray<bool>(data.Attributes.Count, Allocator.Temp);

            if (ParseLeftAttributes(data, sb, flags, isNoParse))
            {
                LeftAttributes = sb.ToString();
                sb.Clear();

                ParseRightAttributes(data, sb, flags, isNoParse);
                RightAttributes = sb.ToString();
            }
            else if (isNoParse)
            {
                LeftAttributes = TextHelper.NoParseLeftAttr;
                RightAttributes = TextHelper.NoParseRightAttr;
            }

            cache.Release(sb);
            flags.Dispose();
        }

        private bool ParseLeftAttributes(RichTextData data, StringBuilder sb, NativeArray<bool> flags, bool isNoParse)
        {
            bool isAppended = false;

            for (int i = 0; i < data.Attributes.Count; i++)
            {
                string attr = data.Attributes[i];
                if (!TextHelper.IsAttributeSupported(attr))
                    continue;

                if (TextHelper.IsAttributeRequireValue(attr))
                {
                    if (!(data.AttributeValues is null) &&
                        data.AttributeValues.TryGetValue(attr, out var value) &&
                        TextHelper.CombineRequireValueLeftAttribute(attr, value, sb))
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

            sb.Append(isNoParse && isAppended ? TextHelper.NoParseLeftAttr : string.Empty);
            return isAppended;
        }

        private void ParseRightAttributes(RichTextData data, StringBuilder sb, NativeArray<bool> flags, bool isNoParse)
        {
            sb.Append(isNoParse ? TextHelper.NoParseRightAttr : string.Empty);
            for (int i = data.Attributes.Count - 1; i >= 0; i--)
            {
                if (flags[i])
                {
                    sb.Append("</").Append(data.Attributes[i]).Append('>');
                }
            }
        }
    }
}
