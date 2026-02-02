#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Chart
{
    /// <summary>
    /// 用于增删改查 BaseChartNoteData List 的静态工具类
    /// </summary>
    public static class NoteListHelper
    {
        private static readonly Comparison<BaseChartNoteData> Comparison = (a, b) => a.JudgeBeat.CompareTo(b.JudgeBeat);

        public enum NoteValidationStatus
        {
            Valid = 0, // 已排序且合规的列表
            Unsorted = 1, // 合规但无序的列表
            Invalid = 2 // 有 null 元素 / 有 beat 小于 0
        }

        /// <summary>
        /// 校验给定的 Note List 是否合法
        /// </summary>
        /// <param name="Notes">需要检查的 Note 列表</param>
        /// <returns>校验结果</returns>
        public static NoteValidationStatus Validate(IList<BaseChartNoteData> Notes)
        {
            if (Notes == null)
                return NoteValidationStatus.Invalid;

            if (Notes.Count == 0)
                return NoteValidationStatus.Valid;

            bool isSorted = true;
            Beat lastBeat = default;

            // 首个元素无需和前一个元素比较时间
            bool isFirst = true;

            foreach (var note in Notes)
            {
                // 检查元素是否为空
                if (note == null)
                    return NoteValidationStatus.Invalid;

                Beat currentBeat = note.JudgeBeat;

                // 检查 Beat 是否小于 0
                if (currentBeat.IntegerPart < 0)
                    return NoteValidationStatus.Invalid;

                // 检查顺序
                if (!isFirst && isSorted)
                {
                    // Note 允许时间重复 (CompareTo == 0)，所以只有当 current < last 时才算乱序
                    if (currentBeat.CompareTo(lastBeat) < 0)
                    {
                        isSorted = false;
                    }
                }

                lastBeat = currentBeat;
                isFirst = false;
            }

            return isSorted ? NoteValidationStatus.Valid : NoteValidationStatus.Unsorted;
        }

        /// <summary>
        /// 对 Note List 进行按时间（JudgeBeat）升序排序
        /// </summary>
        /// <param name="datas">需要排序的 Note 列表</param>
        public static void Sort(IList<BaseChartNoteData> datas)
        {
            var status = Validate(datas);

            if (status == NoteValidationStatus.Valid)
            {
                Debug.LogWarning("列表已经有序了！");
                return;
            }
            else if (status == NoteValidationStatus.Invalid)
            {
                throw new Exception("Note List 数据不正确（包含 null 或负数时间），无法排序");
            }

            // 针对 List<T> 和 Array [] 进行优化
            if (datas is List<BaseChartNoteData> standardList)
            {
                standardList.Sort(Comparison);
            }
            else if (datas is BaseChartNoteData[] standardArray)
            {
                Array.Sort(standardArray, Comparison);
            }
            else
            {
                // 对于 ObservableList 或其他 IList 实现
                // 采用“复制-排序-回写”策略
                List<BaseChartNoteData> temp = new List<BaseChartNoteData>(datas);
                temp.Sort(Comparison);

                for (int i = 0; i < datas.Count; i++)
                {
                    // 仅当引用不同时赋值，减少可能的 Notify 事件
                    if (datas[i] != temp[i])
                    {
                        datas[i] = temp[i];
                    }
                }
            }
        }

        /// <summary>
        /// 尝试将新 Note 有序插入到列表中
        /// </summary>
        /// <param name="notes">目标列表</param>
        /// <param name="newItem">要插入的 Note</param>
        /// <returns>总是返回 true (除非列表本身非法)</returns>
        public static bool TryInsertItem(IList<BaseChartNoteData> notes, BaseChartNoteData newItem)
        {
            // 插入前先确保列表本身是合法的（可以是无序的，但不能有 invalid 数据，
            // 且为了保证插入后整体有序，通常期望输入列表是有序的，但此处仅做基础校验）
            if (notes == null || newItem == null)
                throw new Exception("列表或插入项不能为空");

            if (Validate(notes) != NoteValidationStatus.Valid)
                throw new Exception("给定的 Note List 不合法！");

            if (newItem.JudgeBeat.ToFloat() < 0)
                throw new Exception("插入项的时间不能小于 0");

            // 如果列表为空，直接添加
            if (notes.Count == 0)
            {
                notes.Add(newItem);
                return true;
            }

            float targetBeat = newItem.JudgeBeat.ToFloat();
            bool inserted = false;

            // 遍历查找第一个时间大于 newItem 的位置
            for (int i = 0; i < notes.Count; i++)
            {
                float currentBeat = notes[i].JudgeBeat.ToFloat();

                // 只要当前项的时间 > 插入项的时间，就插在当前项前面
                if (currentBeat > targetBeat)
                {
                    notes.Insert(i, newItem);
                    inserted = true;
                    break;
                }

                // 如果时间相等，选择插在该时间点所有 Note 的后面
                // 所以这里不处理 == 的情况，继续向后找，直到 > 或者列表结束
            }

            // 如果遍历完都没找到比它大的（或者列表里所有的都比它小/相等），则加到末尾
            if (!inserted)
            {
                notes.Add(newItem);
            }

            return true;
        }
    }
}
