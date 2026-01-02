#nullable enable

using System.Collections.Generic;
using CyanStars.Chart;
using R3;
using ObservableCollections;

namespace CyanStars.Gameplay.ChartEditor.Model
{
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
