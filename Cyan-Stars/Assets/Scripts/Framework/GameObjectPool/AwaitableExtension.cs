using System.Threading.Tasks;
using UnityEngine;

namespace CyanStars.Framework.GameObjectPool
{
    /// <summary>
    /// 可等待扩展
    /// </summary>
    public static class AwaitableExtension
    {
        /// <summary>
        /// 从池中获取一个游戏对象（可等待）
        /// </summary>
        public static Task<GameObject> AwaitGetGameObject(this GameObjectPoolManager self, string prefabName, Transform parent)
        {
            TaskCompletionSource<GameObject> tcs = new TaskCompletionSource<GameObject>();
            self.GetGameObject(prefabName, parent, go => tcs.SetResult(go));
            return tcs.Task;
        }

        /// <summary>
        /// 从池中获取一个游戏对象（可等待）
        /// </summary>
        public static Task<GameObject> AwaitGetGameObject(this GameObjectPoolManager self, GameObject template, Transform parent)
        {
            TaskCompletionSource<GameObject> tcs = new TaskCompletionSource<GameObject>();
            self.GetGameObject(template, parent, (go) => { tcs.SetResult(go); });
            return tcs.Task;
        }
    }
}
