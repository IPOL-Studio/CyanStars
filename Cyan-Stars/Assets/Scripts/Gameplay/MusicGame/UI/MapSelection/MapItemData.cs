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
        public RuntimeChartPack RuntimeChartPack {get; private set; }

        public static MapItemData Create(int index, RuntimeChartPack runtimeChartPack)
        {
            MapItemData data = ReferencePool.Get<MapItemData>();
            data.Index = index;
            data.RuntimeChartPack = runtimeChartPack;
            return data;
        }

        public void Clear()
        {
            Index = default;
            RuntimeChartPack = default;
        }

        public bool Equals(MapItemData other)
        {
            if (other is null)
                return false;

            return this.Index == other.Index && this.RuntimeChartPack.Equals(other.RuntimeChartPack);
        }

        public override bool Equals(object obj)
        {
            return obj is MapItemData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Index ^ 23 ^ RuntimeChartPack.GetHashCode();
        }
    }
}
