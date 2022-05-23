using System.Collections.Generic;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Data;

namespace CyanStars.Gameplay.PromptTone
{
    /// <summary>
    /// 提示音轨道数据
    /// </summary>
    public class PromptToneTrackData : ITrackData<NoteData>
    {
        public int ClipCount => ClipDataList.Count;
        public List<NoteData> ClipDataList { get; set; }
    }
}