using System;
using System.Collections.Generic;
using UnityEngine;
using CyanStars.Framework.Asset;

namespace CyanStars.Framework.GameObjectPool
{
    /// <summary>
    /// 游戏对象池管理器
    /// </summary>
    public class GameObjectPoolManager : BaseManager
    {
        /// <summary>
        /// 预制体名字->加载好的预制体
        /// </summary>
        private Dictionary<string, GameObject> loadedPrefabDict = new Dictionary<string, GameObject>();

        /// <summary>
        /// 模板->对象池
        /// </summary>
        private Dictionary<GameObject, GameObjectPool> poolDict = new Dictionary<GameObject, GameObjectPool>();


        /// <summary>
        /// 默认对象失效时间
        /// </summary>
        [Header("默认对象失效时间")]
        public float DefaultObjectExpireTime = 30;

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

        /// <summary>
        /// 单帧实例化计数器
        /// </summary>
        private int instantiateCounter;

        /// <summary>
        /// 等待实例化的游戏对象队列
        /// </summary>
        private Queue<ValueTuple<GameObject, Transform, Action<GameObject>>> waitInstantiateQueue = new Queue<(GameObject, Transform, Action<GameObject>)>();

        /// <summary>
        /// 等待卸载的预制体名字列表
        /// </summary>
        private List<string> waitUnloadPrefabNames = new List<string>();

        /// <inheritdoc />
        public override int Priority { get; }

        /// <inheritdoc />
        public override void OnInit()
        {
        }

        /// <inheritdoc />
        public override void OnUpdate(float deltaTime)
        {
            //轮询池子
            foreach (KeyValuePair<GameObject, GameObjectPool> pair in poolDict)
            {
                pair.Value.OnUpdate(deltaTime);
            }

            //销毁长时间未使用的，且是由对象池管理器加载了预制体资源的对象池
            foreach (KeyValuePair<string, GameObject> pair in loadedPrefabDict)
            {
                GameObjectPool pool = poolDict[pair.Value];
                if (pool.UnusedTimer > DefaultPoolExpireTime)
                {
                    waitUnloadPrefabNames.Add(pair.Key);
                }
            }

            foreach (string prefabName in waitUnloadPrefabNames)
            {
                DestroyPool(prefabName);
            }

            waitUnloadPrefabNames.Clear();

            //处理分帧实例化
            while (instantiateCounter < MaxInstantiateCount && waitInstantiateQueue.Count > 0)
            {
                var (prefab, parent, callback) = waitInstantiateQueue.Dequeue();
                callback?.Invoke(Instantiate(prefab, parent));
                instantiateCounter++;
                Debug.Log($"实例化了游戏对象：{prefab.name}，当前帧已实例化数量：{instantiateCounter}");
            }

            instantiateCounter = 0;
        }

        /// <summary>
        /// 使用预制体名从池中获取一个游戏对象
        /// </summary>
        public void GetGameObject(string prefabName, Transform parent, Action<GameObject> callback)
        {
            if (loadedPrefabDict.ContainsKey(prefabName))
            {
                GetGameObject(loadedPrefabDict[prefabName], parent, callback);
                return;
            }

            //此prefab未加载过，先加载
            GameRoot.Asset.LoadAsset(prefabName, (success, obj) =>
            {
                if (!success)
                {
                    return;
                }

                if (!(obj is GameObject prefab))
                {
                    GameRoot.Asset.UnloadAsset(obj);
                    Debug.LogError($"GetGameObject调用失败,{prefabName}不是一个GameObject");
                    return;
                }

                GetGameObject(prefab, parent, callback);

                //进行prefab资源绑定
                //这里还得再进行一次判断，因为如果一帧调用多次GetGameObject的话是会多次回调到这里的
                if (!loadedPrefabDict.ContainsKey(prefabName))
                {
                    GameObject root = poolDict[prefab].Root.gameObject;
                    AssetBinder assetBinder = root.AddComponent<AssetBinder>();
                    assetBinder.BindTo(prefab);
                    loadedPrefabDict.Add(prefabName, prefab);
                }
                else
                {
                    Debug.LogError(prefabName + "重复调用了LoadAsset");
                }
            });

        }

        /// <summary>
        /// 使用模板中从池中获取一个游戏对象
        /// </summary>
        public void GetGameObject(GameObject template, Transform parent, Action<GameObject> callback)
        {
            if (!poolDict.TryGetValue(template, out GameObjectPool pool))
            {
                GameObject root = new GameObject($"Pool-{template.name}");
                root.transform.SetParent(transform);

                pool = new GameObjectPool(template, DefaultObjectExpireTime, root.transform);
                poolDict.Add(template, pool);
            }

            pool.GetGameObject(parent, callback);
        }

        /// <summary>
        ///  使用预制体名将游戏对象归还池中
        /// </summary>
        public void ReleaseGameObject(string prefabName, GameObject go)
        {
            if (!loadedPrefabDict.ContainsKey(prefabName))
            {
                return;
            }

            ReleaseGameObject(loadedPrefabDict[prefabName], go);
        }

        /// <summary>
        /// 使用模板将游戏对象归还池中
        /// </summary>
        public void ReleaseGameObject(GameObject template, GameObject go)
        {
            if (!poolDict.TryGetValue(template, out GameObjectPool pool))
            {
                return;
            }

            pool.ReleaseGameObject(go);
        }

        /// <summary>
        /// 销毁对象池
        /// </summary>
        public void DestroyPool(string prefabName)
        {
            if (!loadedPrefabDict.ContainsKey(prefabName))
            {
                return;
            }

            DestroyPool(loadedPrefabDict[prefabName]);

            loadedPrefabDict.Remove(prefabName);
        }

        /// <summary>
        /// 销毁对象池
        /// </summary>
        public void DestroyPool(GameObject template)
        {
            if (!poolDict.TryGetValue(template, out GameObjectPool pool))
            {
                return;
            }

            pool.OnDestroy();
            poolDict.Remove(template);
            Debug.Log($"{template.name}的对象池被销毁了");
        }


        /// <summary>
        /// 分帧异步实例化
        /// </summary>
        public void InstantiateAsync(GameObject prefab, Transform parent, Action<GameObject> callback)
        {
            waitInstantiateQueue.Enqueue((prefab, parent, callback));
        }
    }
}
