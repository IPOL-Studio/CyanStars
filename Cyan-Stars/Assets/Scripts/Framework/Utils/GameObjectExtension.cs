using UnityEngine;

namespace CyanStars.Framework.Utils
{
    /// <summary>
    /// 用于GameObject的扩展方法类
    /// </summary>
    public static class GameObjectExtension
    {
        /// <summary>
        /// 获取指定类型的组件，若不存在则添加
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
           T component = go.GetComponent<T>();
           if (component == null)
           {
               component = go.AddComponent<T>();
           }

           return component;
        }
    }
}