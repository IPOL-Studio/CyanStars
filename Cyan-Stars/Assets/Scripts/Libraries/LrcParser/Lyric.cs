using System.Collections.Generic;

namespace CatLrcParser
{
    /// <summary>
    /// Lrc歌词
    /// </summary>
    public class Lyric
    {
        /// <summary>
        /// 标识标签的字典
        /// </summary>
        public readonly Dictionary<string, string> IDTagDict = new Dictionary<string, string>();

        /// <summary>
        /// 时间标签的列表
        /// </summary>
        public readonly List<LrcTimeTag> TimeTagList = new List<LrcTimeTag>();

        /// <summary>
        /// 获取歌手名
        /// </summary>
        public string GetArtist()
        {
            IDTagDict.TryGetValue("ar", out string result);
            return result;
        }

        /// <summary>
        /// 获取歌曲名
        /// </summary>
        public string GetTitle()
        {
            IDTagDict.TryGetValue("ti", out string result);
            return result;
        }

        /// <summary>
        /// 获取专辑名
        /// </summary>
        public string GetAlbum()
        {
            IDTagDict.TryGetValue("al", out string result);
            return result;
        }
        
        /// <summary>
        /// 获取歌词制作者
        /// </summary>
        public string GetBy()
        {
            IDTagDict.TryGetValue("by", out string result);
            return result;
        }
        
        /// <summary>
        /// 获取补偿时值（毫秒）
        /// </summary>
        public float GetOffset()
        {
            int offset = 0;
            if (IDTagDict.TryGetValue("offset", out string result))
            {
                offset = int.Parse(result);
            }

            return offset;
        }
    }
}

