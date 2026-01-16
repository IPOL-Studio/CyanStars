#nullable enable

using System.Collections.Generic;
using System.Threading;
using CyanStars.Framework;
using CyanStars.Framework.GameObjectPool;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class EditAreaView : BaseView<EditAreaViewModel>
    {
        [SerializeField]
        private GameObject posLinesFrameObject = null!;

        [SerializeField]
        private RectTransform viewportRect = null!;

        [SerializeField]
        private RectTransform contentRect = null!;

        [SerializeField]
        private ScrollRect scrollRect = null!;

        [SerializeField]
        private RectTransform judgeLineRect = null!;


        // 节拍线颜色定义
        private static readonly Color BeatIntegerColor = Color.white;
        private static readonly Color BeatHalfColor = new Color(1f, 0.7f, 0.4f, 0.8f);
        private static readonly Color BeatQuarterColor = new Color(0.4f, 0.7f, 1f, 0.7f);
        private static readonly Color BeatOtherColor = new Color(0.6f, 1f, 0.6f, 0.6f);

        // 适用于对象池的字段
        private readonly CancellationTokenSource Cts = new CancellationTokenSource();
        private static GameObjectPoolManager PoolManager => GameRoot.GameObjectPool;
        private const string PosLinePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/PosLine.prefab";
        private const string BeatLinePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BeatLine.prefab";

        // 管理当前激活的节拍线：Key=节拍索引（含细分拍），Value=节拍线物体实例
        private readonly Dictionary<int, GameObject> ActiveBeatLines = new Dictionary<int, GameObject>();
        private readonly HashSet<int> LoadingBeatLines = new HashSet<int>();


        public override void Bind(EditAreaViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ViewModel.PosLineCount
                .Subscribe(UpdatePosLines)
                .AddTo(this);

            // 节拍线布局变化时整体重绘
            Observable.CombineLatest(
                    ViewModel.BeatAccuracy,
                    ViewModel.BeatZoom,
                    ViewModel.TotalBeats,
                    (_, _, _) => true
                )
                .Subscribe(_ => ForceRebuildBeatLines())
                .AddTo(this);

            // 滚动时增量更新节拍线
            scrollRect.onValueChanged
                .AsObservable()
                .Subscribe(_ => UpdateBeatLinesVisibility())
                .AddTo(this);
        }

        private async void UpdatePosLines(int count)
        {
            int oldPosLineCount = posLinesFrameObject.transform.childCount - 1; // UI 最左侧（在层级中为第 1 个）有一个不可见的自动布局元素

            for (int i = oldPosLineCount; i < count; i++)
            {
                // 补足位置线数量
                var go = await PoolManager.GetGameObjectAsync(PosLinePath, posLinesFrameObject.transform);
            }

            for (int i = oldPosLineCount; i > count; i--)
            {
                // 删除多余的位置线
                var go = posLinesFrameObject.transform.GetChild(i).gameObject; // 在算上左边一条线后 i 正好是需要的 Index
                PoolManager.ReleaseGameObject(PosLinePath, go);
            }
        }

        private void ForceRebuildBeatLines()
        {
            // 回收所有
            foreach (var kvp in ActiveBeatLines)
                PoolManager.ReleaseGameObject(BeatLinePath, kvp.Value);
            ActiveBeatLines.Clear();
            LoadingBeatLines.Clear();

            // 立即更新一次
            UpdateBeatLinesVisibility();
        }

        // 根据目前的滚动位置更新 BeatLines 可见性
        private void UpdateBeatLinesVisibility()
        {
            int beatAccuracy = ViewModel.BeatAccuracy.CurrentValue;
            float totalBeats = ViewModel.TotalBeats.CurrentValue;
            float beatLineDist = ViewModel.GetBeatLineDistance();

            // 计算视口在 Content 中的 Y 轴范围
            float contentHeight = contentRect.rect.height;
            float viewportHeight = viewportRect.rect.height;

            // 计算视口底部相对于 Content 底部的 Y 坐标
            float scrollY = (contentHeight - viewportHeight) * scrollRect.verticalNormalizedPosition;
            scrollY = Mathf.Clamp(scrollY, 0, contentHeight);

            // 多渲染一点视口外的线
            float minVisibleY = scrollY - 100f;
            float maxVisibleY = scrollY + viewportHeight + 100f;

            // 将 Y 轴范围转换为节拍索引范围
            int minIndex = Mathf.FloorToInt((minVisibleY - judgeLineRect.anchoredPosition.y) / beatLineDist);
            int maxIndex = Mathf.CeilToInt((maxVisibleY - judgeLineRect.anchoredPosition.y) / beatLineDist);

            minIndex = Mathf.Max(0, minIndex);
            int maxTotalIndex = Mathf.FloorToInt(totalBeats * beatAccuracy);
            maxIndex = Mathf.Min(maxIndex, maxTotalIndex);

            // 回收离视口过远的线
            List<int> toRemove = new List<int>();
            foreach (var kvp in ActiveBeatLines)
            {
                int index = kvp.Key;
                if (index < minIndex || index > maxIndex)
                {
                    toRemove.Add(index);
                }
            }

            foreach (int key in toRemove)
            {
                if (ActiveBeatLines.TryGetValue(key, out var go))
                {
                    PoolManager.ReleaseGameObject(BeatLinePath, go);
                    ActiveBeatLines.Remove(key);
                }
            }

            // 生成视口内缺失的线
            for (int i = minIndex; i <= maxIndex; i++)
            {
                if (!ActiveBeatLines.ContainsKey(i) && !LoadingBeatLines.Contains(i))
                {
                    CreateBeatLine(i, beatLineDist, beatAccuracy);
                }
            }
        }

        private async void CreateBeatLine(int index, float distance, int accuracy)
        {
            // 这里的 index 是细分后的索引

            LoadingBeatLines.Add(index);

            GameObject go = await PoolManager.GetGameObjectAsync(BeatLinePath, contentRect, Cts.Token);

            if (Cts.Token.IsCancellationRequested || ViewModel == null)
            {
                PoolManager.ReleaseGameObject(BeatLinePath, go);
                return;
            }

            LoadingBeatLines.Remove(index);
            ActiveBeatLines.Add(index, go);

            // 设置位置
            if (go.transform is RectTransform rect)
            {
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.localScale = Vector3.one;
                float yPos = judgeLineRect.anchoredPosition.y + (index * distance);
                rect.anchoredPosition = new Vector2(0, yPos);
            }

            // 设置样式 (颜色、文字)
            go.GetComponent<BeatLineItem>().SetVisuals(index, accuracy);
        }


        protected override void OnDestroy()
        {
            Cts.Cancel();
            Cts.Dispose();

            // 销毁时归还所有节拍线对象
            foreach (var kvp in ActiveBeatLines)
                PoolManager.ReleaseGameObject(BeatLinePath, kvp.Value);

            ActiveBeatLines.Clear();
            LoadingBeatLines.Clear();
        }
    }
}
