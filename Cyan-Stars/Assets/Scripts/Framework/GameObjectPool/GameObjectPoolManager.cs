using System;
using System.Collections;
using System.Collections.Generic;
using CyanStars.Framework;
using UnityEngine;

namespace CyanStars.Framework.GameObjectPool
{
    /// <summary>
    /// 游戏对象池管理器
    /// </summary>
    public class GameObjectPoolManager : BaseManager
    {
        /// <summary>
        /// 预制体名字->游戏对象池
        /// </summary>
        private Dictionary<string, GameObjectPool> poolDict = new Dictionary<string, GameObjectPool>();

        /// <summary>
        /// 默认对象失效时间
        /// </summary>
        [Header("默认对象失效时间")]
        public float DefaultExpireTime = 30;
        
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
        private Queue<ValueTuple<GameObject, Action<GameObject>>> waitInstantiateQueue =
            new Queue<(GameObject, Action<GameObject>)>();

        public override int Priority { get; }
        
        public override void OnInit()
        {
            
        }

        public override void OnUpdate(float deltaTime)
        {
            //轮询池子
            foreach (KeyValuePair<string,GameObjectPool> item in poolDict)
            {
                item.Value.OnUpdate(deltaTime);
            }
            
            //处理分帧实例化
            while (instantiateCounter < MaxInstantiateCount && waitInstantiateQueue.Count > 0)
            {
                var (prefab, callback) = waitInstantiateQueue.Dequeue();
                callback?.Invoke(Instantiate(prefab));
                instantiateCounter++;
                Debug.Log($"实例化了游戏对象：{prefab.name}，当前帧已实例化数：{instantiateCounter}");
            }

            instantiateCounter = 0;
        }

        /// <summary>
        /// 从池中获取一个游戏对象
        /// </summary>
        public void GetGameObject(string prefabName, Action<GameObject> callback)
        {
            if (!poolDict.TryGetValue(prefabName,out GameObjectPool pool))
            {
                pool = new GameObjectPool(prefabName, DefaultExpireTime);
                poolDict.Add(prefabName,pool);
            }
            pool.GetGameObject(callback);
        }

        /// <summary>
        /// 将游戏对象归还池中
        /// </summary>
        public void ReleaseGameObject(string prefabName,GameObject go)
        {
            if (!poolDict.TryGetValue(prefabName,out GameObjectPool pool))
            {
                return;
            }
            
            pool.ReleaseGameObject(go);
            
            go.SetActive(false);
            go.transform.SetParent(transform);
        }
        
        /// <summary>
        /// 分帧异步实例化
        /// </summary>
        public void InstantiateAsync(GameObject prefab, Action<GameObject> callback)
        {
            waitInstantiateQueue.Enqueue((prefab,callback));
        }
    }
}

