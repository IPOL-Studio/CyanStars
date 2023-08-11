using System.Collections.Generic;
using CyanStars.Framework.Timeline;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 提示音轨道数据
    /// </summary>
    public class PromptToneTrackData : ITrackData<PromptToneClipData>
    {
        public int ClipCount => 1;
        public List<PromptToneClipData> ClipDataList { get; set; }

        public PromptToneTrackData(PromptToneClipData clipClipDataList)
        {
            ClipDataList = new List<PromptToneClipData> { clipClipDataList };
        }
    }
}
