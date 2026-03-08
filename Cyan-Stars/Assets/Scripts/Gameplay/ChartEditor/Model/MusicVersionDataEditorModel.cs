#nullable enable

using System.Collections.Generic;
using System.Linq;
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
        public readonly ReactiveProperty<string> VersionTitle;
        public readonly ReactiveProperty<string> AudioFilePath;
        public readonly ReactiveProperty<int> Offset;
        public readonly ObservableDictionary<string, List<string>> Staffs;


        /// <summary>
        /// 构造函数：将纯数据实例转为可观察实例，用于制谱器绑定
        /// </summary>
        public MusicVersionDataEditorModel(MusicVersionData musicVersionData)
        {
            VersionTitle = new ReactiveProperty<string>(musicVersionData.VersionTitle);
            AudioFilePath = new ReactiveProperty<string>(musicVersionData.AudioFilePath);
            Offset = new ReactiveProperty<int>(musicVersionData.Offset);
            Staffs = new ObservableDictionary<string, List<string>>(musicVersionData.Staffs);
        }

        /// <summary>
        /// 将可观察实例转为纯数据实例，用于序列化
        /// </summary>
        public MusicVersionData ToMusicVersionData()
        {
            return new MusicVersionData(
                VersionTitle.CurrentValue,
                AudioFilePath.CurrentValue,
                Offset.CurrentValue,
                new Dictionary<string, List<string>>(Staffs.Select(static kvp => new KeyValuePair<string, List<string>>(kvp.Key, kvp.Value)))
            );
        }
    }
}
