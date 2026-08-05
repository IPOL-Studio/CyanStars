#nullable enable

using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.Framework;
using Gameplay.ChartEditor;
using UnityEngine;
using GameObjectPoolManager = CyanStars.Framework.GameObjectPool.GameObjectPoolManager;

namespace CyanStars.Gameplay.ChartEditor.View
{
    /// <summary>
    /// 悬停预览音符管理器
    /// </summary>
    /// <remarks>
    /// 挂在 GhostNoteFrame 上。为每种音符类型持有专用的预览实例
    /// </remarks>
    public class Previewer : MonoBehaviour
    {
        private const float DefaultPreviewAlpha = 0.39f;

        /// <summary>
        /// 预览音符的整体透明度
        /// </summary>
        [SerializeField]
        private float previewAlpha = DefaultPreviewAlpha;

        private static readonly NoteType[] PreviewNoteTypes =
        {
            NoteType.Tap, NoteType.Drag, NoteType.Hold, NoteType.Click, NoteType.Break
        };

        private static GameObjectPoolManager PoolManager => GameRoot.GameObjectPool;

        /// <summary>
        /// 各音符类型的预览实例
        /// </summary>
        private readonly Dictionary<NoteType, PreviewInstance> Instances = new Dictionary<NoteType, PreviewInstance>();

        private bool allInstancesReady;   // 全部预览实例是否已创建完成
        private NoteType? pendingType;    // 实例创建期间缓存的最新请求（null = 隐藏）
        private Vector2 pendingPosition;
        private NoteType? currentType;    // 当前显示的实例类型（null = 全部隐藏）

        private class PreviewInstance
        {
            public GameObject Go = null!;
            public RectTransform Rect = null!;
        }

        private void Start()
        {
            CreateInstances();
        }

        /// <summary>
        /// 显示指定类型的预览音符
        /// </summary>
        /// <param name="type">音符类型</param>
        /// <param name="anchoredPosition">以 GhostNoteFrame 底部中心（即 Content 底部中心）为原点的位置</param>
        public void Show(NoteType type, Vector2 anchoredPosition)
        {
            pendingType = type;
            pendingPosition = anchoredPosition;

            if (allInstancesReady)
            {
                ApplyState();
            }
        }

        /// <summary>
        /// 隐藏预览音符
        /// </summary>
        public void Hide()
        {
            pendingType = null;

            if (allInstancesReady)
            {
                ApplyState();
            }
        }

        /// <summary>
        /// 按当前请求应用预览实例的显隐与位置
        /// </summary>
        private void ApplyState()
        {
            if (pendingType == currentType)
            {
                // 类型未变：仅更新位置
                if (pendingType is { } type && Instances.TryGetValue(type, out var instance))
                {
                    instance.Rect.anchoredPosition = pendingPosition;
                }

                return;
            }

            // 隐藏旧实例
            if (currentType is { } oldType && Instances.TryGetValue(oldType, out var oldInstance))
            {
                oldInstance.Go.SetActive(false);
            }

            currentType = pendingType;

            // 显示新实例
            if (pendingType is { } newType && Instances.TryGetValue(newType, out var newInstance))
            {
                newInstance.Go.SetActive(true);
                newInstance.Rect.anchoredPosition = pendingPosition;
            }
        }

        /// <summary>
        /// 为每种音符类型创建专用的预览实例（从对象池获取后不再归还）
        /// </summary>
        private async void CreateInstances()
        {
            foreach (NoteType type in PreviewNoteTypes)
            {
                string path = ChartEditorAssetHelper.GetNotePrefabPath(type);

                GameObject go = await PoolManager.GetGameObjectAsync(path, transform, destroyCancellationToken);
                go.transform.localScale = Vector3.one;

                if (destroyCancellationToken.IsCancellationRequested)
                {
                    // View 已销毁，归还本次获取的对象
                    PoolManager.ReleaseGameObject(path, go);
                    return;
                }

                // 挂 CanvasGroup 控制整体透明度并禁用射线拦截
                // 实例为预览专用（不归还对象池），组件状态无需还原
                if (!go.TryGetComponent<CanvasGroup>(out var canvasGroup))
                {
                    canvasGroup = go.AddComponent<CanvasGroup>();
                }

                canvasGroup.enabled = true;
                canvasGroup.alpha = previewAlpha;
                canvasGroup.blocksRaycasts = false;

                var instance = new PreviewInstance
                {
                    Go = go,
                    Rect = (RectTransform)go.transform
                };
                Instances[type] = instance;
                go.SetActive(false);
            }

            allInstancesReady = true;
            ApplyState();
        }
    }
}
