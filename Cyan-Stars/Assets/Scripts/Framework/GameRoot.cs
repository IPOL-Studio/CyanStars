using System;
using System.Collections;
using System.Collections.Generic;
using CyanStars.Framework.Asset;
using CyanStars.Framework.FSM;
using UnityEngine;

namespace CyanStars.Framework
{
    /// <summary>
    /// 游戏根节点
    /// </summary>
    public class GameRoot : MonoBehaviour
    {
        /// <summary>
        /// 游戏管理器列表
        /// </summary>
        private static List<BaseManager> managers = new List<BaseManager>();

        /// <summary>
        /// 资源管理器
        /// </summary>
        public static AssetManager Asset;
        
        /// <summary>
        /// 有效状态机管理器
        /// </summary>
        public static FSMManager FSM;
        

        private async void Start()
        {
            //按优先级排序并初始化所有Manager
            managers.Sort((x, y) => x.Priority.CompareTo(y.Priority));
            for (int i = 0; i < managers.Count; i++)
            {
                managers[i].OnInit();
            }

            FSM = GetManager<FSMManager>();
            Asset = GetManager<AssetManager>();

#if UNITY_EDITOR
            if (Asset.IsEditorMode)
            {
                Asset.LoadScene("Assets/BundleRes/Scenes/Dark.unity", null);
                return;
            }
#endif
            bool success = await Asset.AwaitCheckPackageManifest();
            if (success)
            {
                Asset.LoadScene("Assets/BundleRes/Scenes/Dark.unity", null);
            }
            else
            {
                Debug.LogError("资源清单检查失败");
            }

        }

        private void Update()
        {
            //轮询所有Manager
            for (int i = 0; i < managers.Count; i++)
            {
                managers[i].OnUpdate(Time.deltaTime);
            }
        }

        /// <summary>
        /// 注册管理器
        /// </summary>
        public static void RegisterManager(BaseManager manager)
        {
            managers.Add(manager);
        }

        /// <summary>
        /// 获取指定类型的游戏管理器
        /// </summary>
        public static T GetManager<T>() where T : BaseManager
        {
            for (int i = 0; i < managers.Count; i++)
            {
                if (typeof(T) == managers[i].GetType())
                {
                    return (T)managers[i];
                }
            }

            return null;
        }
    }
}

