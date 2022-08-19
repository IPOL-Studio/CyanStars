using System.Collections.Generic;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace CyanStars.Gameplay.Dialogue
{
    [DialogueStep("Content")]
    public class ContentStep : BaseStep
    {
        private readonly DialogueDataModule DataModule = GameRoot.GetDataModule<DialogueDataModule>();
        private readonly DialogueSettingsModule SettingsModule = GameRoot.GetDataModule<DialogueSettingsModule>();

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ContentStepType Type { get; set; }

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
            if (Type == ContentStepType.Overlay)
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

            var stop = Stop > 0 ? Stop : SettingsModule.Stop;
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

                text = string.Empty;

                bool colorSupport = ColorUtility.TryParseHtmlString(Color, out _);

                if (colorSupport)
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

                int insertPos = text.Length;

                if (Italic)
                {
                    text += "</i>";
                }

                if (Bold)
                {
                    text += "</b>";
                }

                if (colorSupport)
                {
                    text += "</color>";
                }

                return insertPos;
            }
        }
    }

    public enum ContentStepType
    {
        Overlay,
        Append
    }
}
