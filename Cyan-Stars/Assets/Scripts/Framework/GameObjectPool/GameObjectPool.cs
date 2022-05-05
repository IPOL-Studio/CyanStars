using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CyanStars.Framework.GameObjectPool
{
    /// <summary>
    /// 游戏对象池
    /// </summary>
    public class GameObjectPool
    {
        /// <summary>
        /// 预制体名字
        /// </summary>
        private string prefabName;

        /// <summary>
        /// 预制体
        /// </summary>
        private GameObject prefab;
        

        /// <summary>
        /// 对象失效时间
        /// </summary>
        private float expireTime;

        /// <summary>
        /// 游戏对象->池对象
        /// </summary>
        private Dictionary<GameObject, PoolObject> poolObjectDict = new Dictionary<GameObject, PoolObject>();

        /// <summary>
        /// 未被使用的池对象列表
        /// </summary>
        private List<PoolObject> unusedPoolObjectList = new List<PoolObject>();


        public GameObjectPool(string prefabName, float expireTime)
        {
            this.prefabName = prefabName;
            this.expireTime = expireTime;
        }


        /// <summary>
        /// 轮询游戏对象池
        /// </summary>
        public void OnUpdate(float deltaTime)
        {
            for (int i = unusedPoolObjectList.Count - 1; i >= 0; i--)
            {
                PoolObject poolObject = unusedPoolObjectList[i];
                poolObject.UnusedTimer += deltaTime;
                if (poolObject.UnusedTimer >= expireTime && !poolObject.IsLock)
                {
                    poolObjectDict.Remove(poolObject.Target);
                    unusedPoolObjectList.RemoveAt(i);
                    
                    poolObject.Destroy();
                }
            }
        }

        
        /// <summary>
        /// 销毁游戏对象池
        /// </summary>
        public void OnDestroy()
        {
            Clear();
            
            GameRoot.Asset.UnloadAsset(prefab);
            prefab = null;
        }

        /// <summary>
        /// 清空游戏对象池
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < unusedPoolObjectList.Count; i++)
            {
                unusedPoolObjectList[i].Destroy();
            }
            unusedPoolObjectList.Clear();
            poolObjectDict.Clear();
        }

        /// <summary>
        /// 锁定游戏对象，被锁定后不会被销毁
        /// </summary>
        public void LockGameObject(GameObject go,bool isLock = true)
        {
            if (!poolObjectDict.TryGetValue(go,out PoolObject poolObject))
            {
                return;
            }

            poolObject.IsLock = true;
        }
        
        /// <summary>
        /// 从池中获取一个游戏对象
        /// </summary>
        public void GetGameObject(Transform parent, Action<GameObject> callback)
        {
            if (prefab == null)
            {
                //预制体未加载，加载预制体
                GameRoot.Asset.LoadAsset(prefabName, (success, asset) =>
                {
                    prefab = (GameObject) asset;
                    GetGameObject(parent,callback);
                });
                return;
            }

            if (unusedPoolObjectList.Count == 0)
            {
                //没有未使用的池对象，需要实例化出来
                GameRoot.GameObjectPool.InstantiateAsync(prefab,parent, (go) =>
                {
                    PoolObject poolObject = new PoolObject
                    {
                        Target = go,
                        Used = true
                    };
                    
                    poolObjectDict.Add(go,poolObject);
                    
                    go.SetActive(true);
                    callback?.Invoke(go);
                });

                return;
            }

            //从未被使用的池对象中拿一个出来
            PoolObject poolObject = unusedPoolObjectList[unusedPoolObjectList.Count - 1];
            unusedPoolObjectList.RemoveAt(unusedPoolObjectList.Count - 1);
            poolObject.Target.transform.SetParent(parent);
            poolObject.Target.SetActive(true);
            callback?.Invoke(poolObject.Target);
        }

        /// <summary>
        /// 将游戏对象归还池中
        /// </summary>
        public void ReleaseGameObject(GameObject go)
        {
            if (!poolObjectDict.TryGetValue(go,out PoolObject poolObject))
            {
                return;
            }

            poolObject.Used = false;
            poolObject.UnusedTimer = 0;
            go.SetActive(false);
            
            unusedPoolObjectList.Add(poolObject);
        }
        
    }
}