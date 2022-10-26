using System.Threading.Tasks;
using UnityEngine;

namespace CyanStars.Framework.GameObjectPool
{
    public partial class GameObjectPoolManager
    {
        /// <summary>
        /// 从池中获取一个游戏对象（可等待）
        /// </summary>
        public Task<GameObject> AwaitGetGameObject(string prefabName, Transform parent)
        {
            return CatAsset.Runtime.GameObjectPoolManager.AwaitGetGameObject(prefabName, parent);
        }

        /// <summary>
        /// 从池中获取一个游戏对象（可等待）
        /// </summary>
        public Task<GameObject> AwaitGetGameObject(GameObject template, Transform parent)
        {
            return CatAsset.Runtime.GameObjectPoolManager.AwaitGetGameObject(template, parent);
        }
    }
}
