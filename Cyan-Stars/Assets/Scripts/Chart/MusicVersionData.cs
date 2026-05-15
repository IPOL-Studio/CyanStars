#nullable enable

using System.Collections.Generic;
using Newtonsoft.Json;

namespace CyanStars.Chart
{
    /// <summary>
    /// 用于支持多个版本（翻唱/翻调）的音乐
    /// </summary>
    public class MusicVersionData
    {
        /// <summary>
        /// 曲目版本标题
        /// </summary>
        public string VersionTitle;

        /// <summary>
        /// 曲目文件相对路径
        /// </summary>
        public string AudioFilePath;

        /// <summary>
        /// 在播放前添加多长时间的空白（ms）
        /// </summary>
        /// <remarks>
        /// 即谱面延后时间
        /// </remarks>
        public int Offset;

        /// <summary>
        /// 音乐创作者、歌姬、谱师、游戏曲绘作者等 ID
        /// </summary>
        public HashSet<string> StaffNames;


        /// <summary>
        /// 构造函数
        /// </summary>
        [JsonConstructor]
        public MusicVersionData(string versionTitle = "",
                                string audioFilePath = "",
                                int offset = 0,
                                HashSet<string>? staffNames = null)
        {
            VersionTitle = versionTitle;
            AudioFilePath = audioFilePath;
            Offset = offset;
            StaffNames = staffNames ?? new HashSet<string>();
        }
    }
}
