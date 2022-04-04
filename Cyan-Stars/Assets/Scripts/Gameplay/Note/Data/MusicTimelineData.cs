using System.Collections.Generic;

namespace CyanStars.Gameplay.Note.Data
{
    /// <summary>
    /// 音乐时间轴数据
    /// </summary>
    [System.Serializable]
    public class MusicTimelineData
    {
        /// <summary>
        /// 总时间（毫秒）
        /// </summary>
        public int Time;

        /// <summary>
        /// 基础速度
        /// </summary>
        public float BaseSpeed = 1;

        /// <summary>
        /// 速率
        /// </summary>
        public float SpeedRate = 1;

        /// <summary>
        /// 图层数据
        /// </summary>
        public List<LayerData> LayerDatas;
    }
}
