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
        public readonly ObservableDictionary<string, ObservableList<ReactiveProperty<string>>> Staffs;

        public MusicVersionDataEditorModel(MusicVersionData musicVersionData)
        {
            MusicVersionData = musicVersionData;

            VersionTitle = new ReactiveProperty<string>(musicVersionData.VersionTitle);
            AudioFilePath = new ReactiveProperty<string>(musicVersionData.AudioFilePath);
            Offset = new ReactiveProperty<int>(musicVersionData.Offset);
            Staffs = new ObservableDictionary<string, ObservableList<ReactiveProperty<string>>>();
            foreach (KeyValuePair<string, List<string>> staffItemPair in musicVersionData.Staffs)
            {
                var jobs = new ObservableList<ReactiveProperty<string>>();
                foreach (string job in staffItemPair.Value)
                {
                    jobs.Add(new ReactiveProperty<string>(job));
                }

                Staffs.Add(staffItemPair.Key, jobs);
            }
        }
    }
}
