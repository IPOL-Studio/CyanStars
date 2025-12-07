using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CyanStars.Framework.GameObjectPool
{
    /// <summary>
    /// 游戏对象池管理器
    /// </summary>
    public class GameObjectPoolManager : BaseManager
    {
        /// <summary>
        /// 游戏对象池管理器的根节点
        /// </summary>
        [Header("对象池根节点")]
        public Transform Root;

        /// <summary>
        /// 默认对象失效时间
        /// </summary>
        [Header("默认对象失效时间")]
        public float DefaultObjectExpireTime = 60;

        /// <summary>
        /// 默认对象池失效时间
        /// </summary>
        [Header("默认对象池失效时间")]
        public float DefaultPoolExpireTime = 60;

        /// <summary>
        /// 单帧最大实例化数
        /// </summary>
        [Header("单帧最大实例化数")]
        public int MaxInstantiateCount = 10;

        public override int Priority { get; }

        public override void OnInit()
        {
            CatAsset.Runtime.GameObjectPoolManager.Root = Root;

            CatAsset.Runtime.GameObjectPoolManager.DefaultObjectExpireTime = DefaultObjectExpireTime;
            CatAsset.Runtime.GameObjectPoolManager.DefaultPoolExpireTime = DefaultPoolExpireTime;
            CatAsset.Runtime.GameObjectPoolManager.MaxInstantiateCount = MaxInstantiateCount;
        }

        public override void OnUpdate(float deltaTime)
        {
            CatAsset.Runtime.GameObjectPoolManager.Update(deltaTime);
        }

         /// <summary>
        /// 使用预制体名从池中获取一个游戏对象
        /// </summary>
        public Task<GameObject> GetGameObjectAsync(string prefabName, Transform parent,
                                                   CancellationToken cancellationToken = default)
        {
            return CatAsset.Runtime.GameObjectPoolManager.GetAsync(prefabName,parent,cancellationToken);
        }

        /// <summary>
        /// 使用模板中从池中获取一个游戏对象
        /// </summary>
        public Task<GameObject> GetGameObjectAsync(GameObject template, Transform parent, CancellationToken cancellationToken = default)
        {
            return CatAsset.Runtime.GameObjectPoolManager.GetAsync(template,parent,cancellationToken);
        }

        /// <summary>
        ///  使用预制体名将游戏对象归还池中
        /// </summary>
        public void ReleaseGameObject(string prefabName, GameObject go)
        {
            CatAsset.Runtime.GameObjectPoolManager.Release(prefabName, go);
        }

        /// <summary>
        /// 使用模板将游戏对象归还池中
        /// </summary>
        public void ReleaseGameObject(GameObject template, GameObject go)
        {
            CatAsset.Runtime.GameObjectPoolManager.Release(template,go);
        }

        /// <summary>
        /// 预热对象
        /// </summary>
        public void PrewarmAsync(string prefabName,int count,Action callback)
        {
            CatAsset.Runtime.GameObjectPoolManager.PrewarmAsync(prefabName, count, callback);
        }
    }
}
