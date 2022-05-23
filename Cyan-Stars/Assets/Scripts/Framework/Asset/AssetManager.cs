using System;
using System.Collections.Generic;
using UnityEngine;
using CatAsset;
using Object = UnityEngine.Object;

namespace CyanStars.Framework.Asset
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class AssetManager : BaseManager
    {
        public override int Priority
        {
            get;
        }

        /// <summary>
        /// 运行模式
        /// </summary>
        [Header("运行模式")]
        public RunMode RunMode = RunMode.PackageOnly;

        /// <summary>
        /// 最大任务执行数量
        /// </summary>
        [Header("最大任务执行数量")]
        public int MaxTaskExcuteCount = 10;

        /// <summary>
        /// 延迟卸载时间
        /// </summary>
        [Header("延迟卸载时间")]
        public float UnloadDelayTime = 5;

        /// <summary>
        /// 是否开启编辑器资源模式
        /// </summary>
        [Header("是否开启编辑器资源模式")]
        public bool IsEditorMode = true;

        /// <summary>
        /// 编辑器资源模式下最大随机延迟模拟时间
        /// </summary>
        [Header("编辑器资源模式下最大随机延迟模拟时间")]
        public float EditorModeMaxDelay = 1;

        public override void OnInit()
        {
            CatAssetManager.RunMode = RunMode;

            CatAssetManager.MaxTaskExcuteCount = MaxTaskExcuteCount;
            CatAssetManager.UnloadDelayTime = UnloadDelayTime;

            CatAssetManager.IsEditorMode = IsEditorMode;
            CatAssetManager.EditorModeMaxDelay = EditorModeMaxDelay;
        }

        public override void OnUpdate(float deltaTime)
        {
            CatAssetManager.Update();
        }

        /// <summary>
        /// 检查安装包内资源清单,仅使用安装包内资源模式下专用
        /// </summary>
        public void CheckPackageManifest(Action<bool> callback)
        {
            CatAssetManager.CheckPackageManifest(callback);
        }

        /// <summary>
        /// 加载Asset
        /// </summary>
        public void LoadAsset(string assetName, Action<bool, Object> loadedCallback)
        {
            CatAssetManager.LoadAsset(assetName, loadedCallback);
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public void LoadScene(string sceneName, Action<bool, Object> loadedCallback)
        {
            CatAssetManager.LoadScene(sceneName, loadedCallback);
        }

        /// <summary>
        /// 批量加载Asset
        /// </summary>
        public void LoadAssets(List<string> assetNames, Action<List<Object>> loadedCallback)
        {
            CatAssetManager.LoadAssets(assetNames, loadedCallback);
        }

        /// <summary>
        /// 卸载Asset
        /// </summary>
        public void UnloadAsset(Object asset)
        {
            CatAssetManager.UnloadAsset(asset);
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        public void UnloadScene(string sceneName)
        {
            CatAssetManager.UnloadScene(sceneName);
        }
    }
}
