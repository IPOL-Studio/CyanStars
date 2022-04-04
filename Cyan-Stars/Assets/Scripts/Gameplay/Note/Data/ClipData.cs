using System.Collections.Generic;

namespace CyanStars.Gameplay.Note.Data
{
    /// <summary>
    /// 音乐时间轴片段数据
    /// </summary>
    [System.Serializable]
    public class ClipData
    {
        /// <summary>
        /// 开始时间（毫秒）
        /// </summary>
        public int StartTime;

        /// <summary>
        /// 速率
        /// </summary>
        public float SpeedRate = 1;

        /// <summary>
        /// 音符数据
        /// </summary>
        public List<NoteData> NoteDatas;
    }
}
