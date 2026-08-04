// TODO: 待重构

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using CyanStars.Utils;
using DG.Tweening;
using Gameplay.ChartEditor;
using ObservableCollections;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GameObjectPoolManager = CyanStars.Framework.GameObjectPool.GameObjectPoolManager;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class EditAreaView : BaseView<EditAreaViewModel>, IPointerDownHandler
    {
        [SerializeField]
        private Image centerTrackHighlightImage = null!;

        [SerializeField]
        private Image sideTrackHighlightImage = null!;

        [SerializeField]
        private GameObject posLinesFrameObject = null!;

        [SerializeField]
        private RectTransform viewportRect = null!;

        [SerializeField]
        private RectTransform contentRect = null!;

        [SerializeField]
        private RectTransform beatLinesFrameRect = null!;

        [SerializeField]
        private RectTransform notesFrameRect = null!;

        [SerializeField]
        private RectTransform chartTracebackNotesFrameRect = null!;

        [SerializeField]
        [Range(0f, 1f)]
        private float chartTracebackNotesAlpha = 0.4f;

        [SerializeField]
        private RectTransform ghostNoteFrameRect = null!;

        [SerializeField]
        private CustomScrollRect scrollRect = null!;

        [SerializeField]
        private RectTransform judgeLineRect = null!;


        private static GameObjectPoolManager PoolManager => GameRoot.GameObjectPool;

        // 管理当前激活的节拍线：Key=节拍索引（含细分拍），Value=节拍线物体实例
        // 开始加载时会将 item 对应的 Value 设为 null 占位，加载完成后覆写为 gameObject
        private readonly Dictionary<int, GameObject?> ActiveBeatLines = new Dictionary<int, GameObject?>();

        // 管理当前激活的音符: Key=音符数据对象, Value=(ViewModel, View)
        private readonly Dictionary<BaseChartNoteData, (EditAreaNoteViewModel vm, EditAreaNoteView view)?> ActiveNotes =
            new Dictionary<BaseChartNoteData, (EditAreaNoteViewModel, EditAreaNoteView)?>();

        // 管理当前激活的谱面回溯虚影音符: Key=音符数据对象, Value=(ViewModel, View)
        private readonly Dictionary<BaseChartNoteData, (EditAreaNoteViewModel vm, EditAreaNoteView view)?> ActiveChartTracebackNotes =
            new Dictionary<BaseChartNoteData, (EditAreaNoteViewModel, EditAreaNoteView)?>();

        // 防止拖拽/滚动 scrollRect 更新 time 后再做一次无意义的 scrollRect 位置更新
        private bool isTimelineTimeChangeBySelf = false;

        // == 悬停预览音符 ==
        private const float PreviewNoteAlpha = 0.39f; // 预览音符的整体透明度

        private GameObject? notePreviewObject;   // 已创建好的预览音符物体
        private RectTransform? notePreviewRect;  // 预览音符物体的 RectTransform
        private NoteType? notePreviewType;       // 当前期望的预览音符类型（null = 不显示预览）
        private bool isNotePreviewLoading;       // 是否正在异步加载预览物体
        private int notePreviewGeneration;       // 预览物体创建代次，用于丢弃过期的异步加载结果

        // 预览音符各 Graphic 的原始状态（对象池不会重置组件状态，释放前需还原）
        private readonly List<MaskableGraphic> NotePreviewGraphics = new List<MaskableGraphic>();
        private readonly List<Color> NotePreviewGraphicColors = new List<Color>();
        private readonly List<bool> NotePreviewGraphicRaycastTargets = new List<bool>();

        private readonly PointerEventData HoverPointerData = new PointerEventData(null); // 悬停射线检测复用
        private readonly List<RaycastResult> HoverRaycastResults = new List<RaycastResult>();


        public override void Bind(EditAreaViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ConfigureChartTracebackLayer();

            ViewModel.IsTimelinePlaying
                .Subscribe(isPlaying => scrollRect.vertical = !isPlaying) // 正在播放时完全禁止拖动/滚动 scrollRect
                .AddTo(this);
            ViewModel.SelectedEditTool
                .Subscribe(tool =>
                {
                    // 只有为“选择”工具时才允许拖动 scrollRect
                    scrollRect.IsDragEnabled = tool == EditToolType.Select;

                    centerTrackHighlightImage.DOKill();
                    centerTrackHighlightImage.DOFade(tool == EditToolType.BreakPen ? 0 : 0.39f, 0.1f);
                    sideTrackHighlightImage.DOKill();
                    sideTrackHighlightImage.DOFade(
                        tool is EditToolType.Select or EditToolType.BreakPen or EditToolType.Eraser ? 0.39f : 0,
                        0.1f
                    );
                })
                .AddTo(this);
            ViewModel.ContentExtraHeight
                .Subscribe(addHeight =>
                {
                    var verticalNormalizedPosition = scrollRect.verticalNormalizedPosition;
                    contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (float)(viewportRect.rect.height + addHeight));
                    scrollRect.verticalNormalizedPosition = verticalNormalizedPosition;
                })
                .AddTo(this);
            ViewModel.CurrentTimelineTimeMs
                .Subscribe(_ =>
                {
                    if (isTimelineTimeChangeBySelf)
                        return;

                    scrollRect.SetNormalizedPositionWithoutNotify(
                        new Vector2(0, ViewModel.GetNormalizedPositionYByTimelineTime())
                    );
                    UpdateBeatLinesVisibility();
                    UpdateNotesVisibility();
                    UpdateChartTracebackNotesVisibility();
                })
                .AddTo(this);

            ViewModel.IsCompactNoteButtonArea
                .Subscribe(value =>
                {
                    foreach (var kvp in ActiveNotes)
                    {
                        if (kvp.Value == null)
                            continue;

                        kvp.Value!.Value.view.SetBlurImageRaycastTarget(!value);
                    }
                })
                .AddTo(this);


            // 1. 位置线逻辑
            ViewModel.PosLineCount.Subscribe(UpdatePosLines).AddTo(this);

            // 2. 节拍线重绘逻辑 (布局变化)
            Observable.CombineLatest(
                    ViewModel.BeatAccuracy,
                    ViewModel.BeatZoom,
                    ViewModel.TotalBeats,
                    (_, _, _) => true
                )
                .ThrottleLastFrame(1) // 避免同一帧多次刷新
                .Subscribe(_ => ForceRebuildBeatLines()).AddTo(this);

            // 3. 滚动时刷新节拍线和音符，如果没在播放音乐则一并更新时间轴时间
            scrollRect.onValueChanged.AsObservable()
                .Subscribe(_ =>
                {
                    UpdateBeatLinesVisibility();
                    UpdateNotesVisibility();
                    UpdateChartTracebackNotesVisibility();
                    if (!ViewModel.IsTimelinePlaying.CurrentValue) // 正在播放时由 ChartEditorMusicManager 更新时间
                    {
                        isTimelineTimeChangeBySelf = true;
                        ViewModel.TryUpdateTimelineTime(scrollRect.normalizedPosition.y);
                        isTimelineTimeChangeBySelf = false;
                    }
                })
                .AddTo(this);

            // 4. 音符列表、缩放、选中音符的位置或节拍变化时刷新音符
            Observable.Merge(
                    ViewModel.Notes.ObserveChanged().Select(_ => Unit.Default),
                    ViewModel.BeatZoom.Select(_ => Unit.Default),
                    ViewModel.SelectedNoteDataChangedSubject.Select(_ => Unit.Default)
                )
                .ThrottleLastFrame(1) // 避免同一帧多次刷新
                .Subscribe(_ => UpdateNotesVisibility())
                .AddTo(this);

            // 5. 谱面回溯虚影：开关、音符列表、缩放、选中音符的位置或节拍变化、回溯 beat 变化时刷新
            Observable.Merge(
                    ViewModel.Notes.ObserveChanged().Select(_ => Unit.Default),
                    ViewModel.BeatZoom.Select(_ => Unit.Default),
                    ViewModel.SelectedNoteDataChangedSubject.Select(_ => Unit.Default),
                    ViewModel.ChartTracebackBeatOffset.Select(_ => Unit.Default),
                    ViewModel.IsChartTracebackEnabled.Select(_ => Unit.Default)
                )
                .ThrottleLastFrame(1) // 避免同一帧多次刷新
                .Subscribe(_ => UpdateChartTracebackNotesVisibility())
                .AddTo(this);

            GameRoot.Event.AddListener(Background.ClickEventName, OnBackgroundClick);
        }

        private void ConfigureChartTracebackLayer()
        {
            var canvasGroup = chartTracebackNotesFrameRect.GetComponent<CanvasGroup>();
            canvasGroup.alpha = Mathf.Clamp01(chartTracebackNotesAlpha);
        }

        #region PosLines

        private async void UpdatePosLines(int count)
        {
            if (destroyCancellationToken.IsCancellationRequested) return;
            int oldPosLineCount = posLinesFrameObject.transform.childCount - 1;

            var tasks = new List<Task>();
            for (int i = oldPosLineCount; i < count; i++)
            {
                tasks.Add(CreatePosLine());
            }

            await Task.WhenAll(tasks);

            for (int i = oldPosLineCount; i > count; i--)
            {
                var go = posLinesFrameObject.transform.GetChild(i).gameObject;
                PoolManager.ReleaseGameObject(ChartEditorAssetHelper.PosLinePath, go);
            }
        }

        private async Task CreatePosLine()
        {
            GameObject go = await PoolManager.GetGameObjectAsync(ChartEditorAssetHelper.PosLinePath, posLinesFrameObject.transform);
            go.transform.localPosition = Vector3.one;
        }

        #endregion

        #region BeatLines

        private void ForceRebuildBeatLines()
        {
            foreach (var kvp in ActiveBeatLines)
                if (kvp.Value != null)
                    PoolManager.ReleaseGameObject(ChartEditorAssetHelper.BeatLinePath, kvp.Value);
            ActiveBeatLines.Clear();
            UpdateBeatLinesVisibility();
        }

        private async void UpdateBeatLinesVisibility()
        {
            if (destroyCancellationToken.IsCancellationRequested) return;

            // 计算 Content 底部为 0，向上增加
            // Viewport 可视区域在 Content 中的 Y 轴范围：
            float viewportHeight = viewportRect.rect.height;
            float contentHeight = contentRect.rect.height;

            // 当 verticalNormalizedPosition = 0 时，显示底部 (0 ~ viewportHeight)
            float scrollY = Mathf.Max(0, -contentRect.anchoredPosition.y);

            float minVisibleY = scrollY - 100f;
            float maxVisibleY = scrollY + viewportHeight + 100f;

            double beatLineDist = EditAreaViewHelper.GetMinorBeatLineDistance(ViewModel.BeatAccuracy.CurrentValue, ViewModel.BeatZoom.CurrentValue);
            float judgeLineY = judgeLineRect.anchoredPosition.y;

            int minIndex = (int)Math.Floor((minVisibleY - judgeLineY) / beatLineDist);
            int maxIndex = (int)Math.Ceiling((maxVisibleY - judgeLineY) / beatLineDist);

            minIndex = Mathf.Max(0, minIndex);
            int maxTotalIndex = (int)Math.Floor(ViewModel.TotalBeats.CurrentValue * ViewModel.BeatAccuracy.CurrentValue);
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
                    if (go != null) PoolManager.ReleaseGameObject(ChartEditorAssetHelper.BeatLinePath, go);
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

        private async Task CreateBeatLine(int index, double distance, int accuracy)
        {
            GameObject go = await PoolManager.GetGameObjectAsync(ChartEditorAssetHelper.BeatLinePath, beatLinesFrameRect, destroyCancellationToken);
            go.transform.localScale = Vector3.one;

            if (destroyCancellationToken.IsCancellationRequested || !ActiveBeatLines.ContainsKey(index))
            {
                PoolManager.ReleaseGameObject(ChartEditorAssetHelper.BeatLinePath, go);
                return;
            }

            if (ActiveBeatLines[index] is not null) PoolManager.ReleaseGameObject(ChartEditorAssetHelper.BeatLinePath, ActiveBeatLines[index]);

            ActiveBeatLines[index] = go;
            if (go.TryGetComponent<BeatLineItem>(out var item))
            {
                // 手动设置位置，或者封装在 BeatLineItem 中
                if (go.transform is RectTransform rect)
                {
                    rect.anchorMin = new Vector2(0.5f, 0f);
                    rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.localScale = Vector3.one;
                    rect.anchoredPosition = new Vector2(0, (float)(judgeLineRect.anchoredPosition.y + (index * distance)));
                }

                item.SetVisuals(index, accuracy);
            }
        }

        #endregion

        #region Notes

        private async void UpdateNotesVisibility()
        {
            if (destroyCancellationToken.IsCancellationRequested)
                return;

            float viewportHeight = viewportRect.rect.height;
            float contentHeight = contentRect.rect.height;

            float scrollY = Mathf.Max(0, -contentRect.anchoredPosition.y);

            float viewMinY = scrollY - 100f;
            float viewMaxY = scrollY + viewportHeight + 100f;

            double beatDist = ViewModel.GetMajorBeatLineDistance();
            float judgeLineY = judgeLineRect.anchoredPosition.y;

            double minVisibleFBeatVal = (viewMinY - judgeLineY) / beatDist;
            double maxVisibleFBeatVal = (viewMaxY - judgeLineY) / beatDist;

            var visibleNotes = new HashSet<BaseChartNoteData>();

            var allNotes = ViewModel.Notes;
            var holdNotes = ViewModel.HoldNotes;

            // 二分法更新所有“判定拍位于可视范围内的音符”的可见性
            int startIndex = FindLowerBound(allNotes, minVisibleFBeatVal);

            for (int i = startIndex; i < allNotes.Count; i++)
            {
                var note = allNotes[i];

                if (note.JudgeBeat.ToDouble() > maxVisibleFBeatVal)
                    break;

                visibleNotes.Add(note);
            }

            // 检查所有的 HoldNote，如果这些音符任何部分位于可视范围内，也一并渲染
            // TODO: 维护一个按 JudgeBeat 有序排列的列表以使用二分查找提高性能
            foreach (var holdNote in holdNotes)
            {
                if (holdNote.JudgeBeat.ToDouble() <= maxVisibleFBeatVal &&
                    holdNote.EndJudgeBeat.ToDouble() >= minVisibleFBeatVal)
                {
                    visibleNotes.Add(holdNote);
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
        /// 关闭谱面回溯时，清空所有已创建的谱面回溯虚影音符
        /// </summary>
        private void ClearChartTracebackNotes()
        {
            if (ActiveChartTracebackNotes.Count == 0)
                return;

            foreach (var kvp in ActiveChartTracebackNotes)
            {
                if (kvp.Value == null)
                    continue;

                var (vm, view) = kvp.Value.Value;
                vm.Dispose(); // 销毁 VM
                PoolManager.ReleaseGameObject(GetPrefabPath(kvp.Key.Type), view.gameObject);
            }

            ActiveChartTracebackNotes.Clear();
        }

        /// <summary>
        /// 更新谱面回溯虚影音符的可见性。仅创建视窗附近的音符。
        /// </summary>
        private async void UpdateChartTracebackNotesVisibility()
        {
            if (destroyCancellationToken.IsCancellationRequested || chartTracebackNotesFrameRect == null)
                return;

            if (!ViewModel.IsChartTracebackEnabled.CurrentValue)
            {
                ClearChartTracebackNotes();
                return;
            }

            float viewportHeight = viewportRect.rect.height;

            float scrollY = Mathf.Max(0, -contentRect.anchoredPosition.y);

            float viewMinY = scrollY - 100f;
            float viewMaxY = scrollY + viewportHeight + 100f;

            double beatDist = ViewModel.GetMajorBeatLineDistance();
            float judgeLineY = judgeLineRect.anchoredPosition.y;

            // 谱面回溯 beat = 主谱面 beat - offset，因此反过来换算可见范围
            double beatOffset = ViewModel.ChartTracebackBeatOffset.CurrentValue;
            double minVisibleFBeatVal = (viewMinY - judgeLineY) / beatDist + beatOffset;
            double maxVisibleFBeatVal = (viewMaxY - judgeLineY) / beatDist + beatOffset;

            var visibleNotes = new HashSet<BaseChartNoteData>();

            var allNotes = ViewModel.Notes;
            var holdNotes = ViewModel.HoldNotes;

            // 常数偏移不会改变排序，二分仍然有效
            int startIndex = FindLowerBound(allNotes, minVisibleFBeatVal);

            for (int i = startIndex; i < allNotes.Count; i++)
            {
                var note = allNotes[i];

                if (note.JudgeBeat.ToDouble() > maxVisibleFBeatVal)
                    break;

                visibleNotes.Add(note);
            }

            // 检查所有的 HoldNote，如果虚影任何部分位于可视范围内，也一并渲染
            foreach (var holdNote in holdNotes)
            {
                if (holdNote.JudgeBeat.ToDouble() <= maxVisibleFBeatVal &&
                    holdNote.EndJudgeBeat.ToDouble() >= minVisibleFBeatVal)
                {
                    visibleNotes.Add(holdNote);
                }
            }

            // 对比 diff，回收在本帧移出可视范围的虚影 notes
            var toRemove = new List<BaseChartNoteData>();
            foreach (var kvp in ActiveChartTracebackNotes)
            {
                if (!visibleNotes.Contains(kvp.Key))
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var note in toRemove)
            {
                if (ActiveChartTracebackNotes.TryGetValue(note, out var pair))
                {
                    if (pair != null)
                    {
                        var (vm, view) = pair.Value;
                        vm.Dispose(); // 销毁 VM
                        PoolManager.ReleaseGameObject(GetPrefabPath(note.Type), view.gameObject);
                    }

                    ActiveChartTracebackNotes.Remove(note);
                }
            }

            // 对比 diff，生成本帧新出现的虚影音符
            var tasks = new List<Task>();
            foreach (var note in visibleNotes)
            {
                if (!ActiveChartTracebackNotes.ContainsKey(note))
                {
                    ActiveChartTracebackNotes.Add(note, null); // 占位，防止重复创建
                    tasks.Add(CreateChartTracebackNoteObject(note));
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
        private int FindLowerBound<T>(IReadOnlyList<T> list, double targetBeat) where T : BaseChartNoteData
        {
            int low = 0;
            int high = list.Count - 1;
            int result = list.Count; // 默认为 Count，表示所有元素都比 target 小

            while (low <= high)
            {
                int mid = low + (high - low) / 2;
                if (list[mid].JudgeBeat.ToDouble() >= targetBeat)
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

            GameObject go = await PoolManager.GetGameObjectAsync(path, notesFrameRect, destroyCancellationToken);
            go.transform.localScale = Vector3.one;

            // 双重检查：异步加载过程中可能已经不再需要显示该 Note，或者 View 被销毁
            if (destroyCancellationToken.IsCancellationRequested || !ActiveNotes.ContainsKey(note))
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

                view.SetBlurImageRaycastTarget(!ViewModel.IsCompactNoteButtonArea.CurrentValue);
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

        private async Task CreateChartTracebackNoteObject(BaseChartNoteData note)
        {
            string path = GetPrefabPath(note.Type);

            GameObject go = await PoolManager.GetGameObjectAsync(path, chartTracebackNotesFrameRect, destroyCancellationToken);
            go.transform.localScale = Vector3.one;

            // 双重检查：异步加载过程中可能已经不再需要显示该虚影 Note，或者 View 被销毁
            if (destroyCancellationToken.IsCancellationRequested || !ActiveChartTracebackNotes.ContainsKey(note))
            {
                PoolManager.ReleaseGameObject(path, go);
                return;
            }

            // 清理旧对象（理论上 ActiveChartTracebackNotes[note] 此时应为 null，作为防御性编程）
            if (ActiveChartTracebackNotes[note] is { } oldPair)
            {
                oldPair.vm.Dispose();
                PoolManager.ReleaseGameObject(path, oldPair.view.gameObject);
            }

            if (go.TryGetComponent<EditAreaNoteView>(out var view))
            {
                var vm = ViewModel.CreateChartTracebackNoteViewModel(note, judgeLineRect.anchoredPosition.y);

                // 虚影层不可交互，且由父级 CanvasGroup 统一半透明
                view.SetBlurImageRaycastTarget(false);
                view.Bind(vm);
                ActiveChartTracebackNotes[note] = (vm, view);
            }
            else
            {
                Debug.LogError($"Prefab at {path} missing EditAreaNoteView component!");
                PoolManager.ReleaseGameObject(path, go);
                ActiveChartTracebackNotes.Remove(note);
            }
        }

        private static string GetPrefabPath(NoteType type) => type switch
        {
            NoteType.Tap => ChartEditorAssetHelper.TapNotePath,
            NoteType.Drag => ChartEditorAssetHelper.DragNotePath,
            NoteType.Hold => ChartEditorAssetHelper.HoldNotePath,
            NoteType.Click => ChartEditorAssetHelper.ClickNotePath,
            NoteType.Break => ChartEditorAssetHelper.BreakNotePath,
            _ => throw new NotSupportedException()
        };

        #endregion

        #region NotePreview

        /// <summary>
        /// 当选中画笔工具且鼠标悬浮在编辑区时，在即将创建音符的位置显示半透明预览
        /// </summary>
        /// <remarks>
        /// 与点击创建共用同一套计算（射线检测、CalculateNotePlacement、CreateNoteData），
        /// 因此预览位置与真实点击创建的音符完全一致（轨道钳制、节拍吸附、位置吸附等）
        /// </remarks>
        private void UpdateNotePreview()
        {
            EditToolType tool = ViewModel.SelectedEditTool.CurrentValue;
            bool isPenTool = tool is EditToolType.TapPen or EditToolType.DragPen or EditToolType.HoldPen or EditToolType.ClickPen or EditToolType.BreakPen;

            if (!isPenTool || !ViewModel.CanPutNote.CurrentValue || EventSystem.current == null)
            {
                ReleaseNotePreview();
                return;
            }

            // 与点击使用相同的光线检测：悬停在已有音符或其他 UI 上时，点击不会创建音符，因此不显示预览
            HoverPointerData.position = Input.mousePosition;
            HoverRaycastResults.Clear();
            EventSystem.current.RaycastAll(HoverPointerData, HoverRaycastResults);

            if (HoverRaycastResults.Count == 0)
            {
                ReleaseNotePreview();
                return;
            }

            GameObject topmostHit = HoverRaycastResults[0].gameObject;
            if (!topmostHit.transform.IsChildOf(transform) || topmostHit.GetComponentInParent<EditAreaNoteView>() != null)
            {
                ReleaseNotePreview();
                return;
            }

            // 将屏幕坐标转换为 Content 内的局部坐标（与 OnPointerDown 一致）
            // 由于 Content 的轴心是 (0.5, 0)，localPoint.y 即为距离底部的像素距离
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    contentRect,
                    Input.mousePosition,
                    null,
                    out Vector2 localPoint
                ))
            {
                ReleaseNotePreview();
                return;
            }

            bool canPlace = EditAreaViewHelper.CalculateNotePlacement(
                localPoint,
                judgeLineRect.anchoredPosition.y,
                ViewModel.PosMagnetState.CurrentValue,
                ViewModel.PosAccuracy.CurrentValue,
                ViewModel.BeatAccuracy.CurrentValue,
                ViewModel.BeatZoom.CurrentValue,
                out float pos,
                out Beat beat
            );

            if (!canPlace)
            {
                ReleaseNotePreview();
                return;
            }

            BaseChartNoteData? noteData = ViewModel.CreateNoteData(tool, pos, beat);
            if (noteData == null)
            {
                ReleaseNotePreview();
                return;
            }

            EnsureNotePreviewObject(noteData.Type);

            if (notePreviewRect != null)
            {
                notePreviewRect.anchoredPosition = EditAreaViewHelper.CalculateNoteAnchoredPosition(
                    noteData,
                    judgeLineRect.anchoredPosition.y,
                    ViewModel.BeatZoom.CurrentValue
                );
            }
        }

        /// <summary>
        /// 确保存在指定类型的预览音符物体，类型变化时重建
        /// </summary>
        private void EnsureNotePreviewObject(NoteType type)
        {
            if (notePreviewType == type && (notePreviewObject != null || isNotePreviewLoading))
                return;

            ReleaseNotePreview();

            notePreviewType = type;
            isNotePreviewLoading = true;
            int generation = ++notePreviewGeneration;
            CreateNotePreviewObjectAsync(type, generation);
        }

        /// <summary>
        /// 异步创建预览音符物体，并将各 Graphic 设为半透明且不拦截射线
        /// </summary>
        private async void CreateNotePreviewObjectAsync(NoteType type, int generation)
        {
            string path = GetPrefabPath(type);

            GameObject go = await PoolManager.GetGameObjectAsync(path, ghostNoteFrameRect, destroyCancellationToken);
            go.transform.localScale = Vector3.one;

            // 双重检查：异步加载过程中预览类型或代次已变化，或 View 已销毁
            if (destroyCancellationToken.IsCancellationRequested || notePreviewType != type || generation != notePreviewGeneration)
            {
                PoolManager.ReleaseGameObject(path, go);
                return;
            }

            // 对象池不会重置组件状态，因此保存各 Graphic 的原始状态，再设为半透明并禁用射线拦截
            // 还原逻辑见 ReleaseNotePreview
            NotePreviewGraphics.Clear();
            NotePreviewGraphicColors.Clear();
            NotePreviewGraphicRaycastTargets.Clear();
            foreach (var graphic in go.GetComponentsInChildren<MaskableGraphic>(true))
            {
                NotePreviewGraphics.Add(graphic);
                NotePreviewGraphicColors.Add(graphic.color);
                NotePreviewGraphicRaycastTargets.Add(graphic.raycastTarget);

                Color color = graphic.color;
                color.a *= PreviewNoteAlpha;
                graphic.color = color;
                graphic.raycastTarget = false;
            }

            notePreviewObject = go;
            notePreviewRect = go.transform as RectTransform;
            isNotePreviewLoading = false;
        }

        /// <summary>
        /// 释放预览音符物体并复位预览状态
        /// </summary>
        private void ReleaseNotePreview()
        {
            if (notePreviewObject != null)
            {
                // 先还原各 Graphic 的透明度与射线拦截，再归还对象池
                for (int i = 0; i < NotePreviewGraphics.Count; i++)
                {
                    // 防御：还原前对象被意外销毁的情况
                    if (NotePreviewGraphics[i] != null)
                    {
                        NotePreviewGraphics[i].color = NotePreviewGraphicColors[i];
                        NotePreviewGraphics[i].raycastTarget = NotePreviewGraphicRaycastTargets[i];
                    }
                }

                NotePreviewGraphics.Clear();
                NotePreviewGraphicColors.Clear();
                NotePreviewGraphicRaycastTargets.Clear();

                PoolManager.ReleaseGameObject(GetPrefabPath(notePreviewType!.Value), notePreviewObject);
                notePreviewObject = null;
                notePreviewRect = null;
            }

            notePreviewType = null;
            isNotePreviewLoading = false;
            notePreviewGeneration++;
        }

        #endregion

        #region Input

        private void OnBackgroundClick(object sender, EventArgs args)
        {
            ViewModel.CancelSelectNote();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right || !ViewModel.CanPutNote.CurrentValue)
            {
                // 如果是右键点击到了非音符的空白区域，或当前没有设置音乐/BPM，则取消选中音符
                ViewModel.CancelSelectNote();
                return;
            }


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

            bool needCreateNote = EditAreaViewHelper.CalculateNotePlacement(
                localPoint,
                judgeLineRect.anchoredPosition.y,
                ViewModel.PosMagnetState.CurrentValue,
                ViewModel.PosAccuracy.CurrentValue,
                ViewModel.BeatAccuracy.CurrentValue,
                ViewModel.BeatZoom.CurrentValue,
                out float pos,
                out Beat beat
            );

            // 如果点到间隙就不处理
            if (needCreateNote)
            {
                ViewModel.CreateNote(pos, beat);
            }
        }

        #endregion

        private void Update()
        {
            UpdateNotePreview();

            if (!Input.GetKeyDown(KeyCode.Space))
                return;

            if (ViewModel.OpenCanvasCount >= 1)
                return;

            if (EventSystem.current.currentSelectedGameObject != null &&
                EventSystem.current.currentSelectedGameObject.TryGetComponent(out TMP_InputField _))
                return; // 焦点位于输入框时拦截 Space 响应

            ViewModel.OnSpaceDown();
        }

        protected void OnDestroy()
        {
            GameRoot.Event.RemoveListener(Background.ClickEventName, OnBackgroundClick);

            // 清理预览音符
            ReleaseNotePreview();

            // 清理节拍线
            foreach (var kvp in ActiveBeatLines)
                if (kvp.Value is not null)
                    PoolManager.ReleaseGameObject(ChartEditorAssetHelper.BeatLinePath, kvp.Value);
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

            // 清理谱面回溯虚影音符
            foreach (var kvp in ActiveChartTracebackNotes)
            {
                if (kvp.Value != null)
                {
                    var (vm, view) = kvp.Value.Value;
                    vm.Dispose();
                    PoolManager.ReleaseGameObject(GetPrefabPath(kvp.Key.Type), view.gameObject);
                }
            }

            ActiveChartTracebackNotes.Clear();
        }
    }
}
