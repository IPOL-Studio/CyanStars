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
        /// <summary>
        /// 编辑器精简模式
        /// </summary>
        /// <remarks>精简无关元素方便新手理解制谱器，只允许设置 1 个 BPM 组、曲目版本，隐藏变速和事件相关设置</remarks>
        public bool IsSimplification { get; private set; }

        /// <summary>
        /// 当前选中的画笔（或者是橡皮？）
        /// </summary>
        public EditTool SelectedEditTool { get; private set; }


        /// <summary>
        /// 当前设置的位置精度（等于屏幕上有几条竖直位置线）
        /// </summary>
        public int PosAccuracy { get; private set; }

        /// <summary>
        /// 是否开启了位置吸附
        /// </summary>
        /// <remarks>将会吸附到最近的位置线，或者是两条位置线的中点</remarks>
        public bool PosMagnetState { get; private set; }

        /// <summary>
        /// 节拍精度（等于将每小节均分为几分）
        /// </summary>
        public int BeatAccuracy { get; private set; }

        /// <summary>
        /// 节拍缩放
        /// </summary>
        /// <remarks>编辑器纵向拉伸比例</remarks>
        public float BeatZoom { get; private set; }

        /// <summary>
        /// 创建 Hold 音符时暂存的开始拍
        /// </summary>
        /// <remarks>选中 Hold 画笔后第一次点击空白区域</remarks>
        public Beat? TempHoldJudgeBeat;


        /// <summary>
        /// 用于 M 层的音符数据，在 Model 构造时指向 ChartData 中的对象
        /// </summary>
        /// <remarks>NoteView 在从对象池取回时会存储对应的单个音符数据，可以借此定位回来</remarks>
        public HashSet<BaseChartNoteData> ChartNotes { get; private set; }

        /// <summary>
        /// 当前选中的 Note，用 HashSet 是考虑兼容后续框选多个 Note 一起修改
        /// </summary>
        public HashSet<BaseChartNoteData> SelectedNotes { get; private set; }

        /// <summary>
        /// 计算 offset 后，当前选中的音乐的实际时长（ms）
        /// </summary>
        /// <remarks>超过这个时长的内容都不可以编辑，包括音符编辑、事件等等</remarks>
        public int ActualMusicTime { get; set; } // TODO：测试完成后改为 private set;

        /// <summary>
        /// 音乐版本弹窗可见性
        /// </summary>
        public bool MusicVersionCanvasVisibleness { get; private set; }

        /// <summary>
        /// 谱包信息弹窗可见性
        /// </summary>
        public bool ChartPackDataCanvasVisibleness { get; private set; }


        // --- 从磁盘加载到内存中的、经过校验后的谱包和谱面数据，加载/保存时需要从读写磁盘。 ---
        public ChartPackData ChartPackData { get; private set; }

        public int ChartIndex { get; private set; }

        public ChartData ChartData { get; private set; }

        public List<MusicVersionData> MusicVersionDatas => ChartPackData.MusicVersionDatas;

        public List<BpmGroupItem> BpmGroupDatas => ChartData.BpmGroup.Data;

        public List<SpeedGroupData> SpeedGroupDatas => ChartData.SpeedGroupDatas;


        // --- Model 事件 ---

        /// <summary>
        /// 进入/退出精简模式
        /// </summary>
        public event Action OnSimplificationChanged;

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
        /// 谱包信息（谱面元数据）发生变化
        /// </summary>
        public event Action OnChartPackDataChanged;

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
        /// 曲目弹窗打开或关闭
        /// </summary>
        public event Action OnMusicVersionCanvasVisiblenessChanged;

        /// <summary>
        /// 谱包弹窗打开或关闭
        /// </summary>
        public event Action OnChartPackDataCanvasVisiblenessChanged;


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

            IsSimplification = true;
            SelectedEditTool = EditTool.Select;
            PosAccuracy = 4;
            PosMagnetState = true;
            BeatAccuracy = 2;
            BeatZoom = 1f;
            TempHoldJudgeBeat = null;
            SelectedNotes = new HashSet<BaseChartNoteData>();

            ChartNotes = new HashSet<BaseChartNoteData>();
            foreach (var note in ChartData.Notes)
            {
                ChartNotes.Add(note);
            }

            MusicVersionCanvasVisibleness = false;
            ChartPackDataCanvasVisibleness = false;
        }


        #region 编辑器管理

        public void SetEditTool(EditTool editTool)
        {
            if (SelectedEditTool == editTool)
            {
                return;
            }

            SelectedEditTool = editTool;
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

        public void SetMusicVersionCanvasVisibleness(bool isVisible)
        {
            if (MusicVersionCanvasVisibleness == isVisible)
            {
                return;
            }

            MusicVersionCanvasVisibleness = isVisible;
            OnMusicVersionCanvasVisiblenessChanged?.Invoke();
        }

        public void SetChartPackDataCanvasVisibleness(bool isVisible)
        {
            if (ChartPackDataCanvasVisibleness == isVisible)
            {
                return;
            }

            ChartPackDataCanvasVisibleness = isVisible;
            OnChartPackDataCanvasVisiblenessChanged?.Invoke();
        }

        #endregion

        #region 谱包信息和谱面元数据管理

        /// <summary>
        /// 更新谱包标题
        /// </summary>
        /// <param name="title">新的谱包标题</param>
        public void UpdateChartPackTitle(string title)
        {
            if (ChartPackData.Title != title)
            {
                ChartPackData.Title = title;
                OnChartPackDataChanged?.Invoke();
            }
        }

        public void UpdatePreviewStareBeat(string integerPartString, string numeratorString, string denominatorString)
        {
            if (!(int.TryParse(integerPartString, out int integerPart) &&
                  int.TryParse(numeratorString, out int numerator) &&
                  int.TryParse(denominatorString, out int denominator)))
            {
                OnChartPackDataChanged?.Invoke();
                return;
            }

            if (!Beat.TryCreateBeat(integerPart, numerator, denominator, out Beat? newBeat) ||
                ((Beat)newBeat).ToFloat() > ChartPackData.MusicPreviewEndBeat.ToFloat())
            {
                OnChartPackDataChanged?.Invoke();
                return;
            }

            if (ChartPackData.MusicPreviewStartBeat != (Beat)newBeat)
            {
                ChartPackData.MusicPreviewStartBeat = (Beat)newBeat;
                OnChartPackDataChanged?.Invoke();
            }
        }

        public void UpdatePreviewEndBeat(string integerPartString, string numeratorString, string denominatorString)
        {
            if (!(int.TryParse(integerPartString, out int integerPart) &&
                  int.TryParse(numeratorString, out int numerator) &&
                  int.TryParse(denominatorString, out int denominator)))
            {
                OnChartPackDataChanged?.Invoke();
                return;
            }

            if (!Beat.TryCreateBeat(integerPart, numerator, denominator, out Beat? newBeat) ||
                ((Beat)newBeat).ToFloat() < ChartPackData.MusicPreviewStartBeat.ToFloat())
            {
                OnChartPackDataChanged?.Invoke();
                return;
            }

            if (ChartPackData.MusicPreviewEndBeat != (Beat)newBeat)
            {
                ChartPackData.MusicPreviewEndBeat = (Beat)newBeat;
                OnChartPackDataChanged?.Invoke();
            }
        }

        /// <summary>
        /// 尝试更新曲绘大图
        /// </summary>
        /// <param name="path">曲绘大图相对路径</param>
        /// <remarks>如果路径一致，不会触发事件并返回 false</remarks>
        /// <returns>是否发生了更新</returns>
        public void UpdateCoverFilePath(string path)
        {
            if (ChartPackData.CoverFilePath != path)
            {
                ChartPackData.CoverFilePath = path;
                OnChartPackDataChanged?.Invoke();
            }
        }

        /// <summary>
        /// 尝试更新曲绘小图
        /// </summary>
        /// <param name="path">曲绘小图相对路径</param>
        /// <remarks>如果路径一致，不会触发事件并返回 false</remarks>
        /// <returns>是否发生了更新</returns>
        public void UpdateCroppedCoverFilePath(string path)
        {
            if (ChartPackData.CroppedCoverFilePath == path)
            {
                ChartPackData.CroppedCoverFilePath = path;
                OnChartPackDataChanged?.Invoke();
            }
        }

        /// <summary>
        /// 向列表添加一个新的音乐版本
        /// </summary>
        /// <param name="newData">要添加的音乐版本数据</param>
        public void AddMusicVersionItem(MusicVersionData newData = null)
        {
            newData = newData ?? new MusicVersionData();
            foreach (MusicVersionData musicVersionData in MusicVersionDatas)
            {
                if (musicVersionData.AudioFilePath == newData.AudioFilePath)
                {
                    return;
                }
            }

            MusicVersionDatas.Add(newData);

            OnMusicVersionDataChanged?.Invoke();
        }

        /// <summary>
        /// 从列表删除音乐版本数据元素
        /// </summary>
        public void DeleteMusicVersionItem(MusicVersionData oldItem)
        {
            MusicVersionDatas.Remove(oldItem);
            OnMusicVersionDataChanged?.Invoke();
        }

        public void TopMusicVersionItem(MusicVersionData item)
        {
            MusicVersionDatas.Remove(item);
            MusicVersionDatas.Insert(0, item);
            OnMusicVersionDataChanged?.Invoke();
        }

        public void CopyMusicVersionItem(MusicVersionData item)
        {
            MusicVersionData copiedItem = new MusicVersionData(item.VersionTitle, item.AudioFilePath, item.Offset,
                new Dictionary<string, List<string>>());
            foreach (KeyValuePair<string, List<string>> staff in item.Staffs)
            {
                List<string> copiedStaffJobs = new List<string>(staff.Value);
                copiedItem.Staffs.Add(staff.Key, copiedStaffJobs);
            }

            MusicVersionDatas.Add(copiedItem);
            OnMusicVersionDataChanged?.Invoke();
        }

        public void UpdateMusicVersionTitle(MusicVersionData oldItem, string newTitle)
        {
            int itemIndex = MusicVersionDatas.IndexOf(oldItem);
            if (MusicVersionDatas[itemIndex].VersionTitle != newTitle)
            {
                MusicVersionDatas[itemIndex].VersionTitle = newTitle;
                OnMusicVersionDataChanged?.Invoke();
            }
        }

        public void UpdateMusicVersionOffset(MusicVersionData oldItem, string newOffsetString)
        {
            if (!int.TryParse(newOffsetString, out int newOffset))
            {
                OnMusicVersionDataChanged?.Invoke();
            }

            int itemIndex = MusicVersionDatas.IndexOf(oldItem);
            if (MusicVersionDatas[itemIndex].Offset != newOffset)
            {
                MusicVersionDatas[itemIndex].Offset = newOffset;
                OnMusicVersionDataChanged?.Invoke();
            }
        }

        public void AddMusicVersionOffsetValue(MusicVersionData oldItem, int addNumber)
        {
            if (addNumber == 0)
            {
                return;
            }

            int itemIndex = MusicVersionDatas.IndexOf(oldItem);
            MusicVersionDatas[itemIndex].Offset += addNumber;
            OnMusicVersionDataChanged?.Invoke();
        }

        public void AddStaffItem(MusicVersionData oldItem)
        {
            int i = 1;
            while (oldItem.Staffs.ContainsKey("Staff" + i.ToString()))
            {
                i++;
            }

            oldItem.Staffs.Add("Staff" + i.ToString(), new List<string>());
            OnMusicVersionDataChanged?.Invoke();
        }

        public void DeleteStaffItem(MusicVersionData oldItem, KeyValuePair<string, List<string>> oldStaffItem)
        {
            oldItem.Staffs.Remove(oldStaffItem.Key);
            OnMusicVersionDataChanged?.Invoke();
        }

        public void UpdateStaffItem(MusicVersionData oldMusicVersionItem,
            KeyValuePair<string, List<string>> oldStaffItem, string newName, string newJobString)
        {
            if (oldStaffItem.Key == newName && string.Join("/", oldStaffItem.Value) == newJobString ||
                oldStaffItem.Key != newName && oldMusicVersionItem.Staffs.ContainsKey(newName))
            {
                OnMusicVersionDataChanged?.Invoke();
                return;
            }

            List<string> newJob = new List<string>(newJobString.Split('/'));

            if (oldStaffItem.Key != newName)
            {
                oldMusicVersionItem.Staffs.Remove(oldStaffItem.Key);
                oldMusicVersionItem.Staffs.Add(newName, newJob);
                OnMusicVersionDataChanged?.Invoke();
                return;
            }
            else
            {
                oldMusicVersionItem.Staffs[newName] = newJob;
                OnMusicVersionDataChanged?.Invoke();
                return;
            }
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

            if (SelectedEditTool == EditTool.HoldPen)
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

            switch (SelectedEditTool)
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
            if (SelectedEditTool == EditTool.Eraser)
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
