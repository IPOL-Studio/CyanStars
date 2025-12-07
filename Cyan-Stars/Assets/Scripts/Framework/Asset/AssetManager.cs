using System;
using System.Collections.Generic;
using UnityEngine;
using CatAsset.Runtime;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using System.Threading;
using CyanStars.EditorExtension;

namespace CyanStars.Framework.Asset
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class AssetManager : BaseManager
    {
        public override int Priority { get; }

        [Header("运行模式"), Active(ActiveMode.Edit)]
        public RuntimeMode RuntimeMode = RuntimeMode.PackageOnly;

        [Header("编辑器资源模式"), Active(ActiveMode.Edit)]
        public bool IsEditorMode = true;

        [Header("资源包卸载延迟"), Active(ActiveMode.Edit)]
        public float UnloadBundleDelayTime = 120f;

        [Header("资源卸载延迟"), Active(ActiveMode.Edit)]
        public float UnloadAssetDelayTime = 60f;

        [Header("最大任务同时运行数量"), Active(ActiveMode.Edit)]
        public int MaxTaskRunCount = 30;

        public override void OnInit()
        {
            CatAssetManager.UnloadBundleDelayTime = UnloadBundleDelayTime;
            CatAssetManager.UnloadAssetDelayTime = UnloadAssetDelayTime;
            CatAssetManager.MaxTaskRunCount = MaxTaskRunCount;

            //添加调试分析器组件
            gameObject.AddComponent<ProfilerComponent>();

#if UNITY_EDITOR
            if (IsEditorMode)
            {
                CatAssetManager.SetAssetLoader<EditorAssetLoader>();
                return;
            }
#endif
            switch (RuntimeMode)
            {
                case RuntimeMode.PackageOnly:
                    CatAssetManager.SetAssetLoader<PackageOnlyAssetLoader>();
                    break;
                case RuntimeMode.Updatable:
                    CatAssetManager.SetAssetLoader<UpdatableAssetLoader>();
                    break;
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
            CatAssetManager.RegisterCustomRawAssetConverter(type,converter);
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
            CatAssetManager.ImportReadWriteManifest(manifestPath,callback,bundleRelativePathPrefix);
        }

        /// <summary>
        /// 更新资源组
        /// </summary>
        public void UpdateGroup(string group, BundleUpdatedCallback callback)
        {
            CatAssetManager.UpdateGroup(group,callback);
        }

        /// <summary>
        /// 暂停资源组更新
        /// </summary>
        public void PauseGroupUpdater(string group)
        {
            CatAssetManager.PauseGroupUpdater(group);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public AssetHandler<object> LoadAssetAsync(string assetName, CancellationToken cancellationToken = default,
                                                   TaskPriority priority = TaskPriority.Middle)
        {
            return CatAssetManager.LoadAssetAsync(assetName, cancellationToken, priority);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public AssetHandler<T> LoadAssetAsync<T>(string assetName, CancellationToken cancellationToken = default,
                                                 TaskPriority priority = TaskPriority.Middle)
        {
            return CatAssetManager.LoadAssetAsync<T>(assetName, cancellationToken, priority);
        }

        /// <summary>
        /// 批量加载资源
        /// </summary>
        public BatchAssetHandler BatchLoadAssetAsync(List<string> assetNames, CancellationToken cancellationToken = default,
                                                TaskPriority priority = TaskPriority.Middle)
        {
            return CatAssetManager.BatchLoadAssetAsync(assetNames, cancellationToken, priority);
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        public SceneHandler LoadSceneAsync(string sceneName, CancellationToken cancellationToken = default,
                                           TaskPriority priority = TaskPriority.Middle)
        {
            return CatAssetManager.LoadSceneAsync(sceneName, cancellationToken, priority);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        public void UnloadAsset(AssetHandler handler)
        {
            CatAssetManager.UnloadAsset(handler);
        }

        /// <summary>
        /// 卸载场景
        /// </summary>
        public void UnloadScene(SceneHandler handler)
        {
            CatAssetManager.UnloadScene(handler);
        }

        /// <summary>
        /// 将资源绑定到游戏物体上，会在指定游戏物体销毁时卸载绑定的资源
        /// </summary>
        public void BindToGameObject(GameObject target, IBindableHandler handler)
        {
            CatAssetManager.BindToGameObject(target, handler);
        }

        /// <summary>
        /// 将资源绑定到场景上，会在指定场景卸载时卸载绑定的资源
        /// </summary>
        public void BindToScene(Scene scene, IBindableHandler handler)
        {
            CatAssetManager.BindToScene(scene,handler);
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
