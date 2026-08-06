#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CyanStars.Chart;
using CyanStars.Framework;
using Gameplay.ChartEditor;
using UnityEngine;

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
        /// <summary>
        /// 预览音符的整体透明度
        /// </summary>
        [SerializeField]
        private float previewAlpha = 0.39f;


        private static readonly NoteType[] PreviewNoteTypes =
        {
            NoteType.Tap, NoteType.Drag, NoteType.Hold, NoteType.Click, NoteType.Break
        };

        /// <summary>
        /// 各音符类型的预览实例
        /// </summary>
        private readonly Dictionary<NoteType, PreviewInstance> Instances = new Dictionary<NoteType, PreviewInstance>();

        private bool allInstancesReady; // 全部预览实例是否已创建完成
        private NoteType? currentType; // 当前显示的实例类型（null = 全部隐藏）

        private class PreviewInstance
        {
            public GameObject Go = null!;
            public RectTransform Rect = null!;
            public CanvasGroup CanvasGroup = null!;
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
            if (allInstancesReady)
                ApplyState(type, anchoredPosition);
        }

        /// <summary>
        /// 隐藏预览音符
        /// </summary>
        public void Hide()
        {
            if (allInstancesReady)
                ApplyState(null, Vector2.zero);
        }

        /// <summary>
        /// 按当前请求应用预览实例的显隐与位置
        /// </summary>
        private void ApplyState(NoteType? pendingType, Vector2 pendingPosition)
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
                oldInstance.CanvasGroup.alpha = 0;
            }

            currentType = pendingType;

            // 显示新实例
            if (pendingType is { } newType && Instances.TryGetValue(newType, out var newInstance))
            {
                newInstance.CanvasGroup.alpha = previewAlpha;
                newInstance.Rect.anchoredPosition = pendingPosition;
            }
        }

        /// <summary>
        /// 为每种音符类型创建专用的预览实例
        /// </summary>
        private async void CreateInstances()
        {
            var tasks = PreviewNoteTypes
                .Select(async type =>
                {
                    string path = ChartEditorAssetHelper.GetNotePrefabPath(type);
                    GameObject go = await GameRoot.GameObjectPool.GetGameObjectAsync(path, transform, destroyCancellationToken);

                    PreviewInstance? resultInstance = null;

                    if (destroyCancellationToken.IsCancellationRequested)
                    {
                        if (go != null)
                            GameRoot.GameObjectPool.ReleaseGameObject(path, go);
                        return (type, instance: resultInstance);
                    }

                    go.transform.localScale = Vector3.one;

                    if (!go.TryGetComponent<CanvasGroup>(out var canvasGroup))
                        canvasGroup = go.AddComponent<CanvasGroup>();

                    canvasGroup.enabled = true;
                    canvasGroup.alpha = 0;
                    canvasGroup.blocksRaycasts = false;

                    // 不显示 Hold 拖尾
                    if (go.TryGetComponent<EditAreaNoteView>(out var noteView))
                    {
                        noteView.SetHoldLength(0);
                    }

                    resultInstance = new PreviewInstance
                    {
                        Go = go, Rect = (RectTransform)go.transform, CanvasGroup = canvasGroup
                    };

                    return (type, instance: resultInstance);
                })
                .ToList();

            var results = await Task.WhenAll(tasks);

            if (destroyCancellationToken.IsCancellationRequested)
                return;

            foreach (var res in results)
                if (res.instance != null)
                    Instances[res.type] = res.instance;

            allInstancesReady = true;
            ApplyState(null, Vector2.zero);
        }
    }
}
