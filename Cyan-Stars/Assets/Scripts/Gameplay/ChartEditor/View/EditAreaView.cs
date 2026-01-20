#nullable enable

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Framework.GameObjectPool;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using ObservableCollections;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
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
        private const string TapNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/TapNote.prefab";
        private const string DragNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/DragNote.prefab";
        private const string HoldNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/HoldNote.prefab";
        private const string ClickNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/ClickNote.prefab";
        private const string BreakNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BreakNote.prefab";

        // 管理当前激活的节拍线：Key=节拍索引（含细分拍），Value=节拍线物体实例
        // 开始加载时会将 item 对应的 Value 设为 null 占位，加载完成后覆写为 gameObject
        private readonly Dictionary<int, GameObject?> ActiveBeatLines = new Dictionary<int, GameObject?>();

        // 管理当前激活的音符: Key=音符数据对象, Value=GameObject
        private readonly Dictionary<BaseChartNoteData, GameObject?> ActiveNotes = new Dictionary<BaseChartNoteData, GameObject?>();


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

            // 监听音符数据变化（增删改）或缩放变化 -> 刷新可见性
            Observable.Merge(
                    targetViewModel.Notes.ObserveChanged().Select(_ => Unit.Default),
                    targetViewModel.BeatZoom.Select(_ => Unit.Default), // 缩放改变位置
                    targetViewModel.BeatAccuracy.Select(_ => Unit.Default),
                    scrollRect.onValueChanged.AsObservable().Select(_ => Unit.Default) // 滚动
                )
                .ThrottleLastFrame(1) // 避免同一帧多次刷新
                .Subscribe(_ => UpdateNotesVisibility())
                .AddTo(this);
        }


        private async void UpdatePosLines(int count)
        {
            // 场景销毁时直接取消
            if (Cts.IsCancellationRequested)
                return;

            int oldPosLineCount = posLinesFrameObject.transform.childCount - 1; // UI 最左侧（在层级中为第 1 个）有一个不可见的自动布局元素

            var tasks = new List<Task<GameObject>>();
            for (int i = oldPosLineCount; i < count; i++)
            {
                // 补足位置线数量
                tasks.Add(PoolManager.GetGameObjectAsync(PosLinePath, posLinesFrameObject.transform));
            }

            await Task.WhenAll(tasks);

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

            // 立即更新一次
            UpdateBeatLinesVisibility();
        }

        /// <summary>
        /// 根据目前的滚动位置更新 BeatLines 可见性
        /// </summary>
        private async void UpdateBeatLinesVisibility()
        {
            // 场景销毁时直接取消
            if (Cts.IsCancellationRequested)
                return;

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
                    if (go is not null) // 代表已经完成加载的物体，将会进行销毁
                        PoolManager.ReleaseGameObject(BeatLinePath, go);
                    ActiveBeatLines.Remove(key); // 正在加载的物体在加载完成后会自己检查 index 是否还在激活列表中
                }
            }

            // 生成视口内缺失的线
            var tasks = new List<Task<GameObject?>>();
            for (int i = minIndex; i <= maxIndex; i++)
            {
                if (ActiveBeatLines.TryAdd(i, null))
                {
                    tasks.Add(CreateBeatLine(i, beatLineDist, beatAccuracy));
                }
            }

            await Task.WhenAll(tasks);
        }

        private async Task<GameObject?> CreateBeatLine(int index, float distance, int accuracy) // 这里的 index 是细分后的索引
        {
            GameObject go = await PoolManager.GetGameObjectAsync(BeatLinePath, contentRect, Cts.Token);

            // 如果加载完后 Ctx 被取消了（场景卸载）或元素没有在激活字典里（离开视口），直接抛弃并释放
            if (Cts.Token.IsCancellationRequested || !ActiveBeatLines.TryGetValue(index, out _))
            {
                PoolManager.ReleaseGameObject(BeatLinePath, go);
                return null;
            }

            // 如果加载完后发现已经有物体了（快速在边界滚动导致启动了多个加载任务），则释放原来的物体并用新物体替代
            // TODO: 为每个加载任务单独分配 token
            if (ActiveBeatLines[index] is not null)
                PoolManager.ReleaseGameObject(BeatLinePath, ActiveBeatLines[index]);

            ActiveBeatLines[index] = go;

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
            return go;
        }


        private async void UpdateNotesVisibility()
        {
            if (Cts.IsCancellationRequested) return;

            float judgeLineY = judgeLineRect.anchoredPosition.y;
            float viewportHeight = viewportRect.rect.height;
            float contentHeight = contentRect.rect.height;

            // 计算当前视口在 Content 中的范围
            float scrollY = (contentHeight - viewportHeight) * scrollRect.verticalNormalizedPosition;
            float viewMinY = scrollY - 100f; // Buffer
            float viewMaxY = scrollY + viewportHeight + 100f;

            // 1. 找出需要显示的音符 & 需要移除的音符
            var notesToShow = new List<BaseChartNoteData>();
            var notesToRemove = new List<BaseChartNoteData>();

            // 遍历所有音符 (如果是巨量音符，建议 ViewModel 维护一个按时间排序的列表，这里用二分查找)
            foreach (var note in ViewModel.Notes)
            {
                float yPos = CalculateNoteY(note.JudgeBeat);
                float yEndPos = yPos;

                if (note is HoldChartNoteData hold)
                {
                    yEndPos = CalculateNoteY(hold.EndJudgeBeat);
                }

                // 判断是否在可视范围内 (只要有一部分在范围内即可)
                bool isVisible = (yEndPos >= viewMinY) && (yPos <= viewMaxY);

                if (isVisible)
                {
                    if (!ActiveNotes.ContainsKey(note))
                    {
                        notesToShow.Add(note);
                    }
                }
                else
                {
                    if (ActiveNotes.ContainsKey(note))
                    {
                        notesToRemove.Add(note);
                    }
                }
            }

            // 2. 回收离开视口的音符
            foreach (var note in notesToRemove)
            {
                if (ActiveNotes.TryGetValue(note, out var go))
                {
                    if (go != null)
                    {
                        string path = GetPrefabPath(note.Type);
                        PoolManager.ReleaseGameObject(path, go);
                    }

                    ActiveNotes.Remove(note);
                }
            }

            // 3. 生成进入视口的音符
            var tasks = new List<Task>();
            foreach (var note in notesToShow)
            {
                ActiveNotes.Add(note, null); // 占位，防止重复加载
                tasks.Add(CreateNoteObject(note));
            }

            await Task.WhenAll(tasks);
        }

        private async Task CreateNoteObject(BaseChartNoteData note)
        {
            string path = GetPrefabPath(note.Type);
            GameObject go = await PoolManager.GetGameObjectAsync(path, contentRect, Cts.Token);

            if (Cts.Token.IsCancellationRequested || !ActiveNotes.ContainsKey(note))
            {
                PoolManager.ReleaseGameObject(path, go);
                return;
            }

            if (ActiveNotes[note] != null) PoolManager.ReleaseGameObject(path, ActiveNotes[note]);

            ActiveNotes[note] = go;

            // 设置数据和位置
            if (go.TryGetComponent<EditNoteItem>(out var item))
            {
                float startY = CalculateNoteY(note.JudgeBeat);
                float endY = startY;
                if (note is HoldChartNoteData hold)
                {
                    endY = CalculateNoteY(hold.EndJudgeBeat);
                }

                item.SetData(note, startY, endY);
            }
        }

        private float CalculateNoteY(Beat beat)
        {
            // Y = 判定线位置 + (Beat值 * 每拍距离)
            float beatVal = beat.ToFloat();
            float distPerBeat = ViewModel.GetBeatLineDistance() * ViewModel.BeatAccuracy.CurrentValue; // GetBeatLineDistance 返回的是细分拍距离，需还原
            // 或者直接用:
            float interval = 250f * ViewModel.BeatZoom.CurrentValue; // DefaultBeatLineInterval = 250f

            return judgeLineRect.anchoredPosition.y + (beatVal * interval);
        }

        private string GetPrefabPath(NoteType type) => type switch
        {
            NoteType.Tap => TapNotePath,
            NoteType.Drag => DragNotePath,
            NoteType.Hold => HoldNotePath,
            NoteType.Click => ClickNotePath,
            NoteType.Break => BreakNotePath,
            _ => TapNotePath
        };

        // --- IPointerClickHandler 实现 ---

        public void OnPointerClick(PointerEventData eventData)
        {
            // 将屏幕坐标转为 ScrollRect Content 的局部坐标
            // 这样无论 Content 怎么滚动，得到的都是相对于 Content 原点的坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                contentRect, // 注意这里用 contentRect 而不是 scrollRect
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );

            // 这里 localPoint.y 已经是包含滚动距离的坐标了
            // 但是 ViewModel 的计算逻辑可能需要相对于视口的坐标或者纯粹的逻辑坐标
            // 让我们复用旧代码逻辑：
            // 旧代码用 RectTransformUtility.ScreenPointToLocalPointInRectangle(scrollRect...)
            // 这里的 EditAreaView 挂载在 EditArea Panel 上，通常就是 ScrollRect 所在物体。

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                GetComponent<RectTransform>(), // 应该是 Viewport 或 ScrollRect 本身
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPointInScrollRect
            );

            // 计算 Content 里的实际 Y 偏移
            float contentY = (contentRect.rect.height - viewportRect.rect.height) * scrollRect.verticalNormalizedPosition;
            float clickYOnContent = contentY + localPointInScrollRect.y; // 这里假设 Anchor 设定导致坐标系方向一致，具体需根据 UI 布局微调

            // 为简化，直接传 localPointInScrollRect 给 VM，并在 VM 里结合 contentY (或者 VM 不关心 contentY，只关心相对于判定线的距离)
            // 更好的方式：在 View 层算好相对于 JudgeLine 的 Y 距离

            float relativeYToJudgeLine = clickYOnContent - judgeLineRect.anchoredPosition.y;

            // 重新构造传入 VM 的坐标
            // VM 需要 X 来判断轨道，需要 Y 来判断时间
            Vector2 clickPosForVm = new Vector2(localPointInScrollRect.x, clickYOnContent);

            var result = ViewModel.CalculateNotePlacement(clickPosForVm, judgeLineRect.anchoredPosition.y);

            if (result.HasValue)
            {
                ViewModel.CreateNote(result.Value.pos, result.Value.beat);
            }
        }


        protected override void OnDestroy()
        {
            Cts.Cancel();
            Cts.Dispose();

            // 销毁时归还所有节拍线对象
            foreach (var kvp in ActiveBeatLines)
            {
                PoolManager.ReleaseGameObject(BeatLinePath, kvp.Value);
            }

            // 归还所有 Note
            foreach (var kvp in ActiveNotes)
            {
                if (kvp.Value != null)
                {
                    PoolManager.ReleaseGameObject(GetPrefabPath(kvp.Key.Type), kvp.Value);
                }
            }

            ActiveBeatLines.Clear();
        }
    }
}
