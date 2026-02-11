#nullable enable

using System.Collections.Generic;
using CyanStars.Chart;
using ObservableCollections;
using R3;

namespace CyanStars.Gameplay.ChartEditor.Model
{
    /// <summary>
    /// 在制谱器内使用的音乐版本数据类
    /// </summary>
    public class MusicVersionDataEditorModel
    {
        private readonly MusicVersionData MusicVersionData;

        public readonly ReactiveProperty<string> VersionTitle;
        public readonly ReactiveProperty<string> AudioFilePath;
        public readonly ReactiveProperty<int> Offset;
        public readonly ObservableDictionary<string, List<string>> Staffs;

        public MusicVersionDataEditorModel(MusicVersionData musicVersionData)
        {
            MusicVersionData = musicVersionData;

            VersionTitle = new ReactiveProperty<string>(musicVersionData.VersionTitle);
            AudioFilePath = new ReactiveProperty<string>(musicVersionData.AudioFilePath);
            Offset = new ReactiveProperty<int>(musicVersionData.Offset);
            Staffs = new ObservableDictionary<string, List<string>>(musicVersionData.Staffs);
        }
    }
}
