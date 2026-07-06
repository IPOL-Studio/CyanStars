#nullable enable

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace CyanStars.Chart
{
    /// <summary>
    /// 谱包文件数据结构（包含多个谱面，其中内置谱包要求 4 个难度谱面各 1 个，不含未定义谱面；玩家谱面不做限制）
    /// </summary>
    public class ChartPackData
    {
        // 基本信息

        /// <summary>
        /// 谱包的数据格式版本
        /// </summary>
        public int DataVersion;

        /// <summary>
        /// 谱包标题（音乐名）
        /// </summary>
        public string Title;

        /// <summary>
        /// 谱包介绍信息，以文本形式详细描述 Staff 职务，也可用于添加其他描述信息
        /// </summary>
        public string ChartPackInfo;


        // 音乐和演唱版本

        /// <summary>
        /// 音频信息
        /// </summary>
        public List<MusicVersionData> MusicVersionDatas;

        /// <summary>
        /// bpm 组
        /// </summary>
        /// <remarks>
        /// 控制不同时候的拍子所占时长（拍子可转换为时间）
        /// </remarks>
        public List<BpmGroupItem> BpmGroup;

        /// <summary>
        /// 游戏内选中音乐后的预览开始时间
        /// </summary>
        public Beat MusicPreviewStartBeat;

        /// <summary>
        /// 游戏内选中音乐后的预览结束时间
        /// </summary>
        public Beat MusicPreviewEndBeat;


        // 曲绘文件

        /// <summary>
        /// 原始曲绘相对路径（展示收藏品原图用）
        /// </summary>
        public string? CoverFilePath;

        /// <summary>
        /// 开始裁剪的坐标比例（相对于图片左下角）
        /// </summary>
        public Vector2? CropStartPositionPercent;

        /// <summary>
        /// 裁剪高度/全图高度的比例
        /// </summary>
        public float? CropHeightPercent;

        // 谱面元数据

        /// <summary>
        /// 谱面元数据
        /// </summary>
        public List<ChartMetaData> ChartMetaDatas;


        /// <summary>
        /// 用于反序列化的构造
        /// </summary>
        [JsonConstructor]
        private ChartPackData()
        {
            Title = "";
            ChartPackInfo = "";
            MusicVersionDatas = new List<MusicVersionData>();
            BpmGroup = new List<BpmGroupItem>();
            ChartMetaDatas = new List<ChartMetaData>();
        }

        /// <summary>
        /// 执行浅拷贝
        /// </summary>
        public ChartPackData(ChartPackData oldChartPackData)
        {
            Title = oldChartPackData.Title;
            MusicVersionDatas = oldChartPackData.MusicVersionDatas;
            ChartPackInfo = oldChartPackData.ChartPackInfo;
            BpmGroup = oldChartPackData.BpmGroup;
            MusicPreviewStartBeat = oldChartPackData.MusicPreviewStartBeat;
            MusicPreviewEndBeat = oldChartPackData.MusicPreviewEndBeat;
            CoverFilePath = oldChartPackData.CoverFilePath;
            CropStartPositionPercent = oldChartPackData.CropStartPositionPercent;
            CropHeightPercent = oldChartPackData.CropHeightPercent;
            ChartMetaDatas = oldChartPackData.ChartMetaDatas;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ChartPackData(string title,
                             string chartPackInfo = "",
                             List<MusicVersionData>? musicVersionDatas = null,
                             List<BpmGroupItem>? bpmGroup = null,
                             Beat? musicPreviewStartBeat = null,
                             Beat? musicPreviewEndBeat = null,
                             string? coverFilePath = null,
                             Vector2? cropStartPositionPercent = null,
                             float? cropHeightPercent = null,
                             List<ChartMetaData>? chartMetaDatas = null)
        {
            DataVersion = 1;
            Title = title;
            ChartPackInfo = chartPackInfo;
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
            CropStartPositionPercent = cropStartPositionPercent ?? Vector2.zero;
            CropHeightPercent = cropHeightPercent ?? 0;
            ChartMetaDatas = chartMetaDatas ?? new List<ChartMetaData>();
        }
    }
}
