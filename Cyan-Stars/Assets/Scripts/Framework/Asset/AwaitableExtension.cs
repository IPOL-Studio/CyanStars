using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CyanStars.Framework.Utils;

namespace CyanStars.Framework.Asset
{
    /// <summary>
    /// 可等待扩展
    /// </summary>
    public static class AwaitableExtension
    {
        /// <summary>
        /// 检查资源清单（可等待）
        /// </summary>
        public static Task<bool> AwaitCheckPackageManifest(this AssetManager self)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            self.CheckPackageManifest((success) =>
            {
                tcs.SetResult(success);
            });
            return tcs.Task;
        }

        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public static Task<T> AwaitLoadAsset<T>(this AssetManager self, string assetName,
            GameObject bindingTarget = null) where T : Object
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            self.LoadAsset(assetName, (success, asset) =>
            {
                if (bindingTarget)
                {
                    bindingTarget.GetOrAddComponent<AssetBinder>().BindTo(asset);
                }

                tcs.SetResult(asset as T);
            });
            return tcs.Task;
        }

        /// <summary>
        /// 加载场景（可等待）
        /// </summary>
        public static Task<bool> AwaitLoadScene(this AssetManager self, string sceneName)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            self.LoadScene(sceneName, (success, obj) =>
            {
                Debug.Log("加载场景完成");
                tcs.SetResult(success);
            });
            return tcs.Task;
        }

        /// <summary>
        /// 批量加载资源（可等待）
        /// </summary>
        public static Task<List<Object>> AwaitLoadAssets(this AssetManager self, List<string> assetNames,
            GameObject bindingTarget = null)
        {
            TaskCompletionSource<List<Object>> tcs = new TaskCompletionSource<List<Object>>();
            self.LoadAssets(assetNames, (assets) =>
            {
                if (bindingTarget)
                {
                    AssetBinder binder = bindingTarget.GetOrAddComponent<AssetBinder>();
                    for (int i = 0; i < assets.Count; i++)
                    {
                        binder.BindTo(assets[i]);
                    }
                }


                tcs.SetResult(assets);
            });
            return tcs.Task;
        }
    }
}
