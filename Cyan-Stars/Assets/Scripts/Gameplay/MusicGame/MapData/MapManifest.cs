using System;
using System.IO;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 谱面清单
    /// </summary>
    [Serializable]
    public class MapManifest
    {
        /// <summary>
        /// 歌曲名
        /// </summary>
        [Header("歌曲名")]
        public string Name;

        [Header("Staff信息")]
        [Multiline(10)]
        public string StaffInfo;

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
        /// 曲绘文件名
        /// </summary>
        [Header("曲绘文件名")]
        public string CoverFileName;

        /// <summary>
        /// 时间轴文件名(目前内置谱面的为.asset，外置谱面的为.json)
        /// </summary>
        [Header("时间轴文件名")]
        public string TimelineFileName;
    }
}
