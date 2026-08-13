#nullable enable

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
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

        public readonly TrackedReactiveProperty<string> VersionTitle;
        public readonly ReactiveProperty<string> AudioFilePath;
        public readonly TrackedReactiveProperty<int> Offset;

        public MusicVersionDataEditorModel(MusicVersionData musicVersionData, CommandStack commandStack)
        {
            MusicVersionData = musicVersionData;

            VersionTitle = new TrackedReactiveProperty<string>(commandStack, musicVersionData.VersionTitle);
            AudioFilePath = new ReactiveProperty<string>(musicVersionData.AudioFilePath);
            Offset = new TrackedReactiveProperty<int>(commandStack, musicVersionData.Offset);
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
            return new MusicVersionData(versionTitle, audioFilePath, offset);
        }
    }
}
