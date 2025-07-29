using System.Collections.Generic;

namespace CyanStars.Chart
{
    public class BpmGroup
    {
        public List<BpmGroupItem> Groups = new List<BpmGroupItem>();

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
        /// <returns>int 形式的毫秒时间（相对于时间轴开始）</returns>
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
}
