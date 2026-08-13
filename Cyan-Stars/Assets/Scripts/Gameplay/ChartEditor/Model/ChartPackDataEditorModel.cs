#nullable enable

using System.Diagnostics.Contracts;
using System.Linq;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using ObservableCollections;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.Model
{
    /// <summary>
    /// 在制谱器内使用的谱包数据类
    /// </summary>
    public class ChartPackDataEditorModel
    {
        public readonly ReactiveProperty<int> DataVersion;
        public readonly TrackedReactiveProperty<string> Title;
        public readonly TrackedReactiveProperty<string> ChartPackInfo;
        public readonly ObservableList<MusicVersionDataEditorModel> MusicVersions;
        public readonly ObservableList<BpmGroupItem> BpmGroup;
        public readonly TrackedReactiveProperty<Beat> MusicPreviewStartBeat;
        public readonly TrackedReactiveProperty<Beat> MusicPreviewEndBeat;
        public readonly ReactiveProperty<string?> CoverFilePath;
        public readonly ReactiveProperty<Vector2?> CropStartPositionPercent;
        public readonly ReactiveProperty<float?> CropHeightPercent;
        public readonly ObservableList<ChartMetaDataEditorModel> ChartMetaDatas;

        public ChartPackDataEditorModel(ChartPackData chartPackData, CommandStack commandStack)
        {
            DataVersion = new ReactiveProperty<int>(chartPackData.DataVersion);
            Title = new TrackedReactiveProperty<string>(commandStack, chartPackData.Title);
            ChartPackInfo = new TrackedReactiveProperty<string>(commandStack, chartPackData.ChartPackInfo);
            MusicVersions = new ObservableList<MusicVersionDataEditorModel>(
                chartPackData.MusicVersionDatas
                    .Select(v => new MusicVersionDataEditorModel(v, commandStack))
            );
            BpmGroup = new ObservableList<BpmGroupItem>(chartPackData.BpmGroup);
            MusicPreviewStartBeat = new TrackedReactiveProperty<Beat>(commandStack, chartPackData.MusicPreviewStartBeat);
            MusicPreviewEndBeat = new TrackedReactiveProperty<Beat>(commandStack, chartPackData.MusicPreviewEndBeat);
            CoverFilePath = new ReactiveProperty<string?>(chartPackData.CoverFilePath);
            CropStartPositionPercent = new ReactiveProperty<Vector2?>(chartPackData.CropStartPositionPercent);
            CropHeightPercent = new ReactiveProperty<float?>(chartPackData.CropHeightPercent);
            ChartMetaDatas = new ObservableList<ChartMetaDataEditorModel>(
                chartPackData.ChartMetaDatas
                    .Select(d => new ChartMetaDataEditorModel(d, commandStack))
            );
        }

        /// <summary>
        /// 将制谱器的可观察数据转为常规数据，以用于序列化
        /// </summary>
        [Pure]
        public ChartPackData ToChartPackData()
        {
            var title = Title.CurrentValue;
            var chartPackInfo = ChartPackInfo.CurrentValue;
            var musicVersionDatas =
                MusicVersions.Select(musicVersionEditorDatas => musicVersionEditorDatas.ToMusicVersionData()).ToList();
            var bpmGroup = BpmGroup.ToList();
            var musicPreviewStartBeat = MusicPreviewStartBeat.CurrentValue;
            var musicPreviewEndBeat = MusicPreviewEndBeat.CurrentValue;
            var coverFilePath = CoverFilePath.CurrentValue;
            var cropStartPositionPercent = CropStartPositionPercent.CurrentValue;
            var cropHeightPercent = CropHeightPercent.CurrentValue;
            var chartMetaDatas =
                ChartMetaDatas.Select(chartMetaEditorData => chartMetaEditorData.ToChartMetaData()).ToList();
            return new ChartPackData(
                title,
                chartPackInfo,
                musicVersionDatas,
                bpmGroup,
                musicPreviewStartBeat,
                musicPreviewEndBeat,
                coverFilePath,
                cropStartPositionPercent,
                cropHeightPercent,
                chartMetaDatas
            );
        }
    }
}
