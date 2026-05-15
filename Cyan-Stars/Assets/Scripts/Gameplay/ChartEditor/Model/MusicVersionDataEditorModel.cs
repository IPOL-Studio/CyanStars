#nullable enable

using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        public readonly ObservableHashSet<string> StaffNames;

        public MusicVersionDataEditorModel(MusicVersionData musicVersionData)
        {
            MusicVersionData = musicVersionData;

            VersionTitle = new ReactiveProperty<string>(musicVersionData.VersionTitle);
            AudioFilePath = new ReactiveProperty<string>(musicVersionData.AudioFilePath);
            Offset = new ReactiveProperty<int>(musicVersionData.Offset);
            StaffNames = new ObservableHashSet<string>(musicVersionData.StaffNames);
        }

        /// <summary>
        /// 将制谱器的可观察数据转为常规数据，以用于序列化
        /// </summary>
        [Pure]
        public MusicVersionData ToMusicVersionData()
        {
            var versionTitle = VersionTitle.CurrentValue;
            var audioFilePath = AudioFilePath.CurrentValue;
            var offset = Offset.CurrentValue;
            var staffNames = new HashSet<string>();
            foreach (var staff in StaffNames)
                staffNames.Add(staff);
            return new MusicVersionData(versionTitle, audioFilePath, offset, staffNames);
        }
    }
}
