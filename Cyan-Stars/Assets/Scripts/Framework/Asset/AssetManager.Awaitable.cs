using System.Collections.Generic;
using System.Threading.Tasks;
using CatAsset.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CyanStars.Framework.Asset
{
    public partial class AssetManager
    {
        /// <summary>
        /// 检查安装包内资源清单,仅使用安装包内资源模式下专用（可等待）
        /// </summary>
        public Task<bool> AwaitCheckPackageManifest()
        {
            return CatAssetManager.AwaitCheckPackageManifest();
        }

        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public Task<T> AwaitLoadAsset<T>(string assetName,GameObject target = null,TaskPriority priority = TaskPriority.Middle)
        {
            return CatAssetManager.AwaitLoadAsset<T>(assetName, target, priority);
        }

        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public Task<T> AwaitLoadAsset<T>(string assetName,Scene target,TaskPriority priority = TaskPriority.Middle)
        {
            return CatAssetManager.AwaitLoadAsset<T>(assetName, target, priority);
        }

        /// <summary>
        /// 批量加载资源(可等待)
        /// </summary>
        public Task<List<LoadAssetResult>> AwaitBatchLoadAsset(List<string> assetNames,GameObject target = null,TaskPriority priority = TaskPriority.Middle)
        {
            return CatAssetManager.AwaitBatchLoadAsset(assetNames, target, priority);
        }

        /// <summary>
        /// 批量加载资源(可等待)
        /// </summary>
        public Task<List<LoadAssetResult>> AwaitBatchLoadAsset(List<string> assetNames,Scene target = default,TaskPriority priority = TaskPriority.Middle)
        {
            return CatAssetManager.AwaitBatchLoadAsset(assetNames, target, priority);
        }

        /// <summary>
        /// 加载场景(可等待)
        /// </summary>
        public Task<Scene> AwaitLoadScene(string sceneName)
        {
            return CatAssetManager.AwaitLoadScene(sceneName);
        }
    }
}
