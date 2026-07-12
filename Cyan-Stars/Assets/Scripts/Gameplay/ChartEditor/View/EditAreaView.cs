// TODO: 待重构

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
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
        private CustomScrollRect scrollRect = null!;

        [SerializeField]
        private RectTransform judgeLineRect = null!;

        [SerializeField]
        private RawImage audioWaveRaoImage = null!;


        private readonly Color AudioWaveColor = new Color(1, 1, 1, 0.05f);

        private readonly CancellationTokenSource Cts = new CancellationTokenSource();
        private static GameObjectPoolManager PoolManager => GameRoot.GameObjectPool;

        // 管理当前激活的节拍线：Key=节拍索引（含细分拍），Value=节拍线物体实例
        // 开始加载时会将 item 对应的 Value 设为 null 占位，加载完成后覆写为 gameObject
        private readonly Dictionary<int, GameObject?> ActiveBeatLines = new Dictionary<int, GameObject?>();

        // 管理当前激活的音符: Key=音符数据对象, Value=(ViewModel, View)
        private readonly Dictionary<BaseChartNoteData, (EditAreaNoteViewModel vm, EditAreaNoteView view)?> ActiveNotes =
            new Dictionary<BaseChartNoteData, (EditAreaNoteViewModel, EditAreaNoteView)?>();

        // 防止拖拽/滚动 scrollRect 更新 time 后再做一次无意义的 scrollRect 位置更新
        private bool isTimelineTimeChangeBySelf = false;


        public override void Bind(EditAreaViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

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

            Observable.CombineLatest(
                    ViewModel.AudioClipHandler,
                    ViewModel.FirstMusicVersionItemOffset,
                    ViewModel.TotalBeats,
                    (handler, offset, _) => (handler, offset: offset ?? 0)
                )
                .ThrottleLastFrame(1) // 避免同一帧多次刷新
                .Subscribe(async t =>
                {
                    if (t.handler != null && t.handler.IsDoing)
                        await t.handler;

                    DrawAudioWave(t.handler?.Asset, t.offset);
                })
                .AddTo(this);

            ViewModel.BeatZoom
                .Subscribe(_ => RefreshFramesAnchor())
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
                .Subscribe(_ => RebuildBeatLines()).AddTo(this);

            // 3. 滚动时刷新节拍线和音符可见性，如果没在播放音乐则一并更新时间轴时间
            scrollRect.onValueChanged.AsObservable()
                .Subscribe(_ =>
                {
                    UpdateBeatLinesVisibility();
                    UpdateNotesVisibility();
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
        }

        #region PosLines

        private async void UpdatePosLines(int count)
        {
            if (Cts.IsCancellationRequested) return;
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

        private void RebuildBeatLines()
        {
            foreach (var kvp in ActiveBeatLines)
                if (kvp.Value != null)
                    PoolManager.ReleaseGameObject(ChartEditorAssetHelper.BeatLinePath, kvp.Value);
            ActiveBeatLines.Clear();
            UpdateBeatLinesVisibility();
        }

        private async void UpdateBeatLinesVisibility()
        {
            if (Cts.IsCancellationRequested) return;

            // 计算 Content 底部为 0，向上增加
            // Viewport 可视区域在 Content 中的 Y 轴范围：
            float viewportHeight = viewportRect.rect.height;

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
            GameObject go = await PoolManager.GetGameObjectAsync(ChartEditorAssetHelper.BeatLinePath, beatLinesFrameRect, Cts.Token);
            go.transform.localScale = Vector3.one;

            if (Cts.Token.IsCancellationRequested || !ActiveBeatLines.ContainsKey(index))
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
            if (Cts.IsCancellationRequested)
                return;

            float viewportHeight = viewportRect.rect.height;

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

            GameObject go = await PoolManager.GetGameObjectAsync(path, notesFrameRect, Cts.Token);
            go.transform.localScale = Vector3.one;

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

        #region AudioWave

        private void DrawAudioWave(AudioClip? clip, int musicOffset)
        {
            RefreshFramesAnchor();

            var rectTransform = (RectTransform)audioWaveRaoImage.transform;

            int uiWidth = Mathf.CeilToInt(rectTransform.rect.width);
            int uiHeight = Mathf.CeilToInt(rectTransform.rect.height);

            // 防止宽高度异常导致报错
            if (uiWidth <= 0 || uiHeight <= 0) return;

            // 限制最大纹理尺寸
            int maxTexSize = Mathf.Min(SystemInfo.maxTextureSize, 8192);
            int texWidth = Mathf.Clamp(uiWidth, 1, maxTexSize);
            int texHeight = Mathf.Clamp(uiHeight, 1, maxTexSize);

            Texture2D texture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);

            // 计算 UI 实际尺寸 与 纹理贴图 之间的缩放比例
            float scaleX = (float)uiWidth / texWidth;
            float scaleY = (float)uiHeight / texHeight;

            // 背景填充透明色
            Color[] pixels = new Color[texWidth * texHeight];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;

            // 波形纵轴需与时间轴布局保持一致：AudioWave 铺满 Content，
            // 且节拍线/音符均以 “判定线高度 + 拍数 * 每拍像素” 的方式从 Content 底部排布，
            // 因此像素 -> 拍 -> （经 Bpm 组换算）时间，才能与时间轴对齐。
            var bpmGroup = ViewModel.BpmGroup;
            double majorBeatDist = ViewModel.GetMajorBeatLineDistance();

            // 无音频、缩放异常或 Bpm 组不合法时，仅绘制透明背景
            if (clip != null
                && majorBeatDist > 0
                && bpmGroup.Count > 0
                && BpmGroupHelper.Validate(bpmGroup) == BpmGroupHelper.BpmValidationStatus.Valid)
            {
                // 获取数据
                float[] fullSamples = new float[clip.samples * clip.channels];
                clip.GetData(fullSamples, 0);

                float offsetSeconds = musicOffset / 1000f;
                int centerTexX = texWidth / 2;

                // 搜索峰值，并按峰值拉伸填充像素
                for (int y = 0; y < texHeight; y++)
                {
                    // 1. 计算当前像素行 (y) 映射到 Content 上的高度范围，
                    //    先换算为拍，再通过 Bpm 组求出对应的音频起止时间
                    float uiYStart = y * scaleY;
                    float uiYEnd = (y + 1) * scaleY;

                    double judgeLineY = judgeLineRect.anchoredPosition.y;
                    double beatStart = (uiYStart - judgeLineY) / majorBeatDist;
                    double beatEnd = (uiYEnd - judgeLineY) / majorBeatDist;

                    float startTime = (BpmGroupHelper.CalculateTime(bpmGroup, beatStart) / 1000f) - offsetSeconds;
                    float endTime = (BpmGroupHelper.CalculateTime(bpmGroup, beatEnd) / 1000f) - offsetSeconds;

                    // 2. 转换为 AudioClip 的 sample 索引
                    int startSample = Mathf.FloorToInt(startTime * clip.frequency);
                    int endSample = Mathf.CeilToInt(endTime * clip.frequency);

                    startSample = Mathf.Clamp(startSample, 0, clip.samples - 1);
                    endSample = Mathf.Clamp(endSample, 0, clip.samples - 1);

                    float maxPeak = 0f;

                    // 3. 搜索当前时间片段内的峰值振幅
                    if (startSample < endSample)
                    {
                        int startIdx = startSample * clip.channels;
                        int endIdx = endSample * clip.channels;
                        endIdx = Mathf.Min(endIdx, fullSamples.Length);

                        for (int i = startIdx; i < endIdx; i++)
                        {
                            float absVal = Mathf.Abs(fullSamples[i]);
                            if (absVal > maxPeak)
                            {
                                maxPeak = absVal;
                            }
                        }
                    }

                    // 4. 按峰值拉伸并填充像素
                    // 先算出 UI 上的波形宽度，再除以 scaleX 压缩为 Texture 里的像素宽度
                    float uiWaveWidth = maxPeak * (uiWidth / 2f) * 0.5f; // 只填充 50%
                    int texWaveWidth = Mathf.FloorToInt(uiWaveWidth / scaleX);

                    int startX = Mathf.Clamp(centerTexX - texWaveWidth, 0, texWidth - 1);
                    int endX = Mathf.Clamp(centerTexX + texWaveWidth, 0, texWidth - 1);

                    // 写入一维颜色数组
                    for (int x = startX; x <= endX; x++)
                    {
                        pixels[y * texWidth + x] = AudioWaveColor;
                    }
                }
            }

            // 绘制材质
            texture.SetPixels(pixels);
            texture.Apply(); // 应用像素更改提交到 GPU

            // 释放旧的 Texture
            if (audioWaveRaoImage.texture != null)
                Destroy(audioWaveRaoImage.texture);

            audioWaveRaoImage.texture = texture;
        }

        #endregion

        #region Input

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
            if (!Input.GetKeyDown(KeyCode.Space))
                return;

            if (ViewModel.OpenCanvasCount >= 1)
                return;

            if (EventSystem.current.currentSelectedGameObject != null &&
                EventSystem.current.currentSelectedGameObject.TryGetComponent(out TMP_InputField _))
                return; // 焦点位于输入框时拦截 Space 响应

            ViewModel.OnSpaceDown();
        }

        private void RefreshFramesAnchor()
        {
            // TODO: 目前仅供 AudioWave 使用，后续可考虑重构接入 BeatLinesFrame 和 NoteLinesFrame 以优化缩放时卡顿
            var rectTransform = (RectTransform)audioWaveRaoImage.transform;
            double judgeLineY = judgeLineRect.anchoredPosition.y;
            rectTransform.anchorMin = new Vector2(0, (float)(judgeLineY / contentRect.rect.height));
            rectTransform.anchorMax = new Vector2(1, (contentRect.rect.height - viewportRect.rect.height + (float)judgeLineY) / contentRect.rect.height);
        }

        protected void OnDestroy()
        {
            Cts.Cancel();
            Cts.Dispose();

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
        }
    }
}
