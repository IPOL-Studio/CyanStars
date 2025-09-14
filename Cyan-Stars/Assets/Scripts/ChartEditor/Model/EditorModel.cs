using System;
using System.Collections.Generic;
using CyanStars.Chart;
using UnityEngine;

namespace CyanStars.ChartEditor.Model
{
    /// <summary>
    /// 谱面编辑器 Model 层
    /// </summary>
    public class EditorModel
    {
        // --- 在编辑器内初始化和临时存储的、经过校验后的数据，不会持久化 ---

        public EditTool EditTool { get; private set; }
        public int PosAccuracy { get; private set; }
        public bool PosMagnetState { get; private set; }
        public int BeatAccuracy { get; private set; }
        public float BeatZoom { get; private set; }

        /// <summary>
        /// 创建 Hold 音符时暂存的开始拍
        /// </summary>
        public Beat? TempHoldJudgeBeat;

        public int NoteIdCounter { get; private set; }

        /// <summary>
        /// 用于 M 层的音符数据，在 Model 构造时初始化，提供高效的按 ID 查询音符数据能力
        /// </summary>
        /// <remarks>int 为音符 ID，不持久化，在每次加载谱面时按序分配，不保证连续。</remarks>
        public HashSet<BaseChartNoteData> ChartNotes { get; private set; }

        // 当前选中的 Note，用 HashSet 是考虑兼容后续框选多个 Note 一起修改
        public HashSet<BaseChartNoteData> SelectedNotes { get; private set; }

        /// <summary>
        /// 计算 offset 后，当前选中的音乐的实际时长（ms）
        /// </summary>
        public int ActualMusicTime { get; set; } // TODO：测试完成后改为 private set;


        // --- 从磁盘加载到内存中的、经过校验后的谱包和谱面数据，加载/保存时需要从读写磁盘。 ---
        public ChartPackData ChartPackData { get; private set; }

        public int ChartIndex { get; private set; }

        public ChartData ChartData { get; private set; }


        public List<MusicVersionData> MusicVersionDatas => ChartPackData.MusicVersionDatas;

        public List<BpmGroupItem> BpmGroupDatas => ChartData.BpmGroup.Data;

        public List<SpeedGroupData> SpeedGroupDatas => ChartData.SpeedGroupDatas;


        // --- Model 事件 ---

        /// <summary>
        /// 选中的画笔发生变化
        /// </summary>
        public event Action OnEditToolChanged;

        /// <summary>
        /// 菜单栏任意按钮被点击
        /// </summary>
        public event Action OnMenuButtonClicked;

        /// <summary>
        /// 编辑器属性侧边栏内容变化
        /// </summary>
        public event Action OnEditorAttributeChanged;

        /// <summary>
        /// 音符属性侧边栏内容变化
        /// </summary>
        public event Action OnNoteAttributeChanged;

        /// <summary>
        /// 当选中的音符发生了变化（被选中、被取消选中）
        /// </summary>
        public event Action OnSelectedNotesChanged;

        /// <summary>
        /// 谱包基本信息（目前只有标题）发生变化
        /// </summary>
        public event Action OnChartPackTitleChanged;

        /// <summary>
        /// 谱包元数据（时间等）发生变化（在保存时）
        /// </summary>
        public event Action OnChartPackSave;

        /// <summary>
        /// 谱包大图路径发生变化
        /// </summary>
        public event Action OnCoverFilePathChanged;

        /// <summary>
        /// 谱包小图路径发生变化
        /// </summary>
        public event Action OnCroppedCoverFilePathChanged;

        /// <summary>
        /// 谱包音乐版本数据发生变化时
        /// </summary>
        public event Action OnMusicVersionDataChanged;

        /// <summary>
        /// 谱包预览开始节拍变化时
        /// </summary>
        public event Action OnMusicPreviewStartBeatChanged;

        /// <summary>
        /// 谱包预览结束节拍变化时
        /// </summary>
        public event Action OnMusicPreviewEndBeatChanged;

        /// <summary>
        /// 谱面预备拍数发生变化
        /// </summary>
        public event Action OnReadyBeatChanged;

        /// <summary>
        /// Bpm 组发生变化
        /// </summary>
        public event Action OnBpmGroupChanged;

        /// <summary>
        /// 变速组发生变化
        /// </summary>
        public event Action OnSpeedGroupChanged;

