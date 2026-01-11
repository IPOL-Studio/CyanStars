#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CyanStars.Chart
{
    // 如果有人在非压力测试场景中使用了超过 200 个 BPM 组
    // 请在 github 开启一个 issue 并提供用例
    // 帮助进行基准测试以改进性能

    /// <summary>
    /// 用于增删改查和计算 Bpm List 的静态工具类
    /// </summary>
    public static class BpmGroupHelper
    {
        private static readonly Comparison<BpmGroupItem> Comparison = (a, b) => a.StartBeat.CompareTo(b.StartBeat);

        /// <summary>
        /// 由 Beat 计算时间（ms）的委托
        /// </summary>
        public delegate int BeatToTimeDelegate(IList<BpmGroupItem> datas, Beat beat);

        public enum BpmValidationStatus
        {
            Valid = 0, // 已排序且合规的列表，可以直接用
            Unsorted = 1, // 合规但无序的列表，使用前要排序
            Invalid = 2 // Bpm 时间点重复 / 缺少 0 beat 元素 / 有 beat 小于 0 / 有 null
        }

        /// <summary>
        /// 校验给定的 BPM List 是否合法
        /// </summary>
        /// <seealso cref="BpmValidationStatus"/>
        /// <param name="datas">需要检查的 BPM 列表</param>
        /// <returns>校验结果（有效有序/有效无序/无效）</returns>
        public static BpmValidationStatus Validate(IList<BpmGroupItem> datas)
        {
            if (datas == null || datas.Count == 0)
                return BpmValidationStatus.Invalid;

            bool isSorted = true;
            bool hasZeroStart = false;

            // 用于检测重复的时间点
            HashSet<Beat> visitedBeats = new HashSet<Beat>();

            Beat lastBeat = default;

            for (int i = 0; i < datas.Count; i++)
            {
                BpmGroupItem item = datas[i];

                // 检查元素是否为空
                if (item == null)
                    return BpmValidationStatus.Invalid;

                Beat currentBeat = item.StartBeat;

                // 检查 Beat 是否小于 0
                if (currentBeat.IntegerPart < 0)
                    return BpmValidationStatus.Invalid;

                // 检查是否有重复的时间点
                Beat simplified = currentBeat.Simplify();
                if (!visitedBeats.Add(simplified))
                    return BpmValidationStatus.Invalid;

                // 检查是否存在 0 beat 起始点
                if (simplified.IntegerPart == 0 && simplified.Numerator == 0)
                    hasZeroStart = true;

                // 检查顺序
                if (isSorted && i > 0)
                {
                    // CompareTo 返回值小于 0 表示 currentBeat < lastBeat
                    if (currentBeat.CompareTo(lastBeat) < 0)
                    {
                        isSorted = false;
                    }
                }

                lastBeat = currentBeat;
            }

            // 最终检查是否包含 0 beat
            if (!hasZeroStart)
                return BpmValidationStatus.Invalid;

            // 如果没有触发 Invalid，则根据是否保持了顺序返回结果
            return isSorted ? BpmValidationStatus.Valid : BpmValidationStatus.Unsorted;
        }

        /// <summary>
        /// 对 BPM List 进行按时间（StartBeat）升序排序
        /// </summary>
        /// <remarks>
        /// 排序过程保留 Beat 的原始分数形式，不会进行约分。
        /// </remarks>
        /// <param name="datas">需要排序的 BPM 列表</param>
        public static void Sort(IList<BpmGroupItem> datas)
        {
            var status = Validate(datas);

            if (status == BpmValidationStatus.Valid)
            {
                Debug.LogWarning("列表已经有序了！");
                return; // 已经有序直接返回，避免触发 Change 事件
            }
            else if (status == BpmValidationStatus.Invalid)
            {
                throw new Exception("Bpm List 数据不正确，无法排序");
            }

            // 针对 List<T> 和 Array [] 进行优化，直接使用原生的 Sort
            if (datas is List<BpmGroupItem> standardList)
            {
                standardList.Sort(Comparison);
            }
            else if (datas is BpmGroupItem[] standardArray)
            {
                Array.Sort(standardArray, Comparison);
            }
            else
            {
                // 对于 ObservableList 或其他 IList 实现
                // 由于 IList 接口没有 Sort 方法，采用“复制-排序-回写”的策略
                // 这样既能利用 List 的优化排序，又能兼容 ObservableList
                List<BpmGroupItem> temp = new List<BpmGroupItem>(datas);
                temp.Sort(Comparison);

                // 回写数据
                for (int i = 0; i < datas.Count; i++)
                {
                    // 仅当引用不同时赋值，减少 ObservableList 可能触发的 Notify 事件
                    if (datas[i] != temp[i])
                    {
                        datas[i] = temp[i];
                    }
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CalculateMsDurationInSegment(float fStartBeat, float fEndBeat, float bpm)
        {
            return (fEndBeat - fStartBeat) * (60 / bpm) * 1000f;
        }

        /// <summary>
        /// 将 beat 转换为毫秒时间（不含 offset）
        /// </summary>
        /// <param name="datas">基于此 Bpm List 进行计算</param>
        /// <param name="beat">Beat 形式的拍子</param>
        /// <returns>int 形式的毫秒时间（相对于时间轴开始）</returns>
        public static int CalculateTime(IList<BpmGroupItem> datas, Beat beat)
        {
            return CalculateTime(datas, beat.ToFloat());
        }

        /// <summary>
        /// 将 beat 转换为毫秒时间（不含 offset）
        /// </summary>
        /// <param name="datas">基于此 Bpm List 进行计算</param>
        /// <param name="floatBeat">float 形式的拍子</param>
        /// <returns>int 形式的毫秒时间（相对于时间轴开始）</returns>
        public static int CalculateTime(IList<BpmGroupItem> datas, float floatBeat)
        {
            if (Validate(datas) != BpmValidationStatus.Valid)
                throw new Exception("列表不合法，无法计算！");

            if (datas.Count == 1)
                return (int)(60 / datas[0].Bpm * floatBeat * 1000);

            double sumTime = 0;
            for (int i = 0; i < datas.Count - 1; i++)
            {
                var cur = datas[i];
                var next = datas[i + 1];
                if (floatBeat < next.StartBeat.ToFloat())
                {
                    // fBeat 落在当前 bpm 组中
                    sumTime += CalculateMsDurationInSegment(cur.StartBeat.ToFloat(), floatBeat, cur.Bpm);
                    return (int)sumTime;
                }

                sumTime += CalculateMsDurationInSegment(cur.StartBeat.ToFloat(), next.StartBeat.ToFloat(), cur.Bpm);
            }

            // fBeat 落在最后的 bpm 组中
            var last = datas[datas.Count - 1];
            sumTime += CalculateMsDurationInSegment(last.StartBeat.ToFloat(), floatBeat, last.Bpm);
            return (int)sumTime;
        }

        /// <summary>
        /// 将毫秒时间转换为浮点 beat
        /// </summary>
        /// <param name="datas">基于此 Bpm List 进行计算</param>
        /// <param name="msTime">已经计算 offset 后的毫秒时间（由 offset = 0 后开始计算）</param>
        /// <returns>float 形式的拍子</returns>
        public static float CalculateBeat(IList<BpmGroupItem> datas, int msTime)
        {
            if (datas.Count == 0)
            {
                return 0f;
            }

            // 如果只有一个 BPM 组，直接转换
            if (datas.Count == 1)
            {
                return (msTime / 1000f) * (datas[0].Bpm / 60f);
            }

            float remainingMs = msTime;

            // 遍历除了最后一个之外的所有 BPM 组
            for (int i = 0; i < datas.Count - 1; i++)
            {
                var currentItem = datas[i];
                var nextItem = datas[i + 1];

                // 计算当前 BPM 段的持续时间
                float timeDuration = CalculateMsDurationInSegment(currentItem.StartBeat.ToFloat(), nextItem.StartBeat.ToFloat(), currentItem.Bpm);

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
            var lastItem = datas[datas.Count - 1];
            float finalBeatInSegment = (remainingMs / 1000f) * (lastItem.Bpm / 60f);

            return lastItem.StartBeat.ToFloat() + finalBeatInSegment;
        }


        /// <summary>
        /// 尝试按 bpmItem 的 Beat 在当前列表中有序插入
        /// </summary>
        /// <param name="datas">要操作的 Bpm List</param>
        /// <param name="newItem">要插入的 Item</param>
        /// <returns>是否成功执行了插入操作</returns>
        /// <exception cref="Exception">传入的列表不合法，你应该在调用此方法前验证或确保列表有效</exception>
        public static bool TryInsertItem(IList<BpmGroupItem> datas, BpmGroupItem newItem)
        {
            if (Validate(datas) != BpmValidationStatus.Valid)
                throw new Exception("给定的 Bpm List 不合法！");

            if (newItem.StartBeat.ToFloat() <= 0f)
                return false;

            if (datas.Count == 1)
            {
                datas.Insert(1, newItem);
                return true;
            }

            for (int i = 0; i < datas.Count - 2; i++)
            {
                if (Mathf.Approximately(datas[i].StartBeat.ToFloat(), newItem.StartBeat.ToFloat()) ||
                    Mathf.Approximately(newItem.StartBeat.ToFloat(), datas[i + 1].StartBeat.ToFloat()))
                    return false;

                if (datas[i].StartBeat.ToFloat() < newItem.StartBeat.ToFloat()
                    && newItem.StartBeat.ToFloat() < datas[i + 1].StartBeat.ToFloat())
                {
                    datas.Insert(i + 1, newItem);
                    return true;
                }
            }

            datas.Add(newItem);
            return true;
        }
    }
}
