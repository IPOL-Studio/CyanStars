using System;
using CatAsset.Runtime;
using CyanStars.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 谱面选择item数据
    /// </summary>
    public class MapItemData :  IReference, IEquatable<MapItemData>
    {
        /// <summary>
        /// 索引
        /// </summary>
        public int Index {get; private set; }

        /// <summary>
        /// 谱面清单
        /// </summary>
        public ChartPack ChartPack {get; private set; }

        public static MapItemData Create(int index, ChartPack chartPack)
        {
            MapItemData data = ReferencePool.Get<MapItemData>();
            data.Index = index;
            data.ChartPack = chartPack;
            return data;
        }

        public void Clear()
        {
            Index = default;
            ChartPack = default;
        }

        public bool Equals(MapItemData other)
        {
            if (other is null)
                return false;

            return this.Index == other.Index && this.ChartPack.Equals(other.ChartPack);
        }

        public override bool Equals(object obj)
        {
            return obj is MapItemData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Index ^ 23 ^ ChartPack.GetHashCode();
        }
    }
}
