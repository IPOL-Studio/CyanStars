using System.Collections.Generic;
using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CyanStars.Gameplay.Dialogue
{
    [DialogueActionUnit("Content")]
    public class ContentAction : BaseActionUnit
    {
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ContentActionType Type { get; set; }

        [JsonProperty("inlines")]
        public List<RichTextData> Inlines { get; set; }

        [JsonProperty("stop")]
        public int Stop { get; set; }

        public override async Task ExecuteAsync()
        {
            var textPrinter = GameRoot.Dialogue.GetService<ITextPrinter>();
            if (Type == ContentActionType.Overwrite)
            {
                await textPrinter.PrintTextAsync(null, false);
            }

            if (Inlines is null || Inlines.Count == 0)
            {
                return;
            }

            var text = GenerateRichText();
            await textPrinter.PrintTextAsync(text, Type == ContentActionType.Append, Stop > 0 ? Stop : (int?)null);
        }

        private string GenerateRichText()
        {
            var cache = GameRoot.GetDataModule<DialogueModule>().StringBuilderCache;
            var sb = cache.Get();

            foreach (var inline in Inlines)
            {
                // 跳过空内容的Inline
                if (string.IsNullOrEmpty(inline.Text))
                {
                    continue;
                }

                var richText = RichText.FromRichTextData(inline);
                richText.AppendFullTextTo(sb);
            }

            var text = sb.ToString();
            cache.Release(sb);
            return text;
        }
    }

    public enum ContentActionType
    {
        Overwrite,
        Append
    }
}
