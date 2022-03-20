using System;
using System.Text;

namespace CatLrcParser
{
    /// <summary>
    /// Lrc词法分析器
    /// </summary>
    public class LrcLexer
    {
        private static StringBuilder sb = new StringBuilder();

        /// <summary>
        /// 换行符
        /// </summary>
        private static string newLine = Environment.NewLine;
        
        /// <summary>
        /// Lrc文本
        /// </summary>
        private string lrcText;

        /// <summary>
        /// 当前字符索引
        /// </summary>
        private int curIndex;

        /// <summary>
        /// 是否有缓存的下一个token
        /// </summary>
        private bool hasNextToken;

        /// <summary>
        /// 缓存的下一个token
        /// </summary>
        private ValueTuple<string,int> nextToken;

        /// <summary>
        /// 下一个token的类型
        /// </summary>
        private LrcTokenType nextTokenType;

        /// <summary>
        /// 字符串结束符号
        /// </summary>
        private char stringEnd;

        /// <summary>
        /// 设置Lrc文本
        /// </summary>
        public void SetLrcText(string lrcText)
        {
            this.lrcText = lrcText;
            curIndex = 0;
            hasNextToken = false;
        }
        
        /// <summary>
        /// 查看下一个token的类型
        /// </summary>
        public LrcTokenType LookNextTokenType()
        {
            if (hasNextToken)
            {
                //有缓存直接返回缓存
                return nextTokenType;
            }

            //没有就get一下
            nextToken = GetNextToken(out nextTokenType);
            hasNextToken = true;
            return nextTokenType;
        }

        /// <summary>
        /// 获取下一个指定类型的token
        /// </summary>
        public ValueTuple<string,int> GetNextTokenByType(LrcTokenType tokenType)
        {
            ValueTuple<string,int> token = GetNextToken(out LrcTokenType resultType);
            if (tokenType != resultType)
            {
                throw new Exception($"NextTokenOfType调用失败，需求{tokenType}但获取到的是{resultType}");
            }

            return token;
        }


        /// <summary>
        /// 获取下一个token
        /// </summary>
        public ValueTuple<string,int> GetNextToken(out LrcTokenType tokenType)
        {
            tokenType = default;

            if (hasNextToken)
            {
                //有缓存下一个token的信息 直接返回

                hasNextToken = false;
                tokenType = nextTokenType;
                return nextToken;
            }
            

            if (curIndex >= lrcText.Length)
            {
                //文本结束
                tokenType = LrcTokenType.Eof;
                return default;
            }

            char c = lrcText[curIndex];

            //扫描分隔符
            switch (c)
            {
                case '[':
                    stringEnd = ':';
                    tokenType = LrcTokenType.LeftBracket;
                    Next();
                    return default;

                case ']':
                    stringEnd = '[';
                    tokenType = LrcTokenType.RightBracket;
                    Next();
                    return default;

                case ':':
                    stringEnd = ']';
                    tokenType = LrcTokenType.Colon;
                    Next();
                    return default;
                case '.':
                    tokenType = LrcTokenType.Dot;
                    Next();
                    return default;
            }

            //扫描数字
            if (char.IsDigit(c))
            {
                int num = ScanNumber();
                tokenType = LrcTokenType.Number;
                return (null,num);
            }
            
            //扫描字符串
            string result = ScanString();
            tokenType = LrcTokenType.String;
            return (result,0);

        }


        /// <summary>
        /// 移动CurIndex
        /// </summary>
        private void Next(int n = 1)
        {
            curIndex += n;
        }

        /// <summary>
        /// 扫描数字（正整数）
        /// </summary>
        private int ScanNumber()
        {

            int num = 0;
            
            while (!(curIndex >= lrcText.Length) && char.IsDigit(lrcText[curIndex]))
            {
                num = num * 10 + lrcText[curIndex] - '0';
                Next();
            }
            
            return num;

        }

        /// <summary>
        /// 扫描字符串
        /// </summary>
        /// <returns></returns>
        private string ScanString()
        {
            while (curIndex < lrcText.Length)
            {
                if (IsPrefix(newLine))
                {
                    //无视换行符
                    Next(newLine.Length);
                    continue;
                }

                if (lrcText[curIndex] == stringEnd)
                {
                    //遇到stringEnd字符直接结束
                    break;
                }

                //否则视为字符串的一部分
                sb.Append(lrcText[curIndex]);
                Next();
             
            }

            string result = sb.ToString();
            sb.Clear();
            return result;
        }
        
        /// <summary>
        /// 剩余Lrc字符串是否以prefix开头
        /// </summary>
        private bool IsPrefix(string prefix)
        {
            int tempCurIndex = curIndex;
            for (int i = 0; i < prefix.Length; i++, tempCurIndex++)
            {
                if (lrcText[tempCurIndex] != prefix[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}

