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
        public List<RichTextData> Inlines { get; set; }

        [JsonProperty("stop")]
        public int Stop { get; set; }

        // 当前正在使用的Inline
        private int curInlineIndex = -1;

        // 当前 Inline 已经插入到目标StringBuilder的文本数量
        private int curInlineInsertedCount;

        private float remainingDeltaTime;


        private RichText curRichText;


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
                DataModule.Content.Append(curRichText.LeftAttributes);
            }

            var stop = (Stop > 0 ? Stop : SettingsModule.Stop) / 1000f;
            remainingDeltaTime += deltaTime;

            while (remainingDeltaTime > stop && !CheckCompleted())
            {
                remainingDeltaTime -= stop;
                if (AppendContent())
                {
                    DataModule.Content.Append(curRichText.RightAttributes);
                    if (NextInline())
                    {
                        DataModule.Content.Append(curRichText.LeftAttributes);
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

            DataModule.Content.Append(curRichText.Text.Substring(curInlineInsertedCount));
            DataModule.Content.Append(curRichText.RightAttributes);

            while (NextInline())
            {
                DataModule.Content
                    .Append(curRichText.LeftAttributes)
                    .Append(curRichText.Text)
                    .Append(curRichText.RightAttributes);
            }

            DataModule.IsContentDirty = true;
        }

        /// <returns>当前inline内容是否已经全部插入</returns>
        private bool AppendContent()
        {
            bool isAppend = true;

            while (isAppend)
            {
                char c = curRichText.Text[curInlineInsertedCount];
                DataModule.Content.Append(c);
                curInlineInsertedCount++;
                isAppend = curInlineInsertedCount < curRichText.Text.Length && TextHelper.IsSkipChar(c);
            }

            DataModule.IsContentDirty = true;

            return curInlineInsertedCount >= curRichText.Text.Length;
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
                    curRichText = null;
                    return false;
                }
            } while (string.IsNullOrEmpty(Inlines[curInlineIndex].Text));  // 跳过空内容的Inline

            GenerateCurRichText();

            return true;
        }

        private void GenerateCurRichText()
        {
            curRichText = RichText.FromRichTextData(Inlines[curInlineIndex]);
        }
    }

    public enum ContentActionType
    {
        Overwrite,
        Append
    }
}
