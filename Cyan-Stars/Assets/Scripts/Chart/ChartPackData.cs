using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace CyanStars.Chart
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
        public Beat MusicPreviewStartBeat { get; set; }

        /// <summary>游戏内选中音乐后的预览结束时间</summary>
        public Beat MusicPreviewEndBeat;


        // 曲绘文件

        /// <summary>原始曲绘相对路径（展示收藏品原图用）</summary>
        [CanBeNull]
        public string CoverFilePath;

        /// <summary>开始裁剪像素（相对于图片左下角）</summary>
        public Vector2 CropPosition;

        /// <summary>
        /// 裁剪高度
        /// </summary>
        public float CropHeight;

        /// <summary>
        /// 裁剪宽度
        /// </summary>
        public float CropWidth => CropHeight * 4;

        // 谱面元数据

        /// <summary>谱面元数据</summary>
        public List<ChartMetadata> ChartMetaDatas;


        public ChartPackData(string title, List<MusicVersionData> musicVersionDatas = null,
            Beat? musicPreviewStartBeat = null, Beat? musicPreviewEndBeat = null, string coverFilePath = null,
            string croppedCoverFilePath = null, List<ChartMetadata> chartMetaDatas = null)
        {
            DataVersion = 1;
            Title = title;
            MusicVersionDatas = musicVersionDatas ?? new List<MusicVersionData>();
            MusicPreviewStartBeat = musicPreviewStartBeat ?? new Beat(0, 0, 1);
            MusicPreviewEndBeat = musicPreviewEndBeat ?? new Beat(0, 0, 1);
            CoverFilePath = coverFilePath;
            ChartMetaDatas = chartMetaDatas ?? new List<ChartMetadata>();
        }
    }
}
