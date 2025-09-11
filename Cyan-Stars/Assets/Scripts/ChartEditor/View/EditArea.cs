using System;
using CyanStars.Chart;
using UnityEngine;
using CyanStars.ChartEditor.Model;
using CyanStars.Framework;
using UnityEngine.UI;

namespace CyanStars.ChartEditor.View
{
    public class EditArea : BaseView
    {
        /// <summary>
        /// 一倍缩放时整数节拍线之间的间隔距离
        /// </summary>
        private const float DefaultBeatLineInterval = 250f;

        // 对象池使用的预制体
        private const string BeatLinePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BeatLine.prefab";
        private const string TapNotePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/TapNote.prefab";
        private const string DragNotePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/DragNote.prefab";
        private const string HoldNotePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/HoldNote.prefab";
        private const string ClickNotePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/ClickNote.prefab";
        private const string BreakNotePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BreakNote.prefab";


        [SerializeField]
        private RectTransform mainCanvaRect;

        [SerializeField]
        private GameObject tracks;

        [SerializeField]
        private GameObject posLines;

        [SerializeField]
        private GameObject beatLines;

        [SerializeField]
        private GameObject judgeLine;

        [SerializeField]
        private ScrollRect scrollRect;

        [SerializeField]
        private GameObject notes;


        private RectTransform contentRect;
        private RectTransform judgeLineRect;

        private int totalBeats; // 当前选中的音乐（含offset）的向上取整拍子总数量
        private float lastCanvaHeight; // 上次记录的 Canva 高度（刷新前）
        private float contentHeight; // 内容总高度


        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);

            // 缓存一些物体
            contentRect = scrollRect.content.GetComponent<RectTransform>();
            judgeLineRect = judgeLine.GetComponent<RectTransform>();

