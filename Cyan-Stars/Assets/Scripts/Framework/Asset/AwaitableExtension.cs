using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CyanStars.Framework.Asset
{
    /// <summary>
    /// 可等待扩展
    /// </summary>
    public static class AwaitableExtension
    {
        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public static Task<T> AwaitLoadAsset<T>(this AssetManager self, string assetName) where T : Object
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            self.LoadAsset(assetName, (success, asset) =>
            {
                tcs.SetResult(asset as T);
            });
            return tcs.Task;
        }
        
        /// <summary>
        /// 加载场景（可等待）
        /// </summary>
        public static Task LoadScene(this AssetManager self,string sceneName)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            self.LoadScene(sceneName, (success, obj) =>
            {
                tcs.SetResult(null);
            });
            return tcs.Task;
        }
        
        /// <summary>
        /// 批量加载资源（可等待）
        /// </summary>
        public static Task<List<Object>> AwaitLoadAssets(this AssetManager self, List<string> assetNames)
        {
            TaskCompletionSource<List<Object>> tcs = new TaskCompletionSource<List<Object>>();
            self.LoadAssets(assetNames, (assets) =>
            {
                tcs.SetResult(assets);
            });
            return tcs.Task;
        }
    }

}
