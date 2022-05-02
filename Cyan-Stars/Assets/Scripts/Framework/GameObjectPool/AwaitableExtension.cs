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
        public static Task<GameObject> AwaitGetGameObject(this GameObjectPoolManager self,string prefabName)
        {
            TaskCompletionSource<GameObject> tcs = new TaskCompletionSource<GameObject>();
            self.GetGameObject(prefabName, (go) =>
            {
                tcs.SetResult(go);
            });
            return tcs.Task;
        }
    }
}