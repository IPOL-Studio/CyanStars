// TODO: TO DE DELETED
using System;
using CatAsset.Runtime;

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
        public MapManifest MapManifest {get; private set; }

        public static MapItemData Create(int index, MapManifest mapManifest)
        {
            MapItemData data = ReferencePool.Get<MapItemData>();
            data.Index = index;
            data.MapManifest = mapManifest;
            return data;
        }

        public void Clear()
        {
            Index = default;
            MapManifest = default;
        }

        public bool Equals(MapItemData other)
        {
            if (other is null)
                return false;

            return this.Index == other.Index && this.MapManifest.Equals(other.MapManifest);
        }

        public override bool Equals(object obj)
        {
            return obj is MapItemData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Index ^ 23 ^ MapManifest.GetHashCode();
        }
    }
}
