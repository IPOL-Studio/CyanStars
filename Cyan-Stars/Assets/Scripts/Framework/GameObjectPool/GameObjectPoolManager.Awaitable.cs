using System.Threading.Tasks;
using UnityEngine;

namespace CyanStars.Framework.GameObjectPool
{
    public partial class GameObjectPoolManager
    {
        /// <summary>
        /// 从池中获取一个游戏对象（可等待）
        /// </summary>
        public Task<GameObject> GetGameObjectAsync(string prefabName, Transform parent)
        {
            return CatAsset.Runtime.GameObjectPoolManager.GetGameObjectAsync(prefabName, parent);
        }

        /// <summary>
        /// 从池中获取一个游戏对象（可等待）
        /// </summary>
        public Task<GameObject> GetGameObjectAsync(GameObject template, Transform parent)
        {
            return CatAsset.Runtime.GameObjectPoolManager.GetGameObjectAsync(template, parent);
        }
    }
}
