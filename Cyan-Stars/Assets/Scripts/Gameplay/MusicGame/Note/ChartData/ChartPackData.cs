using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 谱包数据结构（包含多个谱面，其中4个难度谱面各0或1个，未定义难度的谱面数量不限）
    /// </summary>
    [Serializable]
    public class ChartPackData
    {
        /// <summary>谱包的数据格式版本</summary>
        public int DataVersion;

        /// <summary>谱包标题（音乐名）</summary>
        public string Title;

        /// <summary>音频文件相对路径</summary>
        public string MusicFilePath;

        /// <summary>游戏内曲绘文件相对路径</summary>
        [CanBeNull]
        public string CoverFilePath;

        /// <summary>谱面数据</summary>
        public List<ChartData> ChartDatas;

        /// <summary>谱包哈希</summary>
        /// <remarks>导出谱包时固定</remarks>
        [CanBeNull]
        public string PackHash;

        /// <summary>谱包导出时间</summary>
        [CanBeNull]
        public DateTime? ExportTime; //

        /// <summary>谱包工程文件保存时间</summary>
        public DateTime SaveTime;

        /// <summary>音乐创作者、歌姬、谱师、游戏曲绘作者等信息</summary>
        /// <example>{"xxxx": ["作曲", "编曲", "调校", "谱面", "游戏曲绘"]}</example>
        /// <example>{"xxxx": ["作", "编", "调", "谱", "绘"]}</example>
        [CanBeNull]
        public Dictionary<string, List<string>> Staffs;
    }
}