        /// <summary>
        /// 音符组发生变化
        /// </summary>
        public event Action OnNoteDataChanged;

        /// <summary>
        /// 暂存的 Hold 开始拍发生变化
        /// </summary>
        public event Action OnTempHoldJudgeBeatChanged;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="chartPackData">要加载到编辑器的谱包数据</param>
        /// <param name="chartIndex">谱面在此谱包中的下标</param>
        /// <param name="chartData">要加载到编辑器的谱面数据</param>
        public EditorModel(ChartPackData chartPackData, int chartIndex, ChartData chartData)
        {
            ChartPackData = chartPackData;
            ChartIndex = chartIndex;
            ChartData = chartData;

            EditTool = EditTool.Select;
            PosAccuracy = 4;
            PosMagnetState = true;
            BeatAccuracy = 2;
            BeatZoom = 1f;
            TempHoldJudgeBeat = null;
            NoteIdCounter = 0;
            SelectedNotes = new HashSet<BaseChartNoteData>();

            ChartNotes = new HashSet<BaseChartNoteData>();
            foreach (var note in ChartData.Notes)
            {
                ChartNotes.Add(note);
                NoteIdCounter++;
            }
        }


        #region 编辑器管理

        public void SetEditTool(EditTool editTool)
        {
            if (EditTool == editTool)
            {
                return;
            }

            EditTool = editTool;
            OnEditToolChanged?.Invoke();
        }

        public void MenuButtonClicked(MenuButton menuButton)
        {
            OnMenuButtonClicked?.Invoke();
            throw new NotSupportedException();
        }

        public void SetPosAccuracy(string posAccuracyStr)
        {
            if (!int.TryParse(posAccuracyStr, out int posAccuracy) ||
                posAccuracy < 0)
            {
                OnEditorAttributeChanged?.Invoke();
                return;
            }

            if (PosAccuracy == posAccuracy)
            {
                return;
            }

            PosAccuracy = posAccuracy;
            OnEditorAttributeChanged?.Invoke();
        }

        public void SetPosMagnetState(bool isOn)
        {
            if (PosMagnetState == isOn)
            {
                return;
            }

            PosMagnetState = isOn;
            OnEditorAttributeChanged?.Invoke();
        }

        public void SetBeatAccuracy(string beatAccuracyStr)
        {
            if (!int.TryParse(beatAccuracyStr, out int beatAccuracy) ||
                beatAccuracy <= 0)
            {
                OnEditorAttributeChanged?.Invoke();
                return;
            }

            if (BeatAccuracy == beatAccuracy)
            {
                return;
            }

            BeatAccuracy = beatAccuracy;
            OnEditorAttributeChanged?.Invoke();
        }

        public void SetBeatZoom(string beatZoomStr)
        {
            if (!float.TryParse(beatZoomStr, out float beatZoom) ||
                beatZoom <= 0)
            {
                // 不修改值，触发刷新
                OnEditorAttributeChanged?.Invoke();
                return;
            }

            if (Mathf.Approximately(BeatZoom, beatZoom))
            {
                // 不修改值，不刷新
                return;
            }

            // 赋值，刷新
            BeatZoom = beatZoom;
            OnEditorAttributeChanged?.Invoke();
        }

        #endregion

        #region 谱包信息和谱面元数据管理

