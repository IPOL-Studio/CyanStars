using System;
using System.Collections.Generic;

namespace CatLrcParser
{
    /// <summary>
    /// Lrc解析器
    /// </summary>
    public static class LrcParser
    {
        private static LrcLexer lexer = new LrcLexer();
        private static List<TimeSpan> cachedList = new List<TimeSpan>();

        /// <summary>
        /// 解析Lrc文本
        /// </summary>
        public static Lyric Parse(string lrcText)
        {
            if (string.IsNullOrEmpty(lrcText) || string.IsNullOrWhiteSpace(lrcText))
            {
                return null;
            }
            
            Lyric lrc = new Lyric();
            
            lexer.SetLrcText(lrcText);

            while (lexer.LookNextTokenType() != LrcTokenType.Eof)
            {
                //跳过[
                lexer.GetNextTokenByType(LrcTokenType.LeftBracket);

                if (lexer.LookNextTokenType() == LrcTokenType.String)
                {
                    //解析标识标签
                    ParseIDTag(lrc);
                }
                else
                {
                    //解析时间标签
                    ParseTimeTag(lrc);

                    //解析完一个时间标签后，看看后面是不是接着歌词文本
                    if (lexer.LookNextTokenType() == LrcTokenType.String)
                    {
                        string lyricText = lexer.GetNextTokenByType(LrcTokenType.String).Item1;
                        foreach (TimeSpan timeSpan in cachedList)
                        {
                            //处理多个时间标签对应一句歌词的情况
                            lrc.TimeTagList.Add(new LrcTimeTag(timeSpan,lyricText));
                        }
                        cachedList.Clear();
                    }
                }
            }
            
            //时间标签按时间排序一下
            lrc.TimeTagList.Sort((x,y) => x.Timestamp.CompareTo(y.Timestamp));
            
            return lrc;
        }

        /// <summary>
        /// 解析标识标签
        /// </summary>
        private static void ParseIDTag(Lyric lrc)
        {
            string key = lexer.GetNextTokenByType(LrcTokenType.String).Item1;
            lexer.GetNextTokenByType(LrcTokenType.Colon);  //跳过:

            string value;
            if (lexer.LookNextTokenType() == LrcTokenType.String)
            {
                value = lexer.GetNextTokenByType(LrcTokenType.String).Item1;
            }
            else
            {
                //value可能是数字 但需要用字符串来存
                value = lexer.GetNextTokenByType(LrcTokenType.Number).Item2.ToString();
            }
           
            
            lexer.GetNextTokenByType(LrcTokenType.RightBracket);  //跳过]
            lrc.IDTagDict.Add(key,value);
        }

        /// <summary>
        /// 解析时间标签
        /// </summary>
        private static void ParseTimeTag(Lyric lrc)
        {
            //[mm:ss.ff]
            int mm = lexer.GetNextTokenByType(LrcTokenType.Number).Item2;  //分钟
            
            lexer.GetNextTokenByType(LrcTokenType.Colon);  //跳过:
            
            int ss = lexer.GetNextTokenByType(LrcTokenType.Number).Item2;  //秒
            
            int ff = 0;  //百分之一秒
            if (lexer.LookNextTokenType() == LrcTokenType.Dot)
            {
                lexer.GetNextTokenByType(LrcTokenType.Dot);  //跳过.
                ff  = lexer.GetNextTokenByType(LrcTokenType.Number).Item2;
            }else if (lexer.LookNextTokenType() == LrcTokenType.Colon)
            {
                lexer.GetNextTokenByType(LrcTokenType.Colon);  //跳过:
                ff  = lexer.GetNextTokenByType(LrcTokenType.Number).Item2;
            }
            
            lexer.GetNextTokenByType(LrcTokenType.RightBracket);  //跳过]
            
            TimeSpan timeSpan = new TimeSpan(0, 0, mm, ss, ff * 10);
            cachedList.Add(timeSpan);

        }
    }

}
