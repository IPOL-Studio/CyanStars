using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 谱包文件数据结构（包含多个谱面，其中4个难度谱面各0或1个，未定义难度的谱面数量不限）
    /// </summary>
    [Serializable]
    public class ChartPackData
    {
        // 基本信息

        /// <summary>谱包的数据格式版本</summary>
        public int DataVersion;

        /// <summary>谱包标题（音乐名）</summary>
        public string Title;


        // 音乐和演唱版本

        /// <summary>音频信息</summary>
        public List<MusicVersionData> MusicVersionDatas;

        /// <summary>游戏内选中音乐后的预览开始时间</summary>
        [CanBeNull]
        public Beat? MusicPreviewStartBeat;

        /// <summary>游戏内选中音乐后的预览结束时间</summary>
        [CanBeNull]
        public Beat? MusicPreviewEndBeat;


        // 曲绘文件和曲绘裁剪

        /// <summary>游戏内曲绘文件相对路径</summary>
        [CanBeNull]
        public string CoverFilePath;

        /// <summary>将曲绘大图裁剪为1:4小图时，小图左下角在大图上的的像素坐标 X 值（大图左下角为原点）</summary>
        [CanBeNull]
        public int? CoverCroppingPosX;

        /// <summary>将曲绘大图裁剪为1:4小图时，小图左下角在大图上的的像素坐标 Y 值（大图左下角为原点）</summary>
        [CanBeNull]
        public int? CoverCroppingPosY;

        /// <summary>将曲绘大图裁剪为1:4小图时，小图在横向方向上的像素宽度</summary>
        /// <remarks>竖直方向高度为此值的1/4</remarks>
        [CanBeNull]
        public int? CoverCroppingWidth;


        // 谱面音符信息和 Staff 信息

        /// <summary>谱面数据</summary>
        public List<ChartData> ChartDatas;

        /// <summary>音乐创作者、歌姬、谱师、游戏曲绘作者等信息</summary>
        /// <example>{"xxxx": ["作曲", "编曲", "调校", "谱面", "游戏曲绘"]}</example>
        /// <example>{"xxxx": ["作", "编", "调", "谱", "绘"]}</example>
        [CanBeNull]
        public Dictionary<string, List<string>> Staffs;


        // 元数据

        /// <summary>谱包工程文件上一次保存时间</summary>
        public DateTime SaveTime;

        /// <summary>谱包导出时间</summary>
        [CanBeNull]
        public DateTime? ExportTime;

        /// <summary>谱包哈希</summary>
        /// <remarks>导出谱包时固定</remarks>
        [CanBeNull]
        public string PackHash;
    }
}
