using CyanStars.Framework.Pool;
using CyanStars.Framework.UI;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 谱面选择item数据
    /// </summary>
    public class MapItemData : IUIItemData, IReference
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
    }
}
