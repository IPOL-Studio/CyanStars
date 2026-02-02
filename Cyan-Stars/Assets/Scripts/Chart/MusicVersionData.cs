#nullable enable

using System.Collections.Generic;
using CyanStars.Gameplay.ChartEditor.Model;
using JetBrains.Annotations;
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

        /// <summary>音乐创作者、歌姬、谱师、游戏曲绘作者等信息</summary>
        /// <example>{"xxxx": ["作曲", "编曲", "调校", "谱面", "游戏曲绘"]}</example>
        /// <example>{"xxxx": ["作", "编", "调", "谱", "绘"]}</example>
        public Dictionary<string, List<string>> Staffs;


        /// <summary>
        /// 构造函数
        /// </summary>
        [JsonConstructor]
        public MusicVersionData(string versionTitle = "",
                                string audioFilePath = "",
                                int offset = 0,
                                Dictionary<string, List<string>>? staffs = null)
        {
            VersionTitle = versionTitle;
            AudioFilePath = audioFilePath;
            Offset = offset;
            Staffs = staffs ?? new Dictionary<string, List<string>>();
        }

        /// <summary>
        /// 将制谱器的可观察数据转为常规数据，以用于序列化
        /// </summary>
        public MusicVersionData(MusicVersionDataEditorModel editorData)
        {
            VersionTitle = editorData.VersionTitle.CurrentValue;
            AudioFilePath = editorData.AudioFilePath.CurrentValue;
            Offset = editorData.Offset.CurrentValue;
            Staffs = new Dictionary<string, List<string>>();
            foreach (var staffKvp in editorData.Staffs)
                Staffs.Add(staffKvp.Key, staffKvp.Value);
        }
    }
}
