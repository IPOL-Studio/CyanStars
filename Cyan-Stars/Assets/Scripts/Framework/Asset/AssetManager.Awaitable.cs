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
        public Task<bool> CheckPackageManifest()
        {
            return CatAssetManager.CheckPackageManifest();
        }

        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public Task<T> LoadAssetAsync<T>(string assetName,GameObject target = null,TaskPriority priority = TaskPriority.Middle)
        {
            return CatAssetManager.LoadAssetAsync<T>(assetName, target, priority);
        }

        /// <summary>
        /// 加载资源（可等待）
        /// </summary>
        public Task<T> LoadAssetAsync<T>(string assetName,Scene target,TaskPriority priority = TaskPriority.Middle)
        {
            return CatAssetManager.LoadAssetAsync<T>(assetName, target, priority);
        }

        /// <summary>
        /// 批量加载资源(可等待)
        /// </summary>
        public Task<List<LoadAssetResult>> BatchLoadAssetAsync(List<string> assetNames,GameObject target = null,TaskPriority priority = TaskPriority.Middle)
        {
            return CatAssetManager.BatchLoadAssetAsync(assetNames, target, priority);
        }

        /// <summary>
        /// 批量加载资源(可等待)
        /// </summary>
        public Task<List<LoadAssetResult>> BatchLoadAssetAsync(List<string> assetNames,Scene target = default,TaskPriority priority = TaskPriority.Middle)
        {
            return CatAssetManager.BatchLoadAssetAsync(assetNames, target, priority);
        }

        /// <summary>
        /// 加载场景(可等待)
        /// </summary>
        public Task<Scene> LoadSceneAsync(string sceneName)
        {
            return CatAssetManager.LoadSceneAsync(sceneName);
        }
    }
}
