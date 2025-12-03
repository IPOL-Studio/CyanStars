using System.Collections.Generic;

namespace CyanStars.Chart
{
    public class BpmGroup
    {
        private List<BpmGroupItem> data = new List<BpmGroupItem>();
        public IReadOnlyList<BpmGroupItem> Data => data;


        /// <summary>
        /// 由 Beat 组计算时间（ms）的委托
        /// </summary>
        public delegate int BeatToTimeDelegate(Beat beat);


        public void Add(BpmGroupItem item)
        {
            data.Add(item);
            data.Sort((item1, item2) => item1.StartBeat.ToFloat().CompareTo(item2.StartBeat.ToFloat()));
        }

        public void Remove(BpmGroupItem item)
        {
            data.Remove(item);
        }

        public void Clear()
        {
            data.Clear();
        }

        public int IndexOf(BpmGroupItem item)
        {
            return data.IndexOf(item);
        }

        public void RemoveAt(int index)
        {
            data.RemoveAt(index);
        }


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
            if (Data.Count == 1)
            {
                return (int)((60 / Data[0].Bpm) * fBeat * 1000);
            }

            int i = 0; // i 代表 fBeat 所在的 bpm 组下标
            while (i < Data.Count - 1)
            {
                if (fBeat < Data[i + 1].StartBeat.ToFloat())
                {
                    break;
                }

                i++;
            }


            float sumTime = 0f;
            for (int j = 0; j < i; j++)
            {
                sumTime += (Data[j + 1].StartBeat.ToFloat() - Data[j].StartBeat.ToFloat())
                           * (60 / Data[j].Bpm) * 1000f;
            }

            sumTime += (fBeat - Data[i].StartBeat.ToFloat())
                       * (60 / Data[i].Bpm) * 1000f;

            return (int)sumTime;
        }

        /// <summary>
        /// 根据给定的毫秒时间，计算对应的 Beat (float)
        /// </summary>
        /// <param name="msTime">已经计算 offset 后的毫秒时间</param>
        /// <returns>float 形式的拍子</returns>
        public float CalculateBeat(int msTime)
        {
            if (Data.Count == 0)
            {
                return 0f;
            }

            // 如果只有一个 BPM 组，直接转换
            if (Data.Count == 1)
            {
                return (msTime / 1000f) * (Data[0].Bpm / 60f);
            }

            float remainingMs = msTime;

            // 遍历除了最后一个之外的所有 BPM 组
            for (int i = 0; i < Data.Count - 1; i++)
            {
                var currentItem = Data[i];
                var nextItem = Data[i + 1];

                // 计算当前 BPM 段持续了多少拍
                float beatDuration = nextItem.StartBeat.ToFloat() - currentItem.StartBeat.ToFloat();

                // 将拍数转换为毫秒：拍数 * (60 / BPM) * 1000
                float timeDuration = beatDuration * (60f / currentItem.Bpm) * 1000f;

                // 如果给定的时间在这个段内
                if (remainingMs < timeDuration)
                {
                    // 计算这段时间对应的拍数：时间(s) * (BPM / 60)
                    float beatInSegment = (remainingMs / 1000f) * (currentItem.Bpm / 60f);
                    return currentItem.StartBeat.ToFloat() + beatInSegment;
                }

                // 如果时间超过了这个段，减去这段的时间，继续检查下一段
                remainingMs -= timeDuration;
            }

            // 如果遍历完还没有返回，说明时间落在了最后一个 BPM 组（无限延伸）
            var lastItem = Data[Data.Count - 1];
            float finalBeatInSegment = (remainingMs / 1000f) * (lastItem.Bpm / 60f);

            return lastItem.StartBeat.ToFloat() + finalBeatInSegment;
        }
    }
}
