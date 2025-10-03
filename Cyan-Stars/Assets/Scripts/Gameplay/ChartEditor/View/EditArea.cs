using System;
using System.Threading.Tasks;
using CyanStars.Chart;
using UnityEngine;
using CyanStars.Framework;
using CyanStars.GamePlay.ChartEditor.Model;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CyanStars.GamePlay.ChartEditor.View
{
    public class EditArea : BaseView, IPointerClickHandler
    {
        /// <summary>
        /// 一倍缩放时整数节拍线之间的间隔距离
        /// </summary>
        private const float DefaultBeatLineInterval = 250f;

        // 对象池使用的预制体
        private const string BeatLinePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BeatLine.prefab";
        private const string PosLinePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/PosLine.prefab";
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
        private GameObject judgeLine;

        [SerializeField]
        private ScrollRect scrollRect;


        private RectTransform contentRect;
        private RectTransform judgeLineRect;

        private int totalBeats; // 当前选中的音乐（含offset）的向上取整拍子总数量
        private float lastCanvaHeight; // 上次记录的 Canva 高度（刷新前）
        private float contentHeight; // 内容总高度

        private static readonly Color BeatHalfColor = new Color(1f, 0.7f, 0.4f, 0.8f);
        private static readonly Color BeatQuarterColor = new Color(0.4f, 0.7f, 1f, 0.7f);
        private static readonly Color BeatOtherColor = new Color(0.6f, 1f, 0.6f, 0.6f);

        private const float NotePosScale = 802.5f;
        private const float NotePosOffset = -321f;
        private const float BreakLeftX = -468.8f;
        private const float BreakRightX = 468.8f;
        private const float LeftBreakThreshold = -421f;
        private const float RightBreakThreshold = 421f;
        private const float CentralMin = -400f;
        private const float CentralMax = 400f;


        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);

            // 缓存一些物体
            contentRect = scrollRect.content.GetComponent<RectTransform>();
            judgeLineRect = judgeLine.GetComponent<RectTransform>();

            Model.OnNoteDataChanged += RefreshUI;
            Model.OnEditorAttributeChanged += RefreshUI;
            Model.OnNoteAttributeChanged += RefreshUI;

            ResetTotalBeats();
        }


        /// <summary>
        /// 画面变化后(而不是每帧)或在编辑器或音符属性修改后，重新绘制编辑器的节拍线、位置线、音符
        /// </summary>
        private void RefreshUI()
        {
            RefreshScrollRect();
            ReleaseContentGameObject();
            _ = RefreshPosLinesAsync();
            _ = RefreshBeatLinesAsync();
            _ = RefreshNotesAsync();
        }

        /// <summary>
        /// 刷新滚动窗口大小和位置
        /// </summary>
        private void RefreshScrollRect()
        {
            // 记录已滚动的位置百分比
            float verticalNormalizedPosition = scrollRect.verticalNormalizedPosition;

            // 刷新 content 高度
            contentHeight = Math.Max(
                totalBeats * DefaultBeatLineInterval * Model.BeatZoom + judgeLineRect.anchoredPosition.y,
                mainCanvaRect.rect.height
            );
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);

            // 恢复 content 位置
            scrollRect.verticalNormalizedPosition = verticalNormalizedPosition;
        }

        /// <summary>
        /// 将所有预制体归还到对象池
        /// </summary>
        private void ReleaseContentGameObject()
        {
            // 归还位置线
            for (int i = posLines.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = posLines.transform.GetChild(i);
                // 不归还占位物体
                if (child.TryGetComponent<PosLine>(out PosLine posLine))
                {
                    GameRoot.GameObjectPool.ReleaseGameObject(PosLinePrefabPath, posLine.gameObject);
                    continue;
                }
            }

            // 归还节拍线和音符
            for (int i = contentRect.childCount - 1; i >= 0; i--)
            {
                Transform child = contentRect.GetChild(i);

                if (child.TryGetComponent<BeatLine>(out BeatLine beatLine))
                {
                    GameRoot.GameObjectPool.ReleaseGameObject(BeatLinePrefabPath, beatLine.gameObject);
                    continue;
                }

                if (child.TryGetComponent<EditorNote>(out EditorNote editorNote))
                {
                    switch (editorNote.NoteType)
                    {
                        case NoteType.Tap:
                            GameRoot.GameObjectPool.ReleaseGameObject(TapNotePrefabPath, editorNote.gameObject);
                            break;
                        case NoteType.Hold:
                            GameRoot.GameObjectPool.ReleaseGameObject(HoldNotePrefabPath, editorNote.gameObject);
                            break;
                        case NoteType.Drag:
                            GameRoot.GameObjectPool.ReleaseGameObject(DragNotePrefabPath, editorNote.gameObject);
                            break;
                        case NoteType.Click:
                            GameRoot.GameObjectPool.ReleaseGameObject(ClickNotePrefabPath, editorNote.gameObject);
                            break;
                        case NoteType.Break:
                            GameRoot.GameObjectPool.ReleaseGameObject(BreakNotePrefabPath, editorNote.gameObject);
                            break;
                    }

                    continue;
                }
            }
        }

        /// <summary>
        /// 从对象池获取物体并刷新节拍线
        /// </summary>
        private async Task RefreshBeatLinesAsync()
        {
            // 计算屏幕下沿对应的 content 位置
            float contentPos = (contentRect.rect.height - mainCanvaRect.rect.height) *
                               scrollRect.verticalNormalizedPosition;

            // 计算每条节拍线（包括细分节拍线）占用的位置
            float beatLineDistance = DefaultBeatLineInterval * Model.BeatZoom / Model.BeatAccuracy;

            // 计算第一条需要渲染的节拍线的计数
            int currentBeatLineCount =
                (int)((contentPos - judgeLineRect.anchoredPosition.y) / beatLineDistance); // 屏幕外会多渲染几条节拍线，符合预期
            currentBeatLineCount = Math.Max(1, currentBeatLineCount);

            while ((currentBeatLineCount - 1) * beatLineDistance < contentPos + mainCanvaRect.rect.height &&
                   currentBeatLineCount / Model.BeatAccuracy <= totalBeats)
            {
                // 渲染节拍线
                GameObject go = await GameRoot.GameObjectPool.GetGameObjectAsync(BeatLinePrefabPath, contentRect);
                BeatLine beatLine = go.GetComponent<BeatLine>();
                RectTransform rect = beatLine.BeatLineRect;
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                float anchoredPositionY =
                    judgeLineRect.anchoredPosition.y + beatLineDistance * (currentBeatLineCount - 1);
                rect.anchoredPosition = new Vector2(0, anchoredPositionY);
                rect.localScale = Vector3.one;

                int beatAccNum = (currentBeatLineCount - 1) % Model.BeatAccuracy;
                if (beatAccNum == 0)
                {
                    // 整数节拍线
                    beatLine.Image.color = Color.white;
                    beatLine.BeatTextObject.SetActive(true);
                    beatLine.BeatText.text = ((currentBeatLineCount - 1) / Model.BeatAccuracy).ToString();
                }
                else if (Model.BeatAccuracy % 2 == 0 && beatAccNum == Model.BeatAccuracy / 2)
                {
                    // 1/2 节拍线
                    beatLine.Image.color = BeatHalfColor;
                    beatLine.BeatTextObject.SetActive(false);
                }
                else if (Model.BeatAccuracy % 4 == 0 &&
                         (beatAccNum == Model.BeatAccuracy / 4 || beatAccNum == Model.BeatAccuracy / 4 * 3))
                {
                    // 1/4 或 3/4 节拍线
                    beatLine.Image.color = BeatQuarterColor;
                    beatLine.BeatTextObject.SetActive(false);
                }
                else
                {
                    // 其他节拍线
                    beatLine.Image.color = BeatOtherColor;
                    beatLine.BeatTextObject.SetActive(false);
                }

                currentBeatLineCount++;
            }
        }

        /// <summary>
        /// 统一设定 Note Rect 值
        /// </summary>
        /// <param name="rect">Note 的 Rect 组件</param>
        private void ConfigureNoteRect(RectTransform rect)
        {
            rect.localScale = Vector3.one;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
        }

        /// <summary>
        /// 从对象池获取物体并刷新音符
        /// </summary>
        private async Task RefreshNotesAsync()
        {
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

                GameObject go;
                float xPos;
                RectTransform rect;
                EditorNote editorNote;

                switch (noteData.Type)
                {
                    case NoteType.Tap:
                    case NoteType.Drag:
                    case NoteType.Click:
                        string prefabPath = noteData.Type switch
                        {
                            NoteType.Tap => TapNotePrefabPath,
                            NoteType.Drag => DragNotePrefabPath,
                            NoteType.Click => ClickNotePrefabPath,
                            _ => throw new ArgumentOutOfRangeException()
                        };

                        go = await GameRoot.GameObjectPool.GetGameObjectAsync(prefabPath, contentRect);
                        editorNote = go.GetComponent<EditorNote>();
                        editorNote.SetData(Model, noteData);
                        rect = editorNote.Rect;
                        ConfigureNoteRect(rect);
                        xPos = (noteData as IChartNoteNormalPos).Pos * NotePosScale + NotePosOffset;
                        rect.anchoredPosition = new Vector2(xPos, noteCalculatePos);
                        break;
                    case NoteType.Hold:
                        go = await GameRoot.GameObjectPool.GetGameObjectAsync(HoldNotePrefabPath, contentRect);
                        editorNote = go.GetComponent<EditorNote>();
                        editorNote.SetData(Model, noteData);
                        rect = editorNote.Rect;
                        ConfigureNoteRect(rect);
                        xPos = (noteData as HoldChartNoteData).Pos * NotePosScale + NotePosOffset;
                        rect.anchoredPosition = new Vector2(xPos, noteCalculatePos);
                        editorNote.HoldTailRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                            Mathf.Max(0, noteCalculateEndPos - noteCalculatePos - 12.5f));
                        break;
                    case NoteType.Break:
                        go = await GameRoot.GameObjectPool.GetGameObjectAsync(BreakNotePrefabPath, contentRect);
                        editorNote = go.GetComponent<EditorNote>();
                        editorNote.SetData(Model, noteData);
                        rect = editorNote.Rect;
                        ConfigureNoteRect(rect);
                        xPos = (noteData as BreakChartNoteData).BreakNotePos == BreakNotePos.Left
                            ? BreakLeftX
                            : BreakRightX;
                        rect.anchoredPosition = new Vector2(xPos, noteCalculatePos);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// 从对象池获取物体并刷新位置线
        /// </summary>
        private async Task RefreshPosLinesAsync()
        {
            for (int i = 0; i < Model.PosAccuracy; i++)
            {
                GameObject _ = await GameRoot.GameObjectPool.GetGameObjectAsync(PosLinePrefabPath, posLines.transform);
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

            // 使用指数扩展 + 二分查找替代以提升性能
            if (bpmGroup.CalculateTime(0) >= actualMusicTime)
            {
                return 0;
            }

            int low = 0;
            int high = 1;
            while (bpmGroup.CalculateTime(high) < actualMusicTime)
            {
                low = high;
                high <<= 1;
            }

            while (low + 1 < high)
            {
                int mid = low + ((high - low) >> 1);
                if (bpmGroup.CalculateTime(mid) < actualMusicTime)
                {
                    low = mid;
                }
                else
                {
                    high = mid;
                }
            }

            return high;
        }


        /// <summary>
        /// 当空白部分（除 note 按钮组件）被点击时自动触发，用于向 Model 请求在点击位置创建一个音符
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // TODO: 优化计算

            // 将屏幕坐标转为局部坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                scrollRect.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPosition);


            // 计算横坐标，左侧 Break 轨道视为 -1f，右侧 Break 轨道视为 2f，中央轨道取值为 [0f,0.8f]，点击轨道间隙时不触发 Model 方法
            // 开启横坐标吸附时，将音符中心位置解析到最近的（位置线/位置线间隔中线）上，随后返回音符左端点位置
            float notePos;
            if (localPosition.x <= LeftBreakThreshold)
            {
                // 左侧 Break 轨道
                notePos = -1f;
            }
            else if (RightBreakThreshold <= localPosition.x)
            {
                // 右侧 Break 轨道
                notePos = 2f;
            }
            else if (CentralMin <= localPosition.x && localPosition.x <= CentralMax)
            {
                // 中央轨道
                if (Model.PosMagnetState)
                {
                    // 开启了位置吸附
                    if (Model.PosAccuracy == 0)
                    {
                        notePos = 0.4f;
                    }
                    else
                    {
                        float subSectionWidth = (800f / (Model.PosAccuracy + 1)) / 2f;
                        float relativePosX = localPosition.x + 400f;
                        float snappingIndex = Mathf.Round(relativePosX / subSectionWidth);
                        float maxIndex = 2 * Model.PosAccuracy + 1;
                        snappingIndex = Mathf.Max(1f, Mathf.Min(maxIndex, snappingIndex));
                        float snappedRelativePos = snappingIndex * subSectionWidth;
                        float posX = snappedRelativePos - 400f;
                        notePos = (posX + 320f) / 800f;
                        notePos = Mathf.Min(notePos, 0.8f);
                        notePos = Mathf.Max(notePos, 0f);
                    }
                }
                else
                {
                    // 未开启位置吸附
                    notePos = (localPosition.x + 320f) / 800f;
                    notePos = Mathf.Min(notePos, 0.8f);
                    notePos = Mathf.Max(notePos, 0f);
                }
            }
            else
            {
                // 点到轨道间隙了，不处理
                return;
            }


            // 计算纵坐标，将鼠标位置解析到最近的节拍线（含细分）上
            float contentPos = (contentRect.rect.height - mainCanvaRect.rect.height) *
                               scrollRect.verticalNormalizedPosition;
            float clickOnContentPos = contentPos + localPosition.y;
            int subBeatLineIndex = Mathf.RoundToInt((clickOnContentPos - judgeLineRect.anchoredPosition.y) /
                                                    (DefaultBeatLineInterval * Model.BeatZoom / Model.BeatAccuracy));
            subBeatLineIndex = Math.Max(0, subBeatLineIndex);
            subBeatLineIndex = Math.Min(subBeatLineIndex, totalBeats * Model.BeatAccuracy);

            int z = Model.BeatAccuracy;
            int x = subBeatLineIndex / Model.BeatAccuracy;
            int y = subBeatLineIndex % Model.BeatAccuracy;
            Beat noteBeat = new Beat(x, y, z);

            Model.CreateNote(notePos, noteBeat);
        }

        /// <summary>
        /// 当窗口滚动时刷新界面
        /// </summary>
        private void OnScrollValueChanged(Vector2 _)
        {
            RefreshUI();
        }


        private void Awake()
        {
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
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

        private void OnDestroy()
        {
            Model.OnNoteDataChanged -= RefreshUI;
            Model.OnEditorAttributeChanged -= RefreshUI;
            Model.OnNoteAttributeChanged -= RefreshUI;
        }
    }
}
