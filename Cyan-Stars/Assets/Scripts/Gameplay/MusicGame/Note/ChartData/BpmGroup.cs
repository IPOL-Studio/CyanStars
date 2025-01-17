using System;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    [Serializable]
    public class BpmGroup
    {
        /// <summary>此 BPM 组经过几拍后开始生效</summary>
        /// <remarks>
        /// ...一直生效直到有新的 BPM 组取代之
        /// 首个 BPM 组必须从 [0,0,0] 开始，末个 BPM 组持续到谱面结束。
        /// 拍数以 “带分数” 的形式存储，可计算为浮点格式。例：[1,2,3] = 1 + 2 / 3 = 1.67
        /// </remarks>
        public Vector3 StartBeat;

        /// <summary>在生效时，每分钟会经过几拍</summary>
        /// <remarks>
        /// BeatPerMinute，每分钟拍数，数值越大音乐越快，相同时间内经过的拍子数越多
        /// 一般来说这个值在 60~200 之间都是正常的
        /// </remarks>
        public float Bpm;
    }
}
