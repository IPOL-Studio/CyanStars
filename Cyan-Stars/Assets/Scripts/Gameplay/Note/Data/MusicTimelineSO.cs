using UnityEngine;

namespace CyanStars.Gameplay.Note
{
    /// <summary>
    /// 音乐时间轴数据的SO
    /// </summary>
    [CreateAssetMenu(menuName = "音乐时间轴配置")]
    public class MusicTimelineSO : ScriptableObject
    {
        public MusicTimelineData musicTimelineData;
    }
}