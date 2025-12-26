#nullable enable

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace CyanStars.Chart
{
    /// <summary>
    /// 谱包文件数据结构（包含多个谱面，其中4个难度谱面各0或1个，未定义难度的谱面数量不限）
    /// </summary>
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

        /// <summary>bpm 组</summary>
        /// <remarks>控制不同时候的拍子所占时长（拍子可转换为时间）</remarks>
        public List<BpmGroupItem> BpmGroup;

        /// <summary>游戏内选中音乐后的预览开始时间</summary>
        public Beat MusicPreviewStartBeat;

        /// <summary>游戏内选中音乐后的预览结束时间</summary>
        public Beat MusicPreviewEndBeat;


        // 曲绘文件

        /// <summary>原始曲绘相对路径（展示收藏品原图用）</summary>
        public string? CoverFilePath;

        /// <summary>开始裁剪像素（相对于图片左下角）</summary>
        public Vector2? CropStartPosition;

        /// <summary>
        /// 裁剪像素高度
        /// </summary>
        public float? CropHeight;

        /// <summary>
        /// 裁剪像素宽度
        /// </summary>
        [JsonIgnore]
        public float? CropWidth => CropHeight * 4;

        // 谱面元数据

        /// <summary>谱面元数据</summary>
        public List<ChartMetaData> ChartMetaDatas;

        /// <summary>
        /// 执行浅拷贝
        /// </summary>
        public ChartPackData(ChartPackData oldChartPackData)
        {
            DataVersion = oldChartPackData.DataVersion;
            Title = oldChartPackData.Title;
            MusicVersionDatas = oldChartPackData.MusicVersionDatas;
            BpmGroup = oldChartPackData.BpmGroup;
            MusicPreviewStartBeat = oldChartPackData.MusicPreviewStartBeat;
            MusicPreviewEndBeat = oldChartPackData.MusicPreviewEndBeat;
            CoverFilePath = oldChartPackData.CoverFilePath;
            CropStartPosition = oldChartPackData.CropStartPosition;
            CropHeight = oldChartPackData.CropHeight;
            ChartMetaDatas = oldChartPackData.ChartMetaDatas;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ChartPackData(string title, List<MusicVersionData>? musicVersionDatas = null, List<BpmGroupItem>? bpmGroup = null,
                             Beat? musicPreviewStartBeat = null, Beat? musicPreviewEndBeat = null, string? coverFilePath = null,
                             Vector2? cropPosition = null, float? cropHeight = null, List<ChartMetadata>? chartMetaDatas = null)
        {
            DataVersion = 1;
            Title = title;
            MusicVersionDatas = musicVersionDatas ?? new List<MusicVersionData>();
            BpmGroup = bpmGroup ?? new List<BpmGroupItem>();

            if (musicPreviewStartBeat != null)
            {
                MusicPreviewStartBeat = (Beat)musicPreviewStartBeat;
            }
            else
            {
                if (!Beat.TryCreateBeat(0, 0, 1, out MusicPreviewStartBeat))
                {
                    throw new Exception("Couldn't create beat");
                }
            }

            if (musicPreviewEndBeat != null)
            {
                MusicPreviewEndBeat = (Beat)musicPreviewEndBeat;
            }
            else
            {
                if (!Beat.TryCreateBeat(0, 0, 1, out MusicPreviewEndBeat))
                {
                    throw new Exception("Couldn't create beat");
                }
            }

            CoverFilePath = coverFilePath;
            CropStartPosition = cropPosition ?? Vector2.zero;
            CropHeight = cropHeight ?? 0;
            ChartMetaDatas = chartMetaDatas ?? new List<ChartMetaData>();
        }
    }
}
