#nullable enable

using System;
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
    public class EditAreaView : BaseView<EditAreaViewModel>, IPointerClickHandler
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

        // 预制体路径
        private const string PosLinePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/PosLine.prefab";
        private const string BeatLinePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BeatLine.prefab";
        private const string TapNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/TapNote.prefab";
        private const string DragNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/DragNote.prefab";
        private const string HoldNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/HoldNote.prefab";
        private const string ClickNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/ClickNote.prefab";
        private const string BreakNotePath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BreakNote.prefab";

        private readonly CancellationTokenSource Cts = new CancellationTokenSource();
        private static GameObjectPoolManager PoolManager => GameRoot.GameObjectPool;

        // 管理当前激活的节拍线：Key=节拍索引（含细分拍），Value=节拍线物体实例
        // 开始加载时会将 item 对应的 Value 设为 null 占位，加载完成后覆写为 gameObject
        private readonly Dictionary<int, GameObject?> ActiveBeatLines = new Dictionary<int, GameObject?>();

        // 管理当前激活的音符: Key=音符数据对象, Value=(ViewModel, View)
        // 字典 Key 用于快速判断可见性，Value 用于资源管理
        private readonly Dictionary<BaseChartNoteData, (EditAreaNoteViewModel vm, EditAreaNoteView view)?> ActiveNotes =
            new Dictionary<BaseChartNoteData, (EditAreaNoteViewModel, EditAreaNoteView)?>();


        public override void Bind(EditAreaViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ViewModel.ContentHeight
                .Subscribe(height => contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height))
                .AddTo(this);

            // 1. 位置线逻辑
            ViewModel.PosLineCount.Subscribe(UpdatePosLines).AddTo(this);

            // 2. 节拍线重绘逻辑 (布局变化)
            Observable.CombineLatest(ViewModel.BeatAccuracy, ViewModel.BeatZoom, ViewModel.TotalBeats, (_, _, _) => true)
                .Subscribe(_ => ForceRebuildBeatLines()).AddTo(this);

            // 3. 滚动时刷新节拍线和音符
            scrollRect.onValueChanged.AsObservable()
                .Subscribe(_ =>
                {
                    UpdateBeatLinesVisibility();
                    UpdateNotesVisibility();
                }).AddTo(this);

            // 4. 音符数据或缩放变化时刷新音符
            Observable.Merge(
                    targetViewModel.Notes.ObserveChanged().Select(_ => Unit.Default),
                    targetViewModel.BeatZoom.Select(_ => Unit.Default),
                    targetViewModel.BeatAccuracy.Select(_ => Unit.Default)
                )
                .ThrottleLastFrame(1) // 避免同一帧多次刷新
                .Subscribe(_ => UpdateNotesVisibility())
                .AddTo(this);
        }

        #region PosLines

        private async void UpdatePosLines(int count)
        {
            if (Cts.IsCancellationRequested) return;
            int oldPosLineCount = posLinesFrameObject.transform.childCount - 1;

            var tasks = new List<Task<GameObject>>();
            for (int i = oldPosLineCount; i < count; i++)
            {
                tasks.Add(PoolManager.GetGameObjectAsync(PosLinePath, posLinesFrameObject.transform));
            }

            await Task.WhenAll(tasks);

            for (int i = oldPosLineCount; i > count; i--)
            {
                var go = posLinesFrameObject.transform.GetChild(i).gameObject;
                PoolManager.ReleaseGameObject(PosLinePath, go);
            }
        }

        #endregion

        #region BeatLines

        private void ForceRebuildBeatLines()
        {
            foreach (var kvp in ActiveBeatLines)
                if (kvp.Value != null)
                    PoolManager.ReleaseGameObject(BeatLinePath, kvp.Value);
            ActiveBeatLines.Clear();
            UpdateBeatLinesVisibility();
        }

        private async void UpdateBeatLinesVisibility()
        {
            if (Cts.IsCancellationRequested) return;

            // 计算 Content 底部为 0，向上增加
            // Viewport 可视区域在 Content 中的 Y 轴范围：
            float viewportHeight = viewportRect.rect.height;
            float contentHeight = contentRect.rect.height;

            // 当 verticalNormalizedPosition = 0 时，显示底部 (0 ~ viewportHeight)
            float scrollY = (contentHeight - viewportHeight) * scrollRect.verticalNormalizedPosition;
            scrollY = Mathf.Max(0, scrollY); // 防止负数

            float minVisibleY = scrollY - 100f;
            float maxVisibleY = scrollY + viewportHeight + 100f;

            float beatLineDist = ViewModel.GetBeatLineDistance();
            float judgeLineY = judgeLineRect.anchoredPosition.y;

            int minIndex = Mathf.FloorToInt((minVisibleY - judgeLineY) / beatLineDist);
            int maxIndex = Mathf.CeilToInt((maxVisibleY - judgeLineY) / beatLineDist);

            minIndex = Mathf.Max(0, minIndex);
            int maxTotalIndex = Mathf.FloorToInt(ViewModel.TotalBeats.CurrentValue * ViewModel.BeatAccuracy.CurrentValue);
            maxIndex = Mathf.Min(maxIndex, maxTotalIndex);

            // 回收
            List<int> toRemove = new List<int>();
            foreach (var kvp in ActiveBeatLines)
            {
                if (kvp.Key < minIndex || kvp.Key > maxIndex) toRemove.Add(kvp.Key);
            }

            foreach (int key in toRemove)
            {
                if (ActiveBeatLines.TryGetValue(key, out var go))
                {
                    if (go != null) PoolManager.ReleaseGameObject(BeatLinePath, go);
                    ActiveBeatLines.Remove(key);
                }
            }

            // 生成
            var tasks = new List<Task>();
            for (int i = minIndex; i <= maxIndex; i++)
            {
                if (ActiveBeatLines.TryAdd(i, null))
                {
                    tasks.Add(CreateBeatLine(i, beatLineDist, ViewModel.BeatAccuracy.CurrentValue));
                }
            }

            await Task.WhenAll(tasks);
        }

        private async Task CreateBeatLine(int index, float distance, int accuracy)
        {
            GameObject go = await PoolManager.GetGameObjectAsync(BeatLinePath, contentRect, Cts.Token);
            if (Cts.Token.IsCancellationRequested || !ActiveBeatLines.ContainsKey(index))
            {
                PoolManager.ReleaseGameObject(BeatLinePath, go);
                return;
            }

            if (ActiveBeatLines[index] is not null) PoolManager.ReleaseGameObject(BeatLinePath, ActiveBeatLines[index]);

            ActiveBeatLines[index] = go;
            if (go.TryGetComponent<BeatLineItem>(out var item))
            {
                // 手动设置位置，或者封装在 BeatLineItem 中
                if (go.transform is RectTransform rect)
                {
                    rect.anchorMin = new Vector2(0.5f, 0f);
                    rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.localScale = Vector3.one;
                    rect.anchoredPosition = new Vector2(0, judgeLineRect.anchoredPosition.y + (index * distance));
                }

                item.SetVisuals(index, accuracy);
            }
        }

        #endregion

        #region Notes

        private async void UpdateNotesVisibility()
        {
            if (Cts.IsCancellationRequested)
                return;

            float viewportHeight = viewportRect.rect.height;
            float contentHeight = contentRect.rect.height;

            float scrollY = (contentHeight - viewportHeight) * scrollRect.verticalNormalizedPosition;
            scrollY = Mathf.Max(0, scrollY);

            float viewMinY = scrollY - 100f;
            float viewMaxY = scrollY + viewportHeight + 100f;

            float beatDist = ViewModel.GetBeatLineDistance();
            float judgeLineY = judgeLineRect.anchoredPosition.y;

            float minVisibleFBeatVal = (viewMinY - judgeLineY) / beatDist;
            float maxVisibleFBeatVal = (viewMaxY - judgeLineY) / beatDist;

            var visibleNotes = new HashSet<BaseChartNoteData>();

            var allNotes = ViewModel.Notes;
            var holdNotes = ViewModel.HoldNotes;

            // 二分法更新所有“判定拍位于可视范围内的音符”的可见性
            int startIndex = FindLowerBound(allNotes, minVisibleFBeatVal);

            for (int i = startIndex; i < allNotes.Count; i++)
            {
                var note = allNotes[i];

                if (note.JudgeBeat.ToFloat() > maxVisibleFBeatVal)
                    break;

                visibleNotes.Add(note);
            }

            // 检查可视范围前所有的 HoldNote，如果这些音符尾判延伸进了可视范围，也一并渲染
            int holdLimitIndex = FindLowerBound(holdNotes, minVisibleFBeatVal);
            for (int i = 0; i < holdLimitIndex; i++)
            {
                var hold = holdNotes[i];
                // Hold 的开始时间一定 < Min (因为 i < holdLimitIndex)
                // 只要结束时间 >= Min，就是可见的
                if (hold.EndJudgeBeat.ToFloat() >= minVisibleFBeatVal)
                {
                    visibleNotes.Add(hold);
                }
            }

            // 对比 diff，回收在本帧移出可视范围的 notes
            var toRemove = new List<BaseChartNoteData>();
            foreach (var kvp in ActiveNotes)
            {
                if (!visibleNotes.Contains(kvp.Key))
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var note in toRemove)
            {
                if (ActiveNotes.TryGetValue(note, out var pair))
                {
                    if (pair != null)
                    {
                        var (vm, view) = pair.Value;
                        vm.Dispose(); // 销毁 VM
                        PoolManager.ReleaseGameObject(GetPrefabPath(note.Type), view.gameObject);
                    }

                    ActiveNotes.Remove(note);
                }
            }

            // 对比 diff，生成本帧新出现的音符
            var tasks = new List<Task>();
            foreach (var note in visibleNotes)
            {
                if (!ActiveNotes.ContainsKey(note))
                {
                    ActiveNotes.Add(note, null); // 占位，防止重复创建
                    tasks.Add(CreateNoteObject(note));
                }
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }
        }

        /// <summary>
        /// 二分查找：找到第一个 JudgeBeat.ToFloat() >= targetBeat 的索引
        /// </summary>
        private int FindLowerBound<T>(IReadOnlyList<T> list, float targetBeat) where T : BaseChartNoteData
        {
            int low = 0;
            int high = list.Count - 1;
            int result = list.Count; // 默认为 Count，表示所有元素都比 target 小

            while (low <= high)
            {
                int mid = low + (high - low) / 2;
                if (list[mid].JudgeBeat.ToFloat() >= targetBeat)
                {
                    result = mid;
                    high = mid - 1;
                }
                else
                {
                    low = mid + 1;
                }
            }

            return result;
        }

        private async Task CreateNoteObject(BaseChartNoteData note)
        {
            string path = GetPrefabPath(note.Type);

            GameObject go = await PoolManager.GetGameObjectAsync(path, contentRect, Cts.Token);

            // 双重检查：异步加载过程中可能已经不再需要显示该 Note，或者 View 被销毁
            if (Cts.Token.IsCancellationRequested || !ActiveNotes.ContainsKey(note))
            {
                PoolManager.ReleaseGameObject(path, go);
                return;
            }

            // 清理旧对象（理论上 ActiveNotes[note] 此时应为 null，作为防御性编程）
            if (ActiveNotes[note] is { } oldPair)
            {
                oldPair.vm.Dispose();
                PoolManager.ReleaseGameObject(path, oldPair.view.gameObject);
            }

            if (go.TryGetComponent<EditAreaNoteView>(out var view))
            {
                var vm = ViewModel.CreateNoteViewModel(note, judgeLineRect.anchoredPosition.y);

                view.Bind(vm);
                ActiveNotes[note] = (vm, view);
            }
            else
            {
                Debug.LogError($"Prefab at {path} missing EditAreaNoteView component!");
                PoolManager.ReleaseGameObject(path, go);
                ActiveNotes.Remove(note);
            }
        }

        private string GetPrefabPath(NoteType type) => type switch
        {
            NoteType.Tap => TapNotePath,
            NoteType.Drag => DragNotePath,
            NoteType.Hold => HoldNotePath,
            NoteType.Click => ClickNotePath,
            NoteType.Break => BreakNotePath,
            _ => throw new NotSupportedException()
        };

        #endregion

        #region Input

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!ViewModel.CanPutNote.CurrentValue)
                return;

            // 将屏幕点击坐标转换为 Content 内的局部坐标
            // 由于 Content 的轴心是 (0.5, 0)
            // localPoint.y 即为距离底部的像素距离
            // localPoint.x 为距离中心线的水平距离
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                contentRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );

            var result = ViewModel.CalculateNotePlacement(localPoint, judgeLineRect.anchoredPosition.y);

            // 如果点到间隙（result == null）就不处理
            if (result != null)
            {
                ViewModel.CreateNote(result.Value.pos, result.Value.beat);
            }
        }

        #endregion

        protected override void OnDestroy()
        {
            Cts.Cancel();
            Cts.Dispose();

            // 清理节拍线
            foreach (var kvp in ActiveBeatLines)
                if (kvp.Value is not null)
                    PoolManager.ReleaseGameObject(BeatLinePath, kvp.Value);
            ActiveBeatLines.Clear();

            // 清理音符
            foreach (var kvp in ActiveNotes)
            {
                if (kvp.Value != null)
                {
                    var (vm, view) = kvp.Value.Value;
                    vm.Dispose();
                    PoolManager.ReleaseGameObject(GetPrefabPath(kvp.Key.Type), view.gameObject);
                }
            }

            ActiveNotes.Clear();
        }
    }
}
