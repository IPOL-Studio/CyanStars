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

            if ((data.Attributes?.Count ?? 0) <= 0) return;

            var cache = GameRoot.GetDataModule<DialogueModule>().StringBuilderCache;
            var sb = cache.Get();
            var flags = new NativeArray<bool>(data.Attributes.Count, Allocator.Temp);

            if (ParseLeftAttributes(data, sb, flags))
            {
                LeftAttributes = sb.ToString();
                sb.Clear();

                ParseRightAttributes(data, sb, flags);
                RightAttributes = sb.ToString();
            }

            cache.Release(sb);
            flags.Dispose();
        }

        private bool ParseLeftAttributes(RichTextData data, StringBuilder sb, NativeArray<bool> flags)
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

            if (isAppended)
            {
                sb.Append(TextHelper.NoParseLeftAttr);
                return true;
            }

            return false;
        }

        private void ParseRightAttributes(RichTextData data, StringBuilder sb, NativeArray<bool> flags)
        {
            sb.Append(TextHelper.NoParseRightAttr);
            for (int i = data.Attributes.Count - 1; i < 0; i--)
            {
                if (flags[i])
                {
                    sb.Append("</").Append(data.Attributes[i]).Append('>');
                }
            }
        }
    }
}