            ResetTotalBeats();
        }

        /// <summary>
        /// 画面变化后(而不是每帧)或在编辑器或音符属性修改后，重新绘制编辑器的节拍线、位置线、音符
        /// </summary>
        private void RefreshUI()
        {
            RefreshScrollRect();
            RefreshBeatLines();
            RefreshNotes();
        }

        private void RefreshScrollRect()
        {
            // 记录已滚动的位置百分比
            float verticalNormalizedPosition = scrollRect.verticalNormalizedPosition;

            // 刷新 content 高度
            contentHeight = Math.Max(
                totalBeats * DefaultBeatLineInterval * Model.BeatZoom + judgeLineRect.anchoredPosition.y,
                mainCanvaRect.rect.height
            );
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentHeight);

            // 恢复 content 位置
            scrollRect.verticalNormalizedPosition = verticalNormalizedPosition;
        }

        private async void RefreshBeatLines()
        {
            // 归还所有节拍线到池
            for (int i = beatLines.transform.childCount - 1; i >= 0; i--)
            {
                GameRoot.GameObjectPool.ReleaseGameObject(BeatLinePrefabPath,
                    beatLines.transform.GetChild(i).gameObject);
            }

            // 计算屏幕下沿对应的 content 位置
            float contentPos = (contentRect.rect.height - mainCanvaRect.rect.height) *
                               scrollRect.verticalNormalizedPosition;

            // 计算每条节拍线（包括细分节拍线）占用的位置
            float beatLineDistance = DefaultBeatLineInterval * Model.BeatZoom / Model.BeatAccuracy;

            // 计算第一条需要渲染的节拍线的计数
            int currentBeatLineCount =
                (int)((contentPos - judgeLineRect.position.y) / beatLineDistance); // 屏幕外会多渲染几条节拍线，符合预期
            currentBeatLineCount = Math.Max(1, currentBeatLineCount);

            while ((currentBeatLineCount - 1) * beatLineDistance < contentPos + mainCanvaRect.rect.height)
            {
                // 渲染节拍线
                GameObject go =
                    await GameRoot.GameObjectPool.GetGameObjectAsync(BeatLinePrefabPath, beatLines.transform);
                BeatLine beatLine = go.GetComponent<BeatLine>();
                RectTransform rect = beatLine.BeatLineRect;
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                float anchoredPositionY = judgeLineRect.anchoredPosition.y +
                                          beatLineDistance * (currentBeatLineCount - 1) -
                                          contentPos;
                rect.anchoredPosition = new Vector2(0, anchoredPositionY);
                rect.localScale = Vector3.one;

                int beatAccNum = (currentBeatLineCount - 1) % Model.BeatAccuracy;
                if (beatAccNum == 0)
                {
                    // 整数节拍线
                    beatLine.Image.color = Color.white;
                    beatLine.BeatTextObject.SetActive(true);
                    beatLine.BeatText.text = ((currentBeatLineCount - 1) / Model.BeatAccuracy + 1).ToString();
                }
                else if (Model.BeatAccuracy % 2 == 0 && beatAccNum == Model.BeatAccuracy / 2)
                {
                    // 1/2 节拍线
                    beatLine.Image.color = new Color(0.7f, 0.6f, 1f, 0.8f);
                    beatLine.BeatTextObject.SetActive(false);
                }
                else if (Model.BeatAccuracy % 4 == 0 &&
                         (beatAccNum == Model.BeatAccuracy / 4 || beatAccNum == Model.BeatAccuracy / 4 * 3))
                {
                    // 1/4 或 3/4 节拍线
                    beatLine.Image.color = new Color(0.5f, 0.8f, 1f, 0.8f);
                    beatLine.BeatTextObject.SetActive(false);
                }
                else
                {
                    // 其他节拍线
                    beatLine.Image.color = new Color(0.6f, 1f, 0.6f, 0.8f);
                    beatLine.BeatTextObject.SetActive(false);
                }

                currentBeatLineCount++;
            }
        }

        private async void RefreshNotes()
        {
            // 将所有 Note 归还到池
            for (int i = notes.transform.childCount - 1; i >= 0; i--)
            {
                GameObject go = notes.transform.GetChild(i).gameObject;
                switch (go.GetComponent<EditorNote>().NoteType)
                {
                    case NoteType.Tap:
                        GameRoot.GameObjectPool.ReleaseGameObject(TapNotePrefabPath, go);
                        break;
                    case NoteType.Hold:
                        GameRoot.GameObjectPool.ReleaseGameObject(HoldNotePrefabPath, go);
                        break;
                    case NoteType.Drag:
                        GameRoot.GameObjectPool.ReleaseGameObject(DragNotePrefabPath, go);
                        break;
                    case NoteType.Click:
                        GameRoot.GameObjectPool.ReleaseGameObject(ClickNotePrefabPath, go);
                        break;
                    case NoteType.Break:
                        GameRoot.GameObjectPool.ReleaseGameObject(BreakNotePrefabPath, go);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // 计算屏幕下沿对应的 content 位置
            float contentPos = (contentRect.rect.height - mainCanvaRect.rect.height) *
                               scrollRect.verticalNormalizedPosition;

            // 遍历并渲染在可视区域附近的 note
            foreach (BaseChartNoteData noteData in Model.ChartNotes)
            {
                float noteCalculatePos = CalculatePosInContent(noteData.JudgeBeat.ToFloat());
                float noteCalculateEndPos = noteData.Type == NoteType.Hold
                    ? CalculatePosInContent((noteData as HoldChartNoteData).EndJudgeBeat.ToFloat())
                    : noteCalculatePos;

                if ((noteCalculateEndPos < (contentPos - 40f)) ||
                    (noteCalculatePos > (contentPos + mainCanvaRect.rect.height + 40f)))
                {
                    continue;
                }

                // 发生重叠，需要渲染
                GameObject go;
                float xPos;
                RectTransform rect;
                EditorNote editorNote;

                switch (noteData.Type)
                {
                    // TODO: 优化代码
                    case NoteType.Tap:
                    case NoteType.Drag:
                    case NoteType.Click:
                        go = noteData.Type switch
                        {
                            NoteType.Tap => await GameRoot.GameObjectPool.GetGameObjectAsync(TapNotePrefabPath,
                                notes.transform),
                            NoteType.Drag => await GameRoot.GameObjectPool.GetGameObjectAsync(DragNotePrefabPath,
                                notes.transform),
                            NoteType.Click => await GameRoot.GameObjectPool.GetGameObjectAsync(ClickNotePrefabPath,
                                notes.transform),
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        editorNote = go.GetComponent<EditorNote>();
                        editorNote.Init(noteData);
                        rect = editorNote.Rect;
                        rect.localScale = Vector3.one;
                        rect.anchorMin = new Vector2(0.5f, 0f);
                        rect.anchorMax = new Vector2(0.5f, 0f);
                        xPos = (noteData as IChartNoteNormalPos).Pos * 802.5f - 321f;
                        rect.anchoredPosition = new Vector2(xPos, noteCalculatePos - contentPos);
                        break;
                    case NoteType.Hold:
                        go = await GameRoot.GameObjectPool.GetGameObjectAsync(HoldNotePrefabPath, notes.transform);
                        editorNote = go.GetComponent<EditorNote>();
                        editorNote.Init(noteData);
                        rect = editorNote.Rect;
                        rect.localScale = Vector3.one;
                        rect.anchorMin = new Vector2(0.5f, 0f);
                        rect.anchorMax = new Vector2(0.5f, 0f);
                        xPos = (noteData as HoldChartNoteData).Pos * 802.5f - 321f;
                        rect.anchoredPosition = new Vector2(xPos, noteCalculatePos - contentPos);
                        editorNote.HoldTailRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                            Mathf.Max(0, noteCalculateEndPos - noteCalculatePos - 12.5f));
                        break;
                    case NoteType.Break:
                        go = await GameRoot.GameObjectPool.GetGameObjectAsync(BreakNotePrefabPath, notes.transform);
                        editorNote = go.GetComponent<EditorNote>();
                        editorNote.Init(noteData);
                        rect = editorNote.Rect;
                        rect.localScale = Vector3.one;
                        rect.anchorMin = new Vector2(0.5f, 0f);
                        rect.anchorMax = new Vector2(0.5f, 0f);
                        xPos = (noteData as BreakChartNoteData).BreakNotePos == BreakNotePos.Left ? -468.8f : 468.8f;
                        rect.anchoredPosition = new Vector2(xPos, noteCalculatePos - contentPos);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// 根据 beat 计算内容在 Content 中的位置
        /// </summary>
        /// <param name="beatFloat">float 格式的 beat</param>
        /// <returns>内容在 Content 中的位置（相对于 Content 下边缘）</returns>
        private float CalculatePosInContent(float beatFloat)
        {
            // 计算每拍占用的间隔
            float beatDistance = DefaultBeatLineInterval * Model.BeatZoom;

            // 乘算节拍位置
            float pos = beatFloat * beatDistance;

            // 添加判定线的位置偏移
            return pos + judgeLineRect.anchoredPosition.y;
        }


        /// <summary>
        /// 重计算总拍数并刷新编辑器视图
        /// </summary>
        private void ResetTotalBeats()
        {
            totalBeats = CalculateTotalBeats(Model.ActualMusicTime, Model.ChartData.BpmGroup);
            RefreshUI();
        }

        /// <summary>
        /// 根据总时间和 bpm 组数据，计算并向上取整总共有几个拍子。
        /// <para>将计算已经经过的拍子时，不要忘了 -1</para>
        /// </summary>
        /// <param name="actualMusicTime">计算 offset 后的音乐总时长（ms）</param>
        /// <param name="bpmGroup">bpm 组数据</param>
        /// <returns>音乐总共有几拍（向上取整）</returns>
        private int CalculateTotalBeats(int actualMusicTime, BpmGroup bpmGroup)
        {
            bpmGroup.SortGroup();

            // 懒得写算法了，暴力枚举吧
            int beatCount = 0;
            while (bpmGroup.CalculateTime(beatCount) < actualMusicTime)
            {
                beatCount++;
            }

            return beatCount;
        }

        private void Awake()
        {
            scrollRect.onValueChanged.AddListener((_) => { RefreshUI(); });
        }

        private void Update()
        {
            // TODO: 改为由 GameRoot 下发事件
            if (!Mathf.Approximately(lastCanvaHeight, mainCanvaRect.rect.height))
            {
                RefreshUI();
                lastCanvaHeight = mainCanvaRect.rect.height;
            }
        }
    }
}
