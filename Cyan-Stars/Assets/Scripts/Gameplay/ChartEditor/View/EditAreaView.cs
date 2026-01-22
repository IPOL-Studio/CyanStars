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
            if (Cts.IsCancellationRequested) return;

            float viewportHeight = viewportRect.rect.height;
            float contentHeight = contentRect.rect.height;
            float scrollY = (contentHeight - viewportHeight) * scrollRect.verticalNormalizedPosition;
            scrollY = Mathf.Max(0, scrollY);

            float viewMinY = scrollY - 100f;
            float viewMaxY = scrollY + viewportHeight + 100f;

            var notesToShow = new List<BaseChartNoteData>();
            var notesToRemove = new List<BaseChartNoteData>();

            // 遍历所有音符判断可见性
            foreach (var note in ViewModel.Notes)
            {
                float startY = CalculateNoteY(note.JudgeBeat);
                float endY = startY;
                if (note is HoldChartNoteData hold)
                {
                    endY = CalculateNoteY(hold.EndJudgeBeat);
                }

                // 检查音符是否在可视范围附近
                bool isVisible = (endY >= viewMinY) && (startY <= viewMaxY);
                if (isVisible)
                {
                    if (!ActiveNotes.ContainsKey(note))
                        notesToShow.Add(note);
                }
                else
                {
                    if (ActiveNotes.ContainsKey(note))
                        notesToRemove.Add(note);
                }
            }

            // 回收
            foreach (var note in notesToRemove)
            {
                if (ActiveNotes.TryGetValue(note, out var pair))
                {
                    if (pair != null)
                    {
                        var (vm, view) = pair.Value;
                        vm.Dispose(); // 销毁 VM

                        // 归还 View 的 GameObject 到对象池
                        PoolManager.ReleaseGameObject(GetPrefabPath(note.Type), view.gameObject);
                    }

                    ActiveNotes.Remove(note);
                }
            }

            // 生成
            var tasks = new List<Task>();
            foreach (var note in notesToShow)
            {
                ActiveNotes.Add(note, null); // 占位
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

            // 如果之前有旧的对象（理应是 null，但防守式编程），先清理
            if (ActiveNotes[note] is { } oldPair)
            {
                oldPair.vm.Dispose();
                PoolManager.ReleaseGameObject(path, oldPair.view.gameObject);
            }

            // 获取组件并初始化 VM/View
            if (go.TryGetComponent<EditAreaNoteView>(out var view))
            {
                // 使用工厂方法创建子 ViewModel，解决 protected 成员访问问题
                var vm = ViewModel.CreateNoteViewModel(note, judgeLineRect.anchoredPosition.y);

                // 绑定
                view.Bind(vm);

                // 存储引用
                ActiveNotes[note] = (vm, view);
            }
            else
            {
                Debug.LogError($"Prefab at {path} does not have an EditAreaNoteView component!");
                PoolManager.ReleaseGameObject(path, go);
                ActiveNotes.Remove(note);
            }
        }

        /// <summary>
        /// 根据 Beat 计算在 Content 中的 Y 轴位置 (相对于 Content 底部)
        /// 用于可见性剔除的估算
        /// </summary>
        private float CalculateNoteY(Beat beat)
        {
            // Note VM 内部计算位置使用的是 DefaultMajorBeatLineInterval * Zoom
            // 这里为了可见性剔除，逻辑需要保持一致
            float beatInterval = EditAreaViewModel.DefaultMajorBeatLineInterval * ViewModel.BeatZoom.CurrentValue;
            float yPos = judgeLineRect.anchoredPosition.y + (beat.ToFloat() * beatInterval);
            return yPos;
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
