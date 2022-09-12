using System.Collections.Generic;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace CyanStars.Gameplay.Dialogue
{
    [DialogueActionUnit("Content")]
    public class ContentAction : BaseActionUnit
    {
        private readonly DialogueModule DataModule = GameRoot.GetDataModule<DialogueModule>();
        private readonly DialogueSettingsModule SettingsModule = GameRoot.GetDataModule<DialogueSettingsModule>();

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ContentActionType Type { get; set; }

        [JsonProperty("inlines")]
        public List<InlineContent> Inlines { get; set; }

        [JsonProperty("stop")]
        public int Stop { get; set; }

        // 当前正在使用的Inline
        private int curInlineIndex = -1;

        // 当前 Inline 已经插入到目标StringBuilder的文本数量
        private int curInlineInsertedCount;

        private float remainingDeltaTime;

        private InlineContent CurInline => Inlines[curInlineIndex];

        public override void OnInit()
        {
            if (Type == ContentActionType.Overwrite)
            {
                DataModule.Content.Clear();
                DataModule.IsContentDirty = true;
            }

            if (Inlines is null || Inlines.Count == 0)
            {
                IsCompleted = true;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            if (CheckCompleted())
                return;

            if (curInlineIndex == -1)
            {
                NextInline();
                DataModule.Content.Append(CurInline.GetLeftAttribute());
            }

            var stop = (Stop > 0 ? Stop : SettingsModule.Stop) / 1000f;
            remainingDeltaTime += deltaTime;

            while (remainingDeltaTime > stop && !CheckCompleted())
            {
                remainingDeltaTime -= stop;
                if (AppendContent())
                {
                    DataModule.Content.Append(CurInline.GetRightAttribute());
                    if (NextInline())
                    {
                        DataModule.Content.Append(CurInline.GetLeftAttribute());
                    }
                }
            }

            CheckCompleted();
        }

        public override void OnComplete()
        {
            if (CheckCompleted())
            {
                return;
            }

            DataModule.Content.Append(CurInline.Content.Substring(curInlineInsertedCount));
            DataModule.Content.Append(CurInline.GetRightAttribute());

            while (NextInline())
            {
                DataModule.Content
                    .Append(CurInline.GetLeftAttribute())
                    .Append(CurInline.Content)
                    .Append(CurInline.GetRightAttribute());
            }

            DataModule.IsContentDirty = true;
        }

        /// <returns>当前inline内容是否已经全部插入</returns>
        private bool AppendContent()
        {
            bool isAppend = true;

            while (isAppend)
            {
                char c = CurInline.Content[curInlineInsertedCount];
                DataModule.Content.Append(c);
                curInlineInsertedCount++;
                isAppend = curInlineInsertedCount < CurInline.Content.Length && TextHelper.IsSkipChar(c);
            }

            DataModule.IsContentDirty = true;

            return curInlineInsertedCount >= CurInline.Content.Length;
        }

        private bool CheckCompleted()
        {
            IsCompleted = curInlineIndex >= Inlines.Count;
            return IsCompleted;
        }

        /// <summary>
        /// 切换到下一个Inline，并向最终文本插入额外标签
        /// </summary>
        /// <returns>是否存在可以继续访问的Inline</returns>
        private bool NextInline()
        {
            do
            {
                curInlineIndex++;
                curInlineInsertedCount = 0;

                if (CheckCompleted())
                {
                    return false;
                }
            } while (string.IsNullOrEmpty(Inlines[curInlineIndex].Content));  // 跳过空内容的Inline

            return true;
        }

        public class InlineContent
        {
            [JsonProperty("content")]
            public string Content { get; set; }

            [JsonProperty("color")]
            public string Color { get; set; }

            [JsonProperty("bold")]
            public bool Bold { get; set; }

            [JsonProperty("italic")]
            public bool Italic { get; set; }

            private bool? isSupportColor = null;

            public bool IsSupportColor()
            {
                isSupportColor ??= ColorUtility.TryParseHtmlString(Color, out _);
                return isSupportColor.Value;
            }

            public string GetLeftAttribute()
            {
                var text = string.Empty;

                if (IsSupportColor())
                {
                    text += $"<color={Color}>";
                }

                if (Bold)
                {
                    text += "<b>";
                }

                if (Italic)
                {
                    text += "<i>";
                }

                return text;
            }

            public string GetRightAttribute()
            {
                var text = string.Empty;

                if (Italic)
                {
                    text += "</i>";
                }

                if (Bold)
                {
                    text += "</b>";
                }

                if (IsSupportColor())
                {
                    text += "</color>";
                }

                return text;
            }
        }
    }

    public enum ContentActionType
    {
        Overwrite,
        Append
    }
}
