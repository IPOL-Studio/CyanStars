#nullable enable

using System;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.BindableProperty;

namespace CyanStars.Gameplay.ChartEditor.Model
{
    public class ChartEditorModel
    {
        // == == 谱包和谱面数据 == ==

        // 元数据，在实例化 Model 时固定
        public readonly string WorkspacePath; // 当前的工作区绝对路径（谱包索引文件所在路径）
        public readonly int ChartMetadataIndex; // 当前编辑的谱面对应的谱包中的元数据

        // 当前正在编辑的谱包和谱面内容
        public readonly ChartPackData ChartPackData;
        public readonly ChartData ChartData;


        // == == 编辑器运行时数据 == ==
        public readonly BindableProperty<bool> IsSimplificationMode = new BindableProperty<bool>(true);
        public readonly BindableProperty<EditToolType> SelectedEditTool = new BindableProperty<EditToolType>(EditToolType.Select);

        public readonly BindableProperty<bool> ChartPackDataCanvasVisibility = new BindableProperty<bool>(false);
        public readonly BindableProperty<bool> ChartDataCanvasVisibility = new BindableProperty<bool>(false);
        public readonly BindableProperty<bool> MusicVersionCanvasVisibility = new BindableProperty<bool>(false);
        public readonly BindableProperty<bool> BpmGroupCanvasVisibility = new BindableProperty<bool>(false);
        public readonly BindableProperty<bool> SpeedTemplateCanvasVisibility = new BindableProperty<bool>(false); // TODO

        public readonly BindableProperty<int> PosAccuracy = new BindableProperty<int>(4);
        public readonly BindableProperty<bool> PosMagnet = new BindableProperty<bool>(true);
        public readonly BindableProperty<int> BeatAccuracy = new BindableProperty<int>(2);
        public readonly BindableProperty<float> BeatZoom = new BindableProperty<float>(1f);


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="workspacePath">工作区绝对路径（谱包索引文件所在路径）</param>
        /// <param name="chartMetadataIndex">谱面在谱包元数据中的索引</param>
        /// <param name="chartPackData">要修改的谱包数据，注意请先深拷贝一份</param>
        /// <param name="chartData">要修改的谱面数据，注意请先深拷贝一份</param>
        public ChartEditorModel(string workspacePath,
                                int chartMetadataIndex,
                                ChartPackData chartPackData,
                                ChartData chartData)
        {
            WorkspacePath = workspacePath;
            ChartMetadataIndex = chartMetadataIndex;

            ChartPackData = chartPackData;
            ChartData = chartData;
        }


        #region 谱包基本信息（ChartPackData VM）

        /// <summary>
        /// 谱包基本数据（标题、预览拍、曲绘）发生变化
        /// </summary>
        public Action<ChartPackData>? OnChartPackBasicDataChanged;

        /// <summary>
        /// 设置谱包标题
        /// </summary>
        public void SetChartPackTitle(string title)
        {
            if (title == ChartPackData.Title)
                return;
            ChartPackData.Title = title;
            OnChartPackBasicDataChanged?.Invoke(ChartPackData);
        }

        /// <summary>
        /// 设置谱包预览开始拍
        /// </summary>
        public void SetChartPackPreviewStartBeat(Beat beat)
        {
            if (beat == ChartPackData.MusicPreviewStartBeat)
                return;
            ChartPackData.MusicPreviewStartBeat = beat;
            OnChartPackBasicDataChanged?.Invoke(ChartPackData);
        }

        /// <summary>
        /// 设置谱包预览结束拍
        /// </summary>
        public void SetChartPackPreviewEndBeat(Beat beat)
        {
            if (beat == ChartPackData.MusicPreviewEndBeat)
                return;
            ChartPackData.MusicPreviewEndBeat = beat;
            OnChartPackBasicDataChanged?.Invoke(ChartPackData);
        }

        /// <summary>
        /// 设置谱包曲绘文件相对路径
        /// </summary>
        public void SetChartPackCoverPath(string coverPath)
        {
            if (coverPath == ChartPackData.CoverFilePath)
                return;
            ChartPackData.CoverFilePath = coverPath;
            OnChartPackBasicDataChanged?.Invoke(ChartPackData);
        }

        #endregion

        #region 谱面基本信息（ChartPackData VM）

        /// <summary>
        /// 谱面基本数据（难度、定数、预备拍）发生变化
        /// </summary>
        public Action<ChartPackData, ChartData>? OnChartBasicDataChanged;

        public void SetChartDifficulty(ChartDifficulty? difficulty)
        {
            if (difficulty == ChartPackData.ChartMetaDatas[ChartMetadataIndex].Difficulty)
                return;
            ChartPackData.ChartMetaDatas[ChartMetadataIndex].Difficulty = difficulty;
            OnChartBasicDataChanged?.Invoke(ChartPackData, ChartData);
        }

        public void SetChartLevel(string level)
        {
            if (level == ChartPackData.ChartMetaDatas[ChartMetadataIndex].Level)
                return;
            ChartPackData.ChartMetaDatas[ChartMetadataIndex].Level = level;
            OnChartBasicDataChanged?.Invoke(ChartPackData, ChartData);
        }

        public void SetChartReadyBeatCount(int readyBeatCount)
        {
            if (readyBeatCount == ChartData.ReadyBeat)
                return;
            ChartData.ReadyBeat = readyBeatCount;
            OnChartBasicDataChanged?.Invoke(ChartPackData, ChartData);
        }

        #endregion
    }
}
