using System.Collections.Generic;
using CyanStars.Framework.Timeline;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 提示音轨道数据
    /// </summary>
    public class PromptToneTrackData : ITrackData<PromptToneDataCollection>
    {
        public int ClipCount => 1;
        public List<PromptToneDataCollection> ClipDataList { get; set; }

        public PromptToneTrackData(PromptToneDataCollection clipDataList)
        {
            ClipDataList = new List<PromptToneDataCollection> { clipDataList };
        }
    }
}
