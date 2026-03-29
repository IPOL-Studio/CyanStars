#nullable enable

using System;
using CatAsset.Runtime;
using CyanStars.Chart;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 谱面选择item数据
    /// </summary>
    public class MapItemData : IReference
    {
        /// <summary>
        /// 索引
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// 谱面清单
        /// </summary>
        public RuntimeChartPack? RuntimeChartPack { get; private set; }


        [Obsolete("构造函数仅限引用池调用，实例化时请使用 Create()", true)]
        public MapItemData()
        {
        }

        public static MapItemData Create(int index, RuntimeChartPack runtimeChartPack)
        {
            MapItemData data = ReferencePool.Get<MapItemData>();
            data.Index = index;
            data.RuntimeChartPack = runtimeChartPack;
            return data;
        }

        public void Clear()
        {
            Index = 0;
            RuntimeChartPack = null;
        }
    }
}
