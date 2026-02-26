#nullable enable

using System.Collections.Generic;
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
        public readonly ReactiveProperty<Vector2?> CropStartPosition;
        public readonly ReactiveProperty<float?> CropHeight;
        public readonly ReadOnlyReactiveProperty<float?> CropWidth;
        public readonly ObservableList<ChartMetaDataEditorModel> ChartMetaDatas;


        /// <summary>
        /// 构造函数：将纯数据实例转为可观察实例，用于制谱器绑定
        /// </summary>
        public ChartPackDataEditorModel(ChartPackData chartPackData)
        {
            DataVersion = new ReactiveProperty<int>(chartPackData.DataVersion);
            Title = new ReactiveProperty<string>(chartPackData.Title);
            MusicVersions = new ObservableList<MusicVersionDataEditorModel>(chartPackData.MusicVersionDatas.Select(static v => new MusicVersionDataEditorModel(v)));
            BpmGroup = new ObservableList<BpmGroupItem>(chartPackData.BpmGroup);
            MusicPreviewStartBeat = new ReactiveProperty<Beat>(chartPackData.MusicPreviewStartBeat);
            MusicPreviewEndBeat = new ReactiveProperty<Beat>(chartPackData.MusicPreviewEndBeat);
            CoverFilePath = new ReactiveProperty<string?>(chartPackData.CoverFilePath);
            CropStartPosition = new ReactiveProperty<Vector2?>(chartPackData.CropStartPosition);
            CropHeight = new ReactiveProperty<float?>(chartPackData.CropHeight);
            CropWidth = CropHeight.Select(static h => h * 4.0f).ToReadOnlyReactiveProperty();
            ChartMetaDatas = new ObservableList<ChartMetaDataEditorModel>(chartPackData.ChartMetaDatas.Select(static d => new ChartMetaDataEditorModel(d)));
        }

        /// <summary>
        /// 将可观察实例转为纯数据实例，用于序列化
        /// </summary>
        public ChartPackData ToChartPackData()
        {
            return new ChartPackData(
                    Title.Value,
                    new List<MusicVersionData>(MusicVersions.Select(static v => v.ToMusicVersionData())),
                    new List<BpmGroupItem>(BpmGroup),
                    MusicPreviewStartBeat.CurrentValue,
                    MusicPreviewEndBeat.CurrentValue,
                    CoverFilePath.CurrentValue,
                    CropStartPosition.CurrentValue,
                    CropHeight.CurrentValue,
                    new List<ChartMetaData>(ChartMetaDatas.Select(static d => d.ToChartMetaData()))
                )
                ;
        }
    }
}
