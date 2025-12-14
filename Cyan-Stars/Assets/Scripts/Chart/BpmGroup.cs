using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace CyanStars.Chart
{
    // 如果有人在非压力测试场景中使用了超过 200 个 BPM 组
    // 请在 github 开启一个 issue 并提供用例
    // 帮助进行基准测试以改进性能
    public class BpmGroup
    {
        private SortedList<Beat, BpmGroupItem> data = new SortedList<Beat, BpmGroupItem>(16);

        private ReadOnlyCollection<BpmGroupItem> readOnlyData;
        public IReadOnlyList<BpmGroupItem> Data => readOnlyData ??= new ReadOnlyCollection<BpmGroupItem>(data.Values);


        /// <summary>
        /// 由 Beat 组计算时间（ms）的委托
        /// </summary>
        public delegate int BeatToTimeDelegate(Beat beat);


        public void AddRange(IEnumerable<BpmGroupItem> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                _ = item ?? throw new ArgumentException("BpmGroupItem collection cannot contain null elements.", nameof(items));
                data.Add(item.StartBeat.Simplify(), item);
            }
        }

        public void Add(BpmGroupItem item)
        {
            _ = item ?? throw new ArgumentNullException(nameof(item));
            data.Add(item.StartBeat.Simplify(), item);
        }

        public bool Remove(Beat item)
        {
            return data.Remove(item.Simplify());
        }

        public void Clear()
        {
            data.Clear();
        }

        public int IndexOf(Beat item)
        {
            return data.IndexOfKey(item.Simplify());
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
            if (Data.Count == 0)
                throw new InvalidOperationException("BpmGroup Data 元素为空，无法计算 Beat 对应的时间");

            if (Data.Count == 1)
                return (int)(60 / Data[0].Bpm * fBeat * 1000);

            double sumTime = 0;
            for (int i = 0; i < Data.Count - 1; i++)
            {
                var cur = Data[i];
                var next = Data[i + 1];
                if (fBeat < next.StartBeat.ToFloat())
                {
                    // fBeat 落在当前 bpm 组中
                    sumTime += CalculateMsDurationInSegment(cur.StartBeat.ToFloat(), fBeat, cur.Bpm);
                    return (int)sumTime;
                }

                sumTime += CalculateMsDurationInSegment(cur.StartBeat, next.StartBeat, cur.Bpm);
            }

            // fBeat 落在最后的 bpm 组中
            var last = Data[Data.Count - 1];
            sumTime += CalculateMsDurationInSegment(last.StartBeat.ToFloat(), fBeat, last.Bpm);
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

                // 计算当前 BPM 段的持续时间
                float timeDuration = CalculateMsDurationInSegment(currentItem.StartBeat, nextItem.StartBeat, currentItem.Bpm);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalculateMsDurationInSegment(Beat start, Beat end, float bpm)
        {
            return CalculateMsDurationInSegment(start.ToFloat(), end.ToFloat(), bpm);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalculateMsDurationInSegment(float fStartBeat, float fEndBeat, float bpm)
        {
            return (fEndBeat - fStartBeat) * (60 / bpm) * 1000f;
        }
    }
}
