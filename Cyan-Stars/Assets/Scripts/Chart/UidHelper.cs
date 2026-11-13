#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace CyanStars.Chart
{
    /// <summary>
    /// 谱包与谱面 UID 的生成、校验和分类工具类
    /// </summary>
    public static class UidHelper
    {
        /// <summary>
        /// 生成谱包 UID
        /// </summary>
        /// <returns>32 位小写十六进制 UUID，不含连字符</returns>
        [Pure]
        public static string GenerateChartPackUid()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// 生成谱面 UID
        /// </summary>
        /// <returns>7 位随机字符串加 "_v0"</returns>
        /// <remarks>随机部分存在小概率碰撞风险，业务方自行处理，可以做个文件存在性检验。</remarks>
        [Pure]
        public static string GenerateChartUid()
        {
            string randomPart = Guid.NewGuid().ToString("N").Substring(0, 7);
            return $"{randomPart}_v0";
        }

        /// <summary>
        /// 将谱包按 UID 分为唯一组和冲突组
        /// </summary>
        /// <param name="chartPacks">待分类的谱包</param>
        /// <param name="passChartPacks">唯一 UID 谱包，原内容会被清空</param>
        /// <param name="failChartPacks">空或冲突 UID 谱包，原内容会被清空</param>
        public static void ClassifyChartPacks(
            HashSet<RuntimeChartPack> chartPacks,
            HashSet<RuntimeChartPack> passChartPacks,
            HashSet<RuntimeChartPack> failChartPacks
        )
        {
            ClassifyByUid(
                chartPacks,
                static chartPack => chartPack.ChartPackData.Uid,
                passChartPacks,
                failChartPacks
            );
        }

        /// <summary>
        /// 将同一谱包的谱面元数据按 UID 分为唯一组和冲突组
        /// </summary>
        /// <param name="chartMetaDatas">待分类的谱面元数据</param>
        /// <param name="passChartMetaDatas">唯一 UID 谱面，原内容会被清空</param>
        /// <param name="failChartMetaDatas">空或冲突 UID 谱面，原内容会被清空</param>
        public static void ClassifyChartMetaDatas(
            HashSet<ChartMetaData> chartMetaDatas,
            HashSet<ChartMetaData> passChartMetaDatas,
            HashSet<ChartMetaData> failChartMetaDatas
        )
        {
            ClassifyByUid(
                chartMetaDatas,
                static chartMetaData => chartMetaData.Uid,
                passChartMetaDatas,
                failChartMetaDatas
            );
        }

        /// <summary>
        /// 从给定的集合中提取 UID，并校验和分类集合元素
        /// </summary>
        /// <param name="items">要分类的集合</param>
        /// <param name="getUid">从每个集合元素中获取 UID 的方法</param>
        /// <param name="passItems">通过校验的元素集合，原内容会被清空</param>
        /// <param name="failItems">未通过校验的元素集合，原内容会被清空</param>
        private static void ClassifyByUid<T>(
            HashSet<T> items,
            Func<T, string> getUid,
            HashSet<T> passItems,
            HashSet<T> failItems
        )
        {
            passItems.Clear();
            failItems.Clear();

            if (items.Count == 0)
                return;

            var tracker = new Dictionary<string, (T FirstItem, bool HasConflict)>(items.Count);

            foreach (T item in items)
            {
                string uid = getUid(item);
                if (string.IsNullOrWhiteSpace(uid))
                {
                    failItems.Add(item);
                    continue;
                }

                // 首次出现
                if (!tracker.TryGetValue(uid, out var entry))
                {
                    tracker.Add(uid, (item, false));
                    passItems.Add(item);
                    continue;
                }

                // 第二次出现（冲突）：需要将首次加入 passItems 的元素回退转移到 failItems
                if (!entry.HasConflict)
                {
                    tracker[uid] = (entry.FirstItem, true);
                    passItems.Remove(entry.FirstItem);
                    failItems.Add(entry.FirstItem);
                }

                // 当前冲突元素加入 failItems
                failItems.Add(item);
            }
        }
    }
}
