using System;
using System.Collections.Generic;
using UnityEngine;
using CatAsset.Runtime;
using UnityEngine.SceneManagement;

namespace CyanStars.Framework.Asset
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public partial class AssetManager : BaseManager
    {
        public override int Priority { get; }

        [Header("运行模式")]
        public RuntimeMode RuntimeMode = RuntimeMode.PackageOnly;

        [Header("编辑器资源模式")]
        public bool IsEditorMode = true;

        [Header("资源包卸载延迟")]
        public float UnloadDelayTime = 60f;

        [Header("单帧最大任务运行数量")]
        public int MaxTaskRunCount = 30;

        public override void OnInit()
        {
            CatAssetManager.RuntimeMode = RuntimeMode;
            CatAssetManager.IsEditorMode = IsEditorMode;
            CatAssetManager.UnloadDelayTime = UnloadDelayTime;
            CatAssetManager.MaxTaskRunCount = MaxTaskRunCount;

            // 注册一些自定义原生资源解析器
            foreach (var converter in CustomRawAssetConverters.Converters)
            {
                RegisterCustomRawAssetConverter(converter.Key, converter.Value);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            CatAssetManager.Update();
        }

        /// <summary>
        /// 注册自定义原生资源转换方法
        /// </summary>
        public void RegisterCustomRawAssetConverter(Type type, CustomRawAssetConverter converter)
        {
            CatAssetManager.RegisterCustomRawAssetConverter(type, converter);
        }

        /// <summary>
        /// 检查安装包内资源清单,仅使用安装包内资源模式下专用
        /// </summary>
        public void CheckPackageManifest(Action<bool> callback)
        {
            CatAssetManager.CheckPackageManifest(callback);
        }

        /// <summary>
        /// 检查资源版本，可更新资源模式下专用
        /// </summary>
        public void CheckVersion(OnVersionChecked onVersionChecked)
        {
            CatAssetManager.CheckVersion(onVersionChecked);
        }

        /// <summary>
        /// 从外部导入内置资源
        /// </summary>
        public void ImportInternalAsset(string manifestPath, Action<bool> callback,
            string bundleRelativePathPrefix = null)
        {
            CatAssetManager.ImportInternalAsset(manifestPath, callback, bundleRelativePathPrefix);
        }

        /// <summary>
        /// 更新资源组
        /// </summary>
        public void UpdateGroup(string group, OnBundleUpdated callback)
        {
            CatAssetManager.UpdateGroup(group, callback);
        }

        /// <summary>
        /// 暂停资源组更新
        /// </summary>
        public void PauseGroupUpdater(string group, bool isPause)
        {
            CatAssetManager.PauseGroupUpdater(group, isPause);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public int LoadAssetAsync(string assetName, LoadAssetCallback<object> callback,
            TaskPriority priority = TaskPriority.Middle)
        {
            return CatAssetManager.LoadAssetAsync(assetName, callback, priority);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public int LoadAssetAsync<T>(string assetName, LoadAssetCallback<T> callback,
            TaskPriority priority = TaskPriority.Middle)
        {
            return CatAssetManager.LoadAssetAsync<T>(assetName, callback, priority);
        }

        /// <summary>
        /// 批量加载资源
        /// </summary>
        public int BatchLoadAsset(List<string> assetNames, BatchLoadAssetCallback callback,
            TaskPriority priority = TaskPriority.Middle)
        {
            return CatAssetManager.BatchLoadAssetAsync(assetNames, callback);
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public int LoadSceneAsync(string sceneName, LoadSceneCallback callback,
            TaskPriority priority = TaskPriority.Middle)
        {
            return CatAssetManager.LoadSceneAsync(sceneName, callback, priority);
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        public void CancelTask(int guid)
        {
            CatAssetManager.CancelTask(guid);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        public void UnloadAsset(object asset)
        {
            CatAssetManager.UnloadAsset(asset);
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        public void UnloadScene(Scene scene)
        {
            CatAssetManager.UnloadScene(scene);
        }

        /// <summary>
        /// 将资源绑定到游戏物体上，会在指定游戏物体销毁时卸载绑定的资源
        /// </summary>
        public void BindToGameObject(GameObject target, object asset)
        {
            CatAssetManager.BindToGameObject(target, asset);
        }

        /// <summary>
        /// 将资源绑定到场景上，会在指定场景卸载时卸载绑定的资源
        /// </summary>
        public void BindToScene(Scene scene, object asset)
        {
            CatAssetManager.BindToScene(scene, asset);
        }

        /// <summary>
        /// 获取资源组信息
        /// </summary>
        public GroupInfo GetGroupInfo(string group)
        {
            return CatAssetManager.GetGroupInfo(group);
        }

        /// <summary>
        /// 获取所有资源组信息
        /// </summary>
        public List<GroupInfo> GetAllGroupInfo()
        {
            return CatAssetManager.GetAllGroupInfo();
        }

        /// <summary>
        /// 获取指定资源组的更新器
        /// </summary>
        public GroupUpdater GetGroupUpdater(string group)
        {
            return CatAssetManager.GetGroupUpdater(group);
        }
    }
}
