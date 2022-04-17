using UnityEngine;

namespace CyanStars.Gameplay.Data
{
    /// <summary>
    /// 音游谱面数据
    /// </summary>
    [System.Serializable]
    public class MusicGameData
    {
        /// <summary>
        /// 总时间（毫秒）
        /// </summary>
        [Header("总时间（毫秒）")]
        public int Time;

        /// <summary>
        /// 歌词文件名
        /// </summary>
        [Header("歌词文件名")]
        public string LrcFileName;
    
        /// <summary>
        /// 音乐文件名
        /// </summary>
        [Header("音乐文件名")]
        public string MusicFileName;
    
        /// <summary>
        /// 音符轨道数据
        /// </summary>
        [Header("音符轨道数据")]
        public NoteTrackData NoteTrackData;

        /// <summary>
        /// 相机轨道数据
        /// </summary>
        [Header("相机轨道数据")]
        public CameraTrackData CameraTrackData;
    
        /// <summary>
        /// 特效轨道数据
        /// </summary>
        [Header("特效轨道数据")]
        public EffectTrackData EffectTrackData;
    }
}


