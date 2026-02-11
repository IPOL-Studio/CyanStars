// #nullable enable
//
// using System;
// using System.Collections;
// using System.Threading.Tasks;
// using CyanStars.Chart;
// using UnityEngine;
// using CyanStars.Framework;
// using CyanStars.GamePlay.ChartEditor.Model;
// using TMPro;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
//
// namespace CyanStars.GamePlay.ChartEditor.View
// {
//     public class EditArea : BaseView, IPointerClickHandler
//     {
//         /// <summary>
//         /// 一倍缩放时整数节拍线之间的间隔距离
//         /// </summary>
//         private const float DefaultBeatLineInterval = 250f;
//
//         // 对象池使用的预制体
//         private const string BeatLinePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BeatLine.prefab";
//         private const string EndLinePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/EndLine.prefab";
//         private const string PosLinePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/PosLine.prefab";
//         private const string TapNotePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/TapNote.prefab";
//         private const string DragNotePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/DragNote.prefab";
//         private const string HoldNotePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/HoldNote.prefab";
//         private const string ClickNotePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/ClickNote.prefab";
//         private const string BreakNotePrefabPath = "Assets/BundleRes/Prefabs/ChartEditor/EditArea/BreakNote.prefab";
//
//         [SerializeField]
//         private AudioSource audioSource = null!;
//
//         [SerializeField]
//         private RectTransform mainCanvaRect = null!;
//
//         [SerializeField]
//         private GameObject tracks = null!;
//
//         [SerializeField]
//         private GameObject posLines = null!;
//
//         [SerializeField]
//         private GameObject judgeLine = null!;
//
//         [SerializeField]
//         private ScrollRect scrollRect = null!;
//
//
//         private RectTransform contentRect = null!;
//         private RectTransform judgeLineRect = null!;
//
//
//         private static readonly Color BeatHalfColor = new Color(1f, 0.7f, 0.4f, 0.8f);
//         private static readonly Color BeatQuarterColor = new Color(0.4f, 0.7f, 1f, 0.7f);
//         private static readonly Color BeatOtherColor = new Color(0.6f, 1f, 0.6f, 0.6f);
//
//         private const float NotePosScale = 802.5f;
//         private const float NotePosOffset = -321f;
//         private const float BreakLeftX = -468.8f;
//         private const float BreakRightX = 468.8f;
//         private const float LeftBreakThreshold = -421f;
//         private const float RightBreakThreshold = 421f;
//         private const float CentralMin = -400f;
//         private const float CentralMax = 400f;
//
//
//         private float totalBeat; // 谱包总 beat 数量（小数）
//         private float lastCanvaHeight; // 上次记录的 Canva 高度（刷新前）
//         private float contentHeight; // 内容总高度
//
//         /// <summary>
//         /// 当前是否正在播放音乐
//         /// </summary>
//         private bool isChartEditorAudioPlaying = false;
//
//         private Coroutine? playCoroutine = null;
//
//
//         public override void Bind(ChartEditorModel chartEditorModel)
//         {
//             base.Bind(chartEditorModel);
//
//             // 缓存一些物体
//             contentRect = scrollRect.content.GetComponent<RectTransform>();
//             judgeLineRect = judgeLine.GetComponent<RectTransform>();
//
//             Model.OnLoadedAudioClipChanged += RefreshContentUI;
//             Model.OnBpmGroupChanged += RefreshContentUI;
//             Model.OnNoteDataChanged += RefreshBeatLinesAndNotes;
//             Model.OnEditorAttributeChanged += RefreshPosLine;
//             Model.OnEditorAttributeChanged += RefreshBeatLinesAndNotes;
//             Model.OnNoteAttributeChanged += RefreshBeatLinesAndNotes;
//
//             RefreshPosLine();
//             RefreshContentUI();
//         }
//
//
//         /// <summary>
//         /// 切换音乐并重计算 content 高度
//         /// </summary>
//         private void RefreshContentUI()
//         {
//             StopAudio();
//             SetNewAudioClip();
//             CalculateTotalBeatsAndRefreshUI();
//             return;
//
//
//             // 当 Model 的音乐变化时，更新 audioClip
//             void SetNewAudioClip()
//             {
//                 if (!Model.LoadedAudioClip)
//                 {
//                     Debug.LogWarning("未选中 AudioClip");
//                 }
//
//                 audioSource.clip = Model.LoadedAudioClip;
//             }
//
//             // 重计算总拍数并刷新 UI
//             void CalculateTotalBeatsAndRefreshUI()
//             {
//                 totalBeat = (Model.TotalMusicTime == null || Model.BpmGroupDatas.Count == 0)
//                     ? 0
//                     : Model.ChartPackData.BpmGroup.CalculateBeat((int)Model.TotalMusicTime);
//
//                 RefreshBeatLinesAndNotes();
//             }
//         }
//
//         /// <summary>
//         /// 画面变化后(而不是每帧)或在制谱器或音符属性修改后，重新绘制制谱器的节拍线、位置线、音符
//         /// </summary>
//         private void RefreshBeatLinesAndNotes()
//         {
//             RefreshScrollRect();
//             // TODO: 优化渲染逻辑和性能：先计算并移动物体位置，再对剩下的物体执行取回/放入对象池操作
//             ReleaseBetLinesAndNotes();
//             // _ = RefreshPosLinesAsync();
//             _ = RefreshBeatLinesAsync();
//             _ = RefreshNotesAsync();
//             return;
//
//             // 刷新滚动窗口大小和位置
//             void RefreshScrollRect()
//             {
//                 // 记录已滚动的位置百分比
//                 float verticalNormalizedPosition = scrollRect.verticalNormalizedPosition;
//
//                 // 刷新 content 高度
//                 contentHeight = totalBeat * DefaultBeatLineInterval * Model.BeatZoom + mainCanvaRect.rect.height;
//                 contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
//
//                 // 恢复 content 位置
//                 scrollRect.verticalNormalizedPosition = verticalNormalizedPosition;
//             }
//
//             // 将节拍线和音符归还到池
//             void ReleaseBetLinesAndNotes()
//             {
//                 // 归还节拍线和音符
//                 for (int i = contentRect.childCount - 1; i >= 0; i--)
//                 {
//                     Transform child = contentRect.GetChild(i);
//
//                     if (child.TryGetComponent<BeatLine>(out BeatLine beatLine))
//                     {
//                         GameRoot.GameObjectPool.ReleaseGameObject(BeatLinePrefabPath, beatLine.gameObject);
//                         continue;
//                     }
//
//                     if (child.TryGetComponent<EndLine>(out EndLine endLine))
//                     {
//                         GameRoot.GameObjectPool.ReleaseGameObject(EndLinePrefabPath, endLine.gameObject);
//                         continue;
//                     }
//
//                     if (child.TryGetComponent<EditorNote>(out EditorNote editorNote))
//                     {
//                         switch (editorNote.NoteType)
//                         {
//                             case NoteType.Tap:
//                                 GameRoot.GameObjectPool.ReleaseGameObject(TapNotePrefabPath, editorNote.gameObject);
//                                 break;
//                             case NoteType.Hold:
//                                 GameRoot.GameObjectPool.ReleaseGameObject(HoldNotePrefabPath, editorNote.gameObject);
//                                 break;
//                             case NoteType.Drag:
//                                 GameRoot.GameObjectPool.ReleaseGameObject(DragNotePrefabPath, editorNote.gameObject);
//                                 break;
//                             case NoteType.Click:
//                                 GameRoot.GameObjectPool.ReleaseGameObject(ClickNotePrefabPath, editorNote.gameObject);
//                                 break;
//                             case NoteType.Break:
//                                 GameRoot.GameObjectPool.ReleaseGameObject(BreakNotePrefabPath, editorNote.gameObject);
//                                 break;
//                         }
//                     }
//                 }
//             }
//
//             // 从对象池获取物体并刷新节拍线
//             async Task RefreshBeatLinesAsync()
//             {
//                 if (Model.BpmGroupDatas.Count == 0 || Model.LoadedAudioClip == null)
//                 {
//                     return;
//                 }
//
//                 // 计算屏幕下沿对应的 content 位置
//                 float contentPos = (contentRect.rect.height - mainCanvaRect.rect.height) *
//                                    scrollRect.verticalNormalizedPosition;
//
//                 // 计算每条子节拍线（包括细分节拍线）占用的位置
//                 float beatLineDistance = DefaultBeatLineInterval * Model.BeatZoom / Model.BeatAccuracy;
//
//                 // 计算第一条需要渲染的子节拍线的计数（index + 1）
//                 int currentSubBeatLineCount =
//                     (int)((contentPos - judgeLineRect.anchoredPosition.y) / beatLineDistance); // 屏幕外会多渲染几条节拍线，符合预期
//                 currentSubBeatLineCount = Math.Max(1, currentSubBeatLineCount);
//
//                 // 渲染所有的子节拍线
//                 while ((currentSubBeatLineCount - 1) * beatLineDistance <
//                        contentPos + mainCanvaRect.rect.height && // 到达屏幕上边界后不再渲染
//                        (float)(currentSubBeatLineCount - 1) / Model.BeatAccuracy < totalBeat) // 到达音乐结束点后也不再渲染
//                 {
//                     GameObject go = await GameRoot.GameObjectPool.GetGameObjectAsync(BeatLinePrefabPath, contentRect);
//                     BeatLine beatLine = go.GetComponent<BeatLine>();
//                     RectTransform rect = beatLine.BeatLineRect;
//                     rect.anchorMin = new Vector2(0.5f, 0f);
//                     rect.anchorMax = new Vector2(0.5f, 0f);
//                     rect.localScale = Vector3.one;
//
//                     float anchoredPositionY =
//                         judgeLineRect.anchoredPosition.y + beatLineDistance * (currentSubBeatLineCount - 1);
//                     rect.anchoredPosition = new Vector2(0, anchoredPositionY);
//
//                     int beatAccNum = (currentSubBeatLineCount - 1) % Model.BeatAccuracy;
//                     if (beatAccNum == 0)
//                     {
//                         // 整数节拍线
//                         beatLine.Image.color = Color.white;
//                         beatLine.BeatTextObject.SetActive(true);
//                         beatLine.BeatText.text = ((currentSubBeatLineCount - 1) / Model.BeatAccuracy).ToString();
//                     }
//                     else if (Model.BeatAccuracy % 2 == 0 && beatAccNum == Model.BeatAccuracy / 2)
//                     {
//                         // 1/2 节拍线
//                         beatLine.Image.color = BeatHalfColor;
//                         beatLine.BeatTextObject.SetActive(false);
//                     }
//                     else if (Model.BeatAccuracy % 4 == 0 &&
//                              (beatAccNum == Model.BeatAccuracy / 4 || beatAccNum == Model.BeatAccuracy / 4 * 3))
//                     {
//                         // 1/4 或 3/4 节拍线
//                         beatLine.Image.color = BeatQuarterColor;
//                         beatLine.BeatTextObject.SetActive(false);
//                     }
//                     else
//                     {
//                         // 其他节拍线
//                         beatLine.Image.color = BeatOtherColor;
//                         beatLine.BeatTextObject.SetActive(false);
//                     }
//
//                     currentSubBeatLineCount++;
//                 }
//
//                 // 尝试渲染终止线
//                 float endLinePosY = totalBeat * DefaultBeatLineInterval * Model.BeatZoom;
//                 if (contentPos - 100f <= endLinePosY && contentPos <= contentPos + mainCanvaRect.rect.height + 100f)
//                 {
//                     GameObject go = await GameRoot.GameObjectPool.GetGameObjectAsync(EndLinePrefabPath, contentRect);
//                     EndLine endLine = go.GetComponent<EndLine>();
//                     RectTransform rect = endLine.BeatLineRect;
//                     rect.anchorMin = new Vector2(0.5f, 0f);
//                     rect.anchorMax = new Vector2(0.5f, 0f);
//                     rect.localScale = Vector3.one;
//
//                     float anchoredPositionY = judgeLineRect.anchoredPosition.y + endLinePosY;
//                     endLine.Image.color = Color.white;
//                     rect.anchoredPosition = new Vector2(0, anchoredPositionY);
//                 }
//             }
//
//             // 统一设定 Note Rect 值
//             void ConfigureNoteRect(RectTransform rect)
//             {
//                 rect.localScale = Vector3.one;
//                 rect.anchorMin = new Vector2(0.5f, 0f);
//                 rect.anchorMax = new Vector2(0.5f, 0f);
//             }
//
//             // 从对象池获取物体并刷新音符
//             async Task RefreshNotesAsync()
//             {
//                 // 计算屏幕下沿对应的 content 位置
//                 float contentPos = (contentRect.rect.height - mainCanvaRect.rect.height) *
//                                    scrollRect.verticalNormalizedPosition;
//
//                 // 遍历并渲染在可视区域附近的 note
//                 foreach (BaseChartNoteData noteData in Model.ChartNotes)
//                 {
//                     float noteCalculatePos = CalculatePosInContent(noteData.JudgeBeat.ToFloat());
//                     float noteCalculateEndPos = noteData.Type == NoteType.Hold
//                         ? CalculatePosInContent((noteData as HoldChartNoteData).EndJudgeBeat.ToFloat())
//                         : noteCalculatePos;
//
//                     if ((noteCalculateEndPos < (contentPos - 40f)) ||
//                         (noteCalculatePos > (contentPos + mainCanvaRect.rect.height + 40f)))
//                     {
//                         continue;
//                     }
//
//                     GameObject go;
//                     float xPos;
//                     RectTransform rect;
//                     EditorNote editorNote;
//
//                     switch (noteData.Type)
//                     {
//                         case NoteType.Tap:
//                         case NoteType.Drag:
//                         case NoteType.Click:
//                             string prefabPath = noteData.Type switch
//                             {
//                                 NoteType.Tap => TapNotePrefabPath,
//                                 NoteType.Drag => DragNotePrefabPath,
//                                 NoteType.Click => ClickNotePrefabPath,
//                                 _ => throw new ArgumentOutOfRangeException()
//                             };
//
//                             go = await GameRoot.GameObjectPool.GetGameObjectAsync(prefabPath, contentRect);
//                             editorNote = go.GetComponent<EditorNote>();
//                             editorNote.SetData(Model, noteData);
//                             rect = editorNote.Rect;
//                             ConfigureNoteRect(rect);
//                             xPos = (noteData as IChartNoteNormalPos).Pos * NotePosScale + NotePosOffset;
//                             rect.anchoredPosition = new Vector2(xPos, noteCalculatePos);
//                             break;
//                         case NoteType.Hold:
//                             go = await GameRoot.GameObjectPool.GetGameObjectAsync(HoldNotePrefabPath, contentRect);
//                             editorNote = go.GetComponent<EditorNote>();
//                             editorNote.SetData(Model, noteData);
//                             rect = editorNote.Rect;
//                             ConfigureNoteRect(rect);
//                             xPos = (noteData as HoldChartNoteData).Pos * NotePosScale + NotePosOffset;
//                             rect.anchoredPosition = new Vector2(xPos, noteCalculatePos);
//                             editorNote.HoldTailRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
//                                 Mathf.Max(0, noteCalculateEndPos - noteCalculatePos - 12.5f));
//                             break;
//                         case NoteType.Break:
//                             go = await GameRoot.GameObjectPool.GetGameObjectAsync(BreakNotePrefabPath, contentRect);
//                             editorNote = go.GetComponent<EditorNote>();
//                             editorNote.SetData(Model, noteData);
//                             rect = editorNote.Rect;
//                             ConfigureNoteRect(rect);
//                             xPos = (noteData as BreakChartNoteData).BreakNotePos == BreakNotePos.Left
//                                 ? BreakLeftX
//                                 : BreakRightX;
//                             rect.anchoredPosition = new Vector2(xPos, noteCalculatePos);
//                             break;
//                         default:
//                             throw new ArgumentOutOfRangeException();
//                     }
//                 }
//             }
//
//
//             // 根据 beat 计算内容在 Content 中的位置
//             float CalculatePosInContent(float beatFloat)
//             {
//                 // 计算每拍占用的间隔
//                 float beatDistance = DefaultBeatLineInterval * Model.BeatZoom;
//
//                 // 乘算节拍位置
//                 float pos = beatFloat * beatDistance;
//
//                 // 添加判定线的位置偏移
//                 return pos + judgeLineRect.anchoredPosition.y;
//             }
//         }
//
//         /// <summary>
//         /// 从对象池获取物体并刷新位置线
//         /// </summary>
//         async void RefreshPosLine()
//         {
//             for (int i = posLines.transform.childCount - 1; i >= 0; i--)
//             {
//                 Transform child = posLines.transform.GetChild(i);
//                 // 不归还最左侧的一个透明占位物体，这样子可以让 Unity 自动布局到正确的位置
//                 if (child.TryGetComponent<PosLine>(out PosLine posLine))
//                 {
//                     GameRoot.GameObjectPool.ReleaseGameObject(PosLinePrefabPath, posLine.gameObject);
//                 }
//             }
//
//             for (int i = 0; i < Model.PosAccuracy; i++)
//             {
//                 await GameRoot.GameObjectPool.GetGameObjectAsync(PosLinePrefabPath, posLines.transform);
//             }
//         }
//
//         /// <summary>
//         /// 切换音频播放状态
//         /// </summary>
//         private void SwitchAudioPlayingState()
//         {
//             if (!audioSource.clip)
//             {
//                 Debug.LogError("未指定 Clip 或 MusicVersionData，EditArea 无法播放音频");
//                 isChartEditorAudioPlaying = false;
//                 StopAudio();
//                 return;
//             }
//
//             isChartEditorAudioPlaying = !isChartEditorAudioPlaying;
//
//             if (isChartEditorAudioPlaying)
//             {
//                 PlayAudio();
//             }
//             else
//             {
//                 StopAudio();
//             }
//         }
//
//         /// <summary>
//         /// 根据当前的 content 进度和 offset 自动确定播放时机
//         /// </summary>
//         private void PlayAudio()
//         {
//             if (!audioSource.clip || Model.MusicVersionDatas.Count == 0)
//             {
//                 Debug.LogError("未指定 Clip 或 MusicVersionData，EditArea 无法播放音频");
//                 isChartEditorAudioPlaying = false;
//                 StopAudio();
//                 return;
//             }
//
//             // 整个 content 对应的音乐时长（ms） = clip 时长 + offset
//             int totalTime = (int)(audioSource.clip.length * 1000) + Model.MusicVersionDatas[0].Offset;
//             int currentTime = (int)(totalTime * scrollRect.verticalNormalizedPosition);
//             int audioTime = currentTime - Model.MusicVersionDatas[0].Offset;
//
//             if (audioTime >= 0)
//             {
//                 // 直接从指定时间开始播放
//                 audioSource.time = audioTime / 1000f;
//                 audioSource.Play();
//             }
//             else
//             {
//                 // 延迟一段时间后从 0 开始播放
//                 playCoroutine = StartCoroutine(PlayAudioAfterDelay(-audioTime / 1000f));
//             }
//
//             return;
//
//             // 迭代器协程，用于在指定时间后从头开始播放 audio（见于 offset 为正数且当前正在播放这段内容时）
//             IEnumerator PlayAudioAfterDelay(float delay)
//             {
//                 yield return new WaitForSeconds(delay);
//                 audioSource.time = 0f;
//                 audioSource.Play();
//                 playCoroutine = null;
//             }
//         }
//
//
//         /// <summary>
//         /// 立刻停止播放，或取消即将播放的协程
//         /// </summary>
//         private void StopAudio()
//         {
//             if (audioSource.isPlaying)
//             {
//                 audioSource.Stop();
//             }
//
//             if (playCoroutine != null)
//             {
//                 StopCoroutine(playCoroutine);
//                 playCoroutine = null;
//             }
//         }
//
//
//         /// <summary>
//         /// 当空白部分（除 note 按钮组件）被点击时自动触发，用于向 Model 请求在点击位置创建一个音符
//         /// </summary>
//         public void OnPointerClick(PointerEventData eventData)
//         {
//             // TODO: 优化计算
//
//             if (Model.TotalMusicTime == null || Model.TotalMusicTime == 0 || Model.BpmGroupDatas.Count == 0)
//             {
//                 // 没有有效时间或 BPM 组时无法创建音符
//                 return;
//             }
//
//             // 将屏幕坐标转为局部坐标
//             RectTransformUtility.ScreenPointToLocalPointInRectangle(
//                 scrollRect.GetComponent<RectTransform>(),
//                 eventData.position,
//                 eventData.pressEventCamera,
//                 out Vector2 localPosition);
//
//
//             // 计算横坐标，左侧 Break 轨道视为 -1f，右侧 Break 轨道视为 2f，中央轨道取值为 [0f,0.8f]，点击轨道间隙时不触发 Model 方法
//             // 开启横坐标吸附时，将音符中心位置解析到最近的（位置线/位置线间隔中线）上，随后返回音符左端点位置
//             float notePos;
//             if (localPosition.x <= LeftBreakThreshold)
//             {
//                 // 左侧 Break 轨道
//                 notePos = -1f;
//             }
//             else if (RightBreakThreshold <= localPosition.x)
//             {
//                 // 右侧 Break 轨道
//                 notePos = 2f;
//             }
//             else if (CentralMin <= localPosition.x && localPosition.x <= CentralMax)
//             {
//                 // 中央轨道
//                 if (Model.PosMagnetState)
//                 {
//                     // 开启了位置吸附
//                     if (Model.PosAccuracy == 0)
//                     {
//                         notePos = 0.4f;
//                     }
//                     else
//                     {
//                         float subSectionWidth = (800f / (Model.PosAccuracy + 1)) / 2f;
//                         float relativePosX = localPosition.x + 400f;
//                         float snappingIndex = Mathf.Round(relativePosX / subSectionWidth);
//                         float maxIndex = 2 * Model.PosAccuracy + 1;
//                         snappingIndex = Mathf.Max(1f, Mathf.Min(maxIndex, snappingIndex));
//                         float snappedRelativePos = snappingIndex * subSectionWidth;
//                         float posX = snappedRelativePos - 400f;
//                         notePos = (posX + 320f) / 800f;
//                         notePos = Mathf.Min(notePos, 0.8f);
//                         notePos = Mathf.Max(notePos, 0f);
//                     }
//                 }
//                 else
//                 {
//                     // 未开启位置吸附
//                     notePos = (localPosition.x + 320f) / 800f;
//                     notePos = Mathf.Min(notePos, 0.8f);
//                     notePos = Mathf.Max(notePos, 0f);
//                 }
//             }
//             else
//             {
//                 // 点到轨道间隙了，不处理
//                 return;
//             }
//
//
//             // 计算纵坐标，将鼠标位置解析到最近的节拍线（含细分）上，终止线上不能写音符
//             // TODO: 可能的bug：在终止线前几ms写音符，导致判定区间被裁剪，音游结束于miss判定前，之后再修
//             float contentPos = (contentRect.rect.height - mainCanvaRect.rect.height) *
//                                scrollRect.verticalNormalizedPosition;
//             float clickOnContentPos = contentPos + localPosition.y;
//             int subBeatLineIndex = Mathf.RoundToInt((clickOnContentPos - judgeLineRect.anchoredPosition.y) /
//                                                     (DefaultBeatLineInterval * Model.BeatZoom / Model.BeatAccuracy));
//             subBeatLineIndex = Math.Max(0, subBeatLineIndex);
//             int lastValidBeat = (totalBeat % 1f == 0) ? (int)(totalBeat - 1) : (int)totalBeat;
//             subBeatLineIndex = Math.Min(subBeatLineIndex, lastValidBeat * Model.BeatAccuracy);
//
//             int z = Model.BeatAccuracy;
//             int x = subBeatLineIndex / Model.BeatAccuracy;
//             int y = subBeatLineIndex % Model.BeatAccuracy;
//             if (!Beat.TryCreateBeat(x, y, z, out Beat noteBeat))
//             {
//                 throw new Exception("Couldn't create beat");
//             }
//
//             Model.CreateNote(notePos, noteBeat);
//         }
//
//
//         private void Awake()
//         {
//             // 当窗口滚动时刷新界面
//             scrollRect.onValueChanged.AddListener((_) =>
//             {
//                 RefreshBeatLinesAndNotes();
//             });
//         }
//
//         private void Update()
//         {
//             if (Model == null)
//             {
//                 return;
//             }
//
//             // TODO: 改为由 GameRoot 下发事件
//             if (!Mathf.Approximately(lastCanvaHeight, mainCanvaRect.rect.height))
//             {
//                 // 游戏窗口大小变化时刷新 UI
//                 RefreshBeatLinesAndNotes();
//                 lastCanvaHeight = mainCanvaRect.rect.height;
//             }
//
//             // 按下空格且没有打开任何悬浮窗或正在输入文本时，播放或暂停音乐
//             if (Input.GetKeyDown(KeyCode.Space))
//             {
//                 if (Model.IsAnyFloatingCanvasOn)
//                 {
//                     return;
//                 }
//
//                 if (EventSystem.current.currentSelectedGameObject != null &&
//                     (EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null ||
//                      EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null))
//                 {
//                     return;
//                 }
//
//                 SwitchAudioPlayingState();
//             }
//
//             if (isChartEditorAudioPlaying)
//             {
//                 scrollRect.verticalNormalizedPosition = audioSource.time / audioSource.clip.length; //TODO: 计算 offset
//                 RefreshBeatLinesAndNotes();
//             }
//         }
//
//         private void OnDestroy()
//         {
//             Model.OnLoadedAudioClipChanged -= RefreshContentUI;
//             Model.OnNoteDataChanged -= RefreshBeatLinesAndNotes;
//             Model.OnEditorAttributeChanged -= RefreshBeatLinesAndNotes;
//             Model.OnNoteAttributeChanged -= RefreshBeatLinesAndNotes;
//         }
//     }
// }