        /// <summary>
        /// 尝试更新谱包标题
        /// </summary>
        /// <param name="title">新的谱包标题</param>
        /// <remarks>如果新标题和旧标题一致，不会触发事件并返回 false</remarks>
        /// <returns>是否发生了更新（旧标题与新标题不一致）</returns>
        public bool UpdateChartPackTitle(string title)
        {
            if (ChartPackData.Title == title)
            {
                return false;
            }

            ChartPackData.Title = title;
            OnChartPackTitleChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 尝试更新曲绘大图
        /// </summary>
        /// <param name="path">曲绘大图相对路径</param>
        /// <remarks>如果路径一致，不会触发事件并返回 false</remarks>
        /// <returns>是否发生了更新</returns>
        public bool UpdateCoverFilePath(string path)
        {
            if (ChartPackData.CoverFilePath == path)
            {
                return false;
            }

            ChartPackData.CoverFilePath = path;

            OnCoverFilePathChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 尝试更新曲绘小图
        /// </summary>
        /// <param name="path">曲绘小图相对路径</param>
        /// <remarks>如果路径一致，不会触发事件并返回 false</remarks>
        /// <returns>是否发生了更新</returns>
        public bool UpdateCroppedCoverFilePath(string path)
        {
            if (ChartPackData.CroppedCoverFilePath == path)
            {
                return false;
            }

            ChartPackData.CroppedCoverFilePath = path;
            OnCroppedCoverFilePathChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 向列表添加一个新的音乐版本
        /// </summary>
        /// <param name="newMusicVersionData">要添加的音乐版本数据</param>
        /// <returns>是否成功添加，如果音乐路径已存在会返回 false 且不触发事件</returns>
        public bool AddMusicVersionDatas(MusicVersionData newMusicVersionData)
        {
            foreach (MusicVersionData musicVersionData in MusicVersionDatas)
            {
                if (musicVersionData.MusicFilePath == newMusicVersionData.MusicFilePath)
                {
                    return false;
                }
            }

            MusicVersionDatas.Add(newMusicVersionData);

            OnMusicVersionDataChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 从列表删除并返回指定下标的音乐版本数据
        /// </summary>
        /// <param name="index">下标</param>
        /// <returns>弹出的音乐版本数据</returns>
        public MusicVersionData PopMusicVersionDatas(int index)
        {
            MusicVersionData musicVersionData = MusicVersionDatas[index];
            MusicVersionDatas.RemoveAt(index);

            OnMusicVersionDataChanged?.Invoke();
            return musicVersionData;
        }

        /// <summary>
        /// 更新指定下标的音乐版本数据
        /// </summary>
        /// <remarks>注意路径不能与其他数据重复，否则会返回 false 且不会触发事件</remarks>
        /// <param name="index">要更新的下标</param>
        /// <param name="newMusicVersionData">新的音乐版本数据</param>
        /// <returns>是否成功更新</returns>
        public bool UpdateMusicVersionDatas(int index, MusicVersionData newMusicVersionData)
        {
            for (int i = 0; i < MusicVersionDatas.Count; i++)
            {
                if (i == index)
                {
                    continue;
                }

                if (MusicVersionDatas[i].MusicFilePath == newMusicVersionData.MusicFilePath)
                {
                    return false;
                }
            }

            MusicVersionDatas[index] = newMusicVersionData;

            OnMusicVersionDataChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 更新预览开始节拍
        /// </summary>
        /// <param name="beat">新的节拍</param>
        /// <returns>是否成功修改</returns>
        public bool UpdateMusicPreviewStartBeat(Beat? beat)
        {
            if (beat != null && // 如果传入的 beat 为 null，直接允许后续修改
                ChartPackData.MusicPreviewEndBeat != null &&
                ChartPackData.MusicPreviewEndBeat.Value.ToFloat() <= beat.Value.ToFloat())
            {
                // 当 endBeat 不为 null 且小于等于 startBeat 时，不允许修改
                return false;
            }

            ChartPackData.MusicPreviewStartBeat = beat;

            OnMusicPreviewStartBeatChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 更新预览结束节拍
        /// </summary>
        /// <param name="beat">新的节拍</param>
        /// <returns>是否成功修改</returns>
        public bool UpdateMusicPreviewEndBeat(Beat? beat)
        {
            if (beat != null && // 如果传入的 beat 为 null，直接允许后续修改
                ChartPackData.MusicPreviewStartBeat != null &&
                ChartPackData.MusicPreviewStartBeat.Value.ToFloat() >= beat.Value.ToFloat())
            {
                // 当 startBeat 不为 null 且大于等于 startBeat 时，不允许修改
                return false;
            }

            ChartPackData.MusicPreviewEndBeat = beat;

            OnMusicPreviewEndBeatChanged?.Invoke();
            return true;
        }

        #endregion

        #region 谱面管理

        public bool UpdateReadyBeat(int value)
        {
            if (value < 0 || value == ChartData.ReadyBeat)
            {
                return false;
            }

            ChartData.ReadyBeat = value;

            OnReadyBeatChanged?.Invoke();
            return true;
        }

        #region BPM 组管理

        /// <summary>
        /// 获取谱面下的 BPM 组
        /// </summary>
        /// <returns>BPM 组列表</returns>
        public IReadOnlyList<BpmGroupItem> GetBpmGroupItems()
        {
            return BpmGroupDatas;
        }

        /// <summary>
        /// 自动在合适的位置添加或更新 BPM 元素
        /// </summary>
        /// <remarks>将根据 beat 自动插入或更新 Bpm 组</remarks>
        /// <returns>更新后的 BPM 组列表</returns>
        public IReadOnlyList<BpmGroupItem> InsertBpmGroupItems(BpmGroupItem newItem)
        {
            int i;
            for (i = 0; i < BpmGroupDatas.Count; i++)
            {
                if (BpmGroupDatas[i].StartBeat == newItem.StartBeat)
                {
                    // beat 与已有的元素相等
                    BpmGroupDatas[i] = newItem;

                    OnBpmGroupChanged?.Invoke();
                    return BpmGroupDatas;
                }

                if (BpmGroupDatas[i].StartBeat.ToFloat() > newItem.StartBeat.ToFloat())
                {
                    // 新的 bpm 元素应作为第 i 个元素插入 bpm 组
                    break;
                }
            }

            BpmGroupDatas.Insert(i, newItem);

            OnBpmGroupChanged?.Invoke();

            return BpmGroupDatas;
        }

        public BpmGroupItem PopBpmGroupItem(int index)
        {
            BpmGroupItem bpmGroupItem = BpmGroupDatas[index];
            BpmGroupDatas.RemoveAt(index);

            OnBpmGroupChanged?.Invoke();

            return bpmGroupItem;
        }

        #endregion

        #region 变速组管理

        /// <summary>
        /// 在变速组列表末尾添加变速组
        /// </summary>
        /// <param name="speedGroupData">指定的变速组，为空时按照默认值添加</param>
        /// <returns>添加的变速组</returns>
        public SpeedGroupData AddSpeedGroupData(SpeedGroupData speedGroupData = null)
        {
            speedGroupData ??= new SpeedGroupData(type: SpeedGroupType.Relative);

            SpeedGroupDatas.Add(speedGroupData);

            OnSpeedGroupChanged?.Invoke();
            return speedGroupData;
        }

        /// <summary>
        /// 删除并返回指定下标的变速组，下标越界会抛出异常
        /// </summary>
        /// <param name="index">要删除的变速组下标</param>
        /// <returns>弹出的变速组</returns>
        public SpeedGroupData PopSpeedGroupData(int index)
        {
            SpeedGroupData speedGroupData = SpeedGroupDatas[index];
            SpeedGroupDatas.RemoveAt(index);

            OnSpeedGroupChanged?.Invoke();

            return speedGroupData;
        }

        /// <summary>
        /// 用指定的变速组替换某个已有的变速组
        /// </summary>
        /// <param name="speedGroupData">新的变速组</param>
        /// <param name="index">下标</param>
        public void UpdateSpeedGroupData(SpeedGroupData speedGroupData, int index)
        {
            SpeedGroupDatas[index] = speedGroupData;

            OnSpeedGroupChanged?.Invoke();
        }

        #endregion

        #region 音符组管理

        /// <summary>
        /// 点击 Content 空白处以创建音符
        /// </summary>
        public void CreateNote(float pos, Beat beat)
        {
            if (SelectedNotes.Count != 0)
            {
                SelectedNotes.Clear();
                OnSelectedNotesChanged?.Invoke();
            }

            if (EditTool == EditTool.HoldPen)
            {
                if (TempHoldJudgeBeat == null)
                {
                    // 点击的是开始位置，暂存 beat
                    TempHoldJudgeBeat = beat;
                    OnTempHoldJudgeBeatChanged?.Invoke();
                    return;
                }
                else if (((Beat)TempHoldJudgeBeat).ToFloat() < beat.ToFloat())
                {
                    // 点击的是结束位置，创建 HoldNote
                    ChartNotes.Add(new HoldChartNoteData(pos, (Beat)TempHoldJudgeBeat, beat));
                    OnNoteDataChanged?.Invoke();
                    TempHoldJudgeBeat = null;
                    OnTempHoldJudgeBeatChanged?.Invoke();
                    return;
                }
                else
                {
                    // 结束位置小于等于开始位置，放弃创建
                    TempHoldJudgeBeat = null;
                    OnTempHoldJudgeBeatChanged?.Invoke();
                    Debug.LogWarning("EditorModel: 无法创建持续时长小于等于 0 的 HoldNote。");
                    return;
                }
            }

            if (TempHoldJudgeBeat != null)
            {
                // 使用其他画笔点击时，清除暂存的 Hold 开始拍
                TempHoldJudgeBeat = null;
                OnTempHoldJudgeBeatChanged?.Invoke();
            }

            switch (EditTool)
            {
                // HoldNote 的创建逻辑在上文处理
                case EditTool.Select:
                case EditTool.Eraser:
                    return;
                case EditTool.TapPen:
                    if (pos < 0 || 0.8f < pos)
                    {
                        return;
                    }

                    ChartNotes.Add(new TapChartNoteData(pos, beat));
                    OnNoteDataChanged?.Invoke();
                    return;
                case EditTool.DragPen:
                    if (pos < 0 || 0.8f < pos)
                    {
                        return;
                    }

                    ChartNotes.Add(new DragChartNoteData(pos, beat));
                    OnNoteDataChanged?.Invoke();
                    return;
                case EditTool.ClickPen:
                    if (pos < 0 || 0.8f < pos)
                    {
                        return;
                    }

                    ChartNotes.Add(new ClickChartNoteData(pos, beat));
                    OnNoteDataChanged?.Invoke();
                    return;
                case EditTool.BreakPen:
                    if (Mathf.Approximately(pos, -1))
                    {
                        ChartNotes.Add(new BreakChartNoteData(BreakNotePos.Left, beat));
                        OnNoteDataChanged?.Invoke();
                        return;
                    }
                    else if (Mathf.Approximately(pos, 2))
                    {
                        ChartNotes.Add(new BreakChartNoteData(BreakNotePos.Right, beat));
                        OnNoteDataChanged?.Invoke();
                        return;
                    }

                    return;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 点击某个已存在的音符，由 note 的 button 组件触发
        /// </summary>
        /// <param name="note">音符数据</param>
        public void SelectNote(BaseChartNoteData note)
        {
            // TODO: 拓展兼容选中多个音符
            if (EditTool == EditTool.Eraser)
            {
                ChartNotes.Remove(note);
                OnNoteDataChanged?.Invoke();
                return;
            }

            SelectedNotes.Clear();
            SelectedNotes.Add(note);
            OnSelectedNotesChanged?.Invoke();
        }

        /// <summary>
        /// 为选中的 Note 设置判定拍。null 代表不修改此字段而保留原值，以兼容框选多个 note 统一修改。
        /// </summary>
        public void SetJudgeBeat(string integerPart, string numerator, string denominator)
        {
            int? b1 = (int.TryParse(integerPart, out int n1)) ? (int?)n1 : null; // 整数
            int? b2 = (int.TryParse(numerator, out int n2)) ? (int?)n2 : null; // 分子
            int? b3 = (int.TryParse(denominator, out int n3)) ? (int?)n3 : null; // 分母

            if (b1 == null && b2 == null && b3 == null)
            {
                // 输入无有效数字，通知 view 刷新内容，不更新数据
                OnNoteAttributeChanged?.Invoke();
                return;
            }

            SetNotesJudgeBeat(b1, b2, b3);
        }

        private void SetNotesJudgeBeat(int? integerPart = null, int? numerator = null, int? denominator = null)
        {
            if (integerPart is null && numerator is null && denominator is null)
            {
                return;
            }

            bool isChangedFlag = false;
            foreach (BaseChartNoteData note in SelectedNotes)
            {
                if (!Beat.TryCreateBeat(integerPart ?? note.JudgeBeat.IntegerPart,
                        numerator ?? note.JudgeBeat.Numerator,
                        denominator ?? note.JudgeBeat.Denominator,
                        out Beat? beat))
                {
                    // 合法性校验失败，通知刷新
                    OnNoteAttributeChanged?.Invoke();
                    continue;
                }

                Beat newJudgeBeat = (Beat)beat;

                if (note.JudgeBeat == newJudgeBeat)
                {
                    continue;
                }

                note.JudgeBeat = newJudgeBeat;
                isChangedFlag = true;
            }

            if (isChangedFlag)
            {
                OnNoteAttributeChanged?.Invoke();
            }
        }

        /// <summary>
        /// 为选中的 Note 设置结束拍，非 HoldNote 将不发生变化。null 代表不修改此字段而保留原值，以兼容框选多个 note 统一修改。
        /// </summary>
        public void SetEndBeat(string integerPart, string numerator, string denominator)
        {
            int? b1 = (int.TryParse(integerPart, out int n1)) ? (int?)n1 : null; // 整数
            int? b2 = (int.TryParse(numerator, out int n2)) ? (int?)n2 : null; // 分子
            int? b3 = (int.TryParse(denominator, out int n3)) ? (int?)n3 : null; // 分母

            if (b1 == null && b2 == null && b3 == null)
            {
                // 输入无有效数字，通知 view 刷新内容，不更新数据
                OnNoteAttributeChanged?.Invoke();
                return;
            }

            SetNotesEndBeat(b1, b2, b3);
        }

        private void SetNotesEndBeat(int? integerPart = null, int? numerator = null, int? denominator = null)
        {
            if (integerPart is null && numerator is null && denominator is null)
            {
                return;
            }

            bool isChangedFlag = false;
            foreach (BaseChartNoteData note in SelectedNotes)
            {
                if (note.Type != NoteType.Hold)
                {
                    Debug.Log($"EditorModel: 此 Note 不是 HoldNote，将不设定 EndBeat。{note} ");
                    continue;
                }

                HoldChartNoteData holdNote = (HoldChartNoteData)note;

                if (!Beat.TryCreateBeat(integerPart ?? holdNote.EndJudgeBeat.IntegerPart,
                        numerator ?? holdNote.EndJudgeBeat.Numerator,
                        denominator ?? holdNote.EndJudgeBeat.Denominator,
                        out Beat? beat))
                {
                    // 合法性校验失败，通知刷新
                    OnNoteAttributeChanged?.Invoke();
                    continue;
                }

                Beat newEndBeat = (Beat)beat;

                if (holdNote.EndJudgeBeat == newEndBeat)
                {
                    continue;
                }

                holdNote.EndJudgeBeat = newEndBeat;
                isChangedFlag = true;
            }

            if (isChangedFlag)
            {
                OnNoteAttributeChanged?.Invoke();
            }
        }

        /// <summary>
        /// 为选中的 Note 设置位置，BreakNote 将不发生变化。
        /// </summary>
        public void SetPos(string posStr)
        {
            if (!float.TryParse(posStr, out float pos))
            {
                // 输入无法转换为 float，通知 view 刷新内容，不更新数据
                OnNoteAttributeChanged?.Invoke();
                return;
            }

            if (pos < 0 || pos > 0.8f)
            {
                // pos 不在有效范围，通知 view 刷新内容，不更新数据
                OnNoteAttributeChanged?.Invoke();
                return;
            }

            bool isChangedFlag = false;
            foreach (BaseChartNoteData note in SelectedNotes)
            {
                if (note.Type == NoteType.Break)
                {
                    Debug.Log($"EditorModel: 此 Note 是 BreakNote，将不设定 Pos。{note}");
                    continue;
                }

                IChartNoteNormalPos posNote = (IChartNoteNormalPos)note;

                if (Mathf.Approximately(posNote.Pos, pos))
                {
                    continue;
                }

                posNote.Pos = pos;
                isChangedFlag = true;
            }

            if (isChangedFlag)
            {
                OnNoteAttributeChanged?.Invoke();
            }
        }

        public void SetBreakPos(BreakNotePos breakPos)
        {
            bool isChangedFlag = false;
            foreach (BaseChartNoteData note in SelectedNotes)
            {
                if (note.Type != NoteType.Break)
                {
                    Debug.Log($"EditorModel: 此 Note 不是 BreakNote，将不设定 BreakPos。{note}");
                    continue;
                }

                BreakChartNoteData breakNote = (BreakChartNoteData)note;

                if (breakNote.BreakNotePos == breakPos)
                {
                    continue;
                }

                breakNote.BreakNotePos = breakPos;
                isChangedFlag = true;
            }

            if (isChangedFlag)
            {
                OnNoteAttributeChanged?.Invoke();
            }
        }

        #endregion

        // TODO: ChartTrackData 轨道拓展数据下个版本再说

        #endregion
    }
}
