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


        // 插入到目标StringBuilder的位置
        private int curInsertIndex;

        // 当前正在使用的Inline
        private int curInlineIndex = -1;

        // 当前 Inline 已经插入到目标StringBuilder的文本数量
        private int curInlineInsertedCount;

        private float remainingDeltaTime;

        public override void OnInit()
        {
            if (Type == ContentActionType.Overwrite)
            {
                DataModule.Content.Clear();
                DataModule.IsContentDirty = true;
            }
            else
            {
                curInsertIndex = DataModule.Content.Length;
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
            }

            var stop = (Stop > 0 ? Stop : SettingsModule.Stop) / 1000f;
            remainingDeltaTime += deltaTime;

            while (remainingDeltaTime > stop && !CheckCompleted())
            {
                remainingDeltaTime -= stop;
                if (AppendContent())
                {
                    NextInline();
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

            DataModule.Content.Insert(curInsertIndex, Inlines[curInlineIndex].Content.Substring(curInlineInsertedCount));
            NextInline();

            while (!CheckCompleted())
            {
                var inline = Inlines[curInlineIndex];
                DataModule.Content.Append($"{inline.GetLeftAttribute()}{inline.Content}{inline.GetRightAttribute()}");
                NextInline();
            }
        }

        /// <returns>当前inline内容是否已经全部插入</returns>
        private bool AppendContent()
        {
            var inline = Inlines[curInlineIndex];
            char c = inline.Content[curInlineInsertedCount];
            DataModule.Content.Insert(curInsertIndex, c);
            curInsertIndex++;
            curInlineInsertedCount++;

            DataModule.IsContentDirty = true;

            return curInlineInsertedCount >= inline.Content.Length;
        }

        private bool CheckCompleted()
        {
            IsCompleted = curInlineIndex >= Inlines.Count;
            return IsCompleted;
        }

        /// <summary>
        /// 切换到下一个Inline，并向最终文本插入额外标签
        /// </summary>
        private void NextInline()
        {
            do
            {
                curInlineIndex++;
                curInlineInsertedCount = 0;

                if (CheckCompleted())
                {
                    return;
                }

                int insertIndex = Inlines[curInlineIndex].CreateContentAttribute(out var attrText);
                curInsertIndex = insertIndex + DataModule.Content.Length;

                if (insertIndex > 0)
                {
                    DataModule.Content.Append(attrText);
                }
            } while (string.IsNullOrEmpty(Inlines[curInlineIndex].Content));  // 跳过空内容的Inline
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

            public int CreateContentAttribute(out string text)
            {
                if (string.IsNullOrEmpty(Content))
                {
                    text = null;
                    return 0;
                }

                if (string.IsNullOrWhiteSpace(Content))
                {
                    text = Content;
                    return 0;
                }

                text = GetLeftAttribute();
                int insertPos = text.Length;
                text += GetRightAttribute();

                return insertPos;
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
