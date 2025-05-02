using System;
using System.Collections.Generic;
using CyanStars.Gameplay.MusicGame;

namespace CyanStars.Gameplay.Chart
{
    [Serializable]
    public class BpmGroups
    {
        public List<BpmGroup> Groups;

        /// <summary>
        /// 由 Beat 组计算时间（ms）的委托
        /// </summary>
        public delegate int BeatToTimeDelegate(Beat beat);


        /// <summary>
        /// 根据当前 BPM 组，计算 beat 对应的时间(ms)
        /// </summary>
        /// <param name="beat">Beat 形式的拍子</param>
        /// <returns>int 形式的毫秒时间（相对于时间轴开始）</returns>
        public int CalculateTime(Beat beat)
        {
            return CalculateTime(beat.ToFloat());
        }

        /// <summary>
        /// 根据当前 BPM 组，计算 beat 对应的时间(ms)
        /// </summary>
        /// <param name="fBeat">float 形式的拍子</param>
        /// <returns>int 形式的毫秒时间（相对于时间轴开始）</returns
        public int CalculateTime(float fBeat)
        {
            if (Groups.Count == 1)
            {
                return (int)((60 / Groups[0].Bpm) * fBeat * 1000);
            }

            int i = 0; // i 代表 fBeat 所在的 bpm 组下标
            while (i < Groups.Count - 1)
            {
                if (fBeat < Groups[i + 1].StartBeat.ToFloat())
                {
                    break;
                }

                i++;
            }


            float sumTime = 0f;
            for (int j = 0; j < i; j++)
            {
                sumTime += (Groups[j + 1].StartBeat.ToFloat() - Groups[j].StartBeat.ToFloat())
                           * (60 / Groups[j].Bpm) * 1000f;
            }

            sumTime += (fBeat - Groups[i].StartBeat.ToFloat())
                       * (60 / Groups[i].Bpm) * 1000f;

            return (int)sumTime;
        }
    }

    [Serializable]
    public class BpmGroup
    {
        /// <summary>在生效时，每分钟会经过几拍</summary>
        /// <remarks>
        /// BeatPerMinute，每分钟拍数，数值越大音乐越快，相同时间内经过的拍子数越多
        /// 一般来说这个值在 60~200 之间都是正常的
        /// </remarks>
        public float Bpm;

        /// <summary>此 BPM 组经过几拍后开始生效</summary>
        /// <remarks>
        /// ...一直生效直到有新的 BPM 组取代之
        /// 首个 BPM 组必须从 [0,0,0] 开始，末个 BPM 组持续到谱面结束。
        /// </remarks>
        public Beat StartBeat;
    }
}
