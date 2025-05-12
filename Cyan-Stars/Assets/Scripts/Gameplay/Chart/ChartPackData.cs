using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CyanStars.Gameplay.Chart
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


        // 曲绘文件

        /// <summary>原始曲绘相对路径（展示收藏品原图用）</summary>
        [CanBeNull]
        public string CoverFilePath;

        /// <summary>裁剪后的1:4横向小图（在选取页展示用）</summary>
        [CanBeNull]
        public string CroppedCoverFilePath;


        // 谱面元数据

        /// <summary>谱面元数据</summary>
        public List<ChartMetadata> Charts;


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
