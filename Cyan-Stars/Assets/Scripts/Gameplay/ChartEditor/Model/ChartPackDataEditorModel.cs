#nullable enable

using System.Diagnostics.Contracts;
using System.Linq;
using CyanStars.Chart;
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
        public readonly ReactiveProperty<string> Title;
        public readonly ObservableList<MusicVersionDataEditorModel> MusicVersions;
        public readonly ObservableList<BpmGroupItem> BpmGroup;
        public readonly ReactiveProperty<Beat> MusicPreviewStartBeat;
        public readonly ReactiveProperty<Beat> MusicPreviewEndBeat;
        public readonly ReactiveProperty<string?> CoverFilePath;
        public readonly ReactiveProperty<Vector2?> CropStartPositionPercent;
        public readonly ReactiveProperty<float?> CropHeightPercent;
        public readonly ObservableList<ChartMetaDataEditorModel> ChartMetaDatas;

        public ChartPackDataEditorModel(ChartPackData chartPackData)
        {
            DataVersion = new ReactiveProperty<int>(chartPackData.DataVersion);
            Title = new ReactiveProperty<string>(chartPackData.Title);
            MusicVersions = new ObservableList<MusicVersionDataEditorModel>(chartPackData.MusicVersionDatas.Select(static v => new MusicVersionDataEditorModel(v)));
            BpmGroup = new ObservableList<BpmGroupItem>(chartPackData.BpmGroup);
            MusicPreviewStartBeat = new ReactiveProperty<Beat>(chartPackData.MusicPreviewStartBeat);
            MusicPreviewEndBeat = new ReactiveProperty<Beat>(chartPackData.MusicPreviewEndBeat);
            CoverFilePath = new ReactiveProperty<string?>(chartPackData.CoverFilePath);
            CropStartPositionPercent = new ReactiveProperty<Vector2?>(chartPackData.CropStartPositionPercent);
            CropHeightPercent = new ReactiveProperty<float?>(chartPackData.CropHeightPercent);
            ChartMetaDatas = new ObservableList<ChartMetaDataEditorModel>(chartPackData.ChartMetaDatas.Select(static d => new ChartMetaDataEditorModel(d)));
        }

        /// <summary>
        /// 将制谱器的可观察数据转为常规数据，以用于序列化
        /// </summary>
        [Pure]
        public ChartPackData ToChartPackData()
        {
            var title = Title.CurrentValue;
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
            return new ChartPackData(title, musicVersionDatas, bpmGroup, musicPreviewStartBeat, musicPreviewEndBeat, coverFilePath, cropStartPositionPercent, cropHeightPercent, chartMetaDatas);
        }
    }
}
