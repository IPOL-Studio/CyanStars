// TODO: 已重构，待测试
using System;
using CatAsset.Runtime;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 谱面选择item数据
    /// </summary>
    public class ChartPackItemData :  IReference, IEquatable<ChartPackItemData>
    {
        /// <summary>
        /// 索引
        /// </summary>
        public int Index {get; private set; }

        /// <summary>
        /// 谱包清单
        /// </summary>
        public ChartPack ChartPack {get; private set; }

        public static ChartPackItemData Create(int index, ChartPack chartPack)
        {
            ChartPackItemData data = ReferencePool.Get<ChartPackItemData>();
            data.Index = index;
            data.ChartPack = chartPack;
            return data;
        }

        public void Clear()
        {
            Index = 0;
            ChartPack = null;
        }

        public bool Equals(ChartPackItemData other)
        {
            if (other is null)
                return false;

            return this.Index == other.Index && this.ChartPack.Equals(other.ChartPack);
        }

        public override bool Equals(object obj)
        {
            return obj is ChartPackItemData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Index ^ 23 ^ ChartPack.GetHashCode();
        }
    }
}
