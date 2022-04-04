using System;

namespace CatLrcParser
{
    /// <summary>
    /// Lrc时间标签
    /// </summary>
    public readonly struct LrcTimeTag
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public readonly TimeSpan Timestamp;

        /// <summary>
        /// 歌词文本
        /// </summary>
        public readonly string LyricText;

        public LrcTimeTag(TimeSpan timestamp, string lyricText)
        {
            Timestamp = timestamp;
            LyricText = lyricText;
        }

        public override string ToString()
        {
            return $"[{Timestamp.Minutes:00}:{Timestamp.Seconds:00}.{Timestamp.Milliseconds * 0.1:00}]{LyricText}";
        }
    }
}

