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

        public int NoteIdCounter { get; private set; }

        public List<ModelChartNoteData> ChartNotes { get; private set; }

        public List<int> SelectedNoteIDs { get; private set; } // 当前选中的 Note ID，用列表是考虑兼容后续框选多个 Note 一起修改


        // --- 从磁盘加载到内存中的、经过校验后的谱包和谱面数据，加载/保存时需要从读写磁盘。 ---
        public ChartPackData ChartPackData { get; private set; }

        public int ChartIndex { get; private set; }

        public ChartData ChartData { get; private set; }


        public List<MusicVersionData> MusicVersionDatas => ChartPackData.MusicVersionDatas;

        public List<BpmGroupItem> BpmGroupDatas => ChartData.BpmGroup.Groups;

        public List<SpeedGroupData> SpeedGroupDatas => ChartData.SpeedGroupDatas;


        // --- 编辑器事件 ---

        /// <summary>
        /// 编辑器任意内容发生变化
        /// </summary>
        public event Action OnEditorDataChanged;

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


        // --- 谱包事件 ---

        /// <summary>
        /// 谱包中任意内容发生变化
        /// </summary>
        public event Action OnChartPackDataChanged;

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

        // --- 谱面事件 ---

        /// <summary>
        /// 谱面中任意内容发生变化
        /// </summary>
        public event Action OnChartDataChanged;

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
            NoteIdCounter = 0;
            SelectedNoteIDs = new List<int>();

            ChartNotes = new List<ModelChartNoteData>();
            foreach (var note in ChartData.Notes)
            {
                ChartNotes.Add(new ModelChartNoteData(NoteIdCounter, note));
                NoteIdCounter++;
            }
        }

        /// <summary>
        /// 根据 ID 搜索音符
        /// </summary>
        /// <param name="targetId">音符 ID，与视图层 ID 和 ModelChartNoteData.ID 一致</param>
        /// <param name="linearCheckCount">在执行二分查找前先对末尾x个元素进行线性查找</param>
        /// <param name="modelChartNoteData">返回的元素</param>
        /// <returns>是否搜索到对应元素</returns>
        private bool SearchNote(int targetId, out ModelChartNoteData modelChartNoteData, int linearCheckCount = 5)
        {
            modelChartNoteData = null;

            if (ChartNotes == null || ChartNotes.Count == 0)
            {
                return false;
            }

            // --- 阶段 1: 反向线性查找 (针对高频访问的末尾区域) ---
            int count = ChartNotes.Count;
            // 确保 linearCheckCount 是一个合理的正数，且不超过列表总数
            if (linearCheckCount <= 0) linearCheckCount = 1;
            int checkCount = Math.Min(count, linearCheckCount);

            for (int i = count - 1; i >= count - checkCount; i--)
            {
                if (ChartNotes[i].ID != targetId)
                {
                    continue;
                }

                modelChartNoteData = ChartNotes[i];
                return true;
            }

            // --- 阶段 2: 二分查找 (针对列表的其余部分) ---

            int searchUpperBound = count - checkCount - 1;

            // 如果二分查找的范围无效 (例如，线性部分已覆盖全部)，或者目标 ID 小于该范围内的最小值，则没有必要进行二分查找。
            if (searchUpperBound < 0 || targetId < ChartNotes[0].ID || targetId > ChartNotes[searchUpperBound].ID)
            {
                return false;
            }

            // 执行二分查找
            int low = 0;
            int high = searchUpperBound;

            while (low <= high)
            {
                int mid = low + (high - low) / 2;
                int midId = ChartNotes[mid].ID;

                if (midId == targetId)
                {
                    modelChartNoteData = ChartNotes[mid];
                    return true;
                }

                if (midId < targetId)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            // 两个阶段都搜索完毕，未找到
            return false;
        }

        #region 编辑器管理

        public void SetEditTool(EditTool editTool)
        {
            if (EditTool == editTool)
            {
                return;
            }

            EditTool = editTool;
            OnEditorDataChanged?.Invoke();
            OnEditToolChanged?.Invoke();
        }

        public void MenuButtonClicked(MenuButton menuButton)
        {
            OnEditorDataChanged?.Invoke();
            OnMenuButtonClicked?.Invoke();
            throw new NotSupportedException();
        }

        public void SetPosAccuracy(string posAccuracyStr)
        {
            if (!int.TryParse(posAccuracyStr, out int posAccuracy) ||
                posAccuracy < 0)
            {
                OnEditorDataChanged?.Invoke();
                OnEditorAttributeChanged?.Invoke();
                return;
            }

            if (PosAccuracy == posAccuracy)
            {
                return;
            }

            PosAccuracy = posAccuracy;
            OnEditorDataChanged?.Invoke();
            OnEditorAttributeChanged?.Invoke();
        }

        public void SetPosMagnetState(bool isOn)
        {
            if (PosMagnetState == isOn)
            {
                return;
            }

            PosMagnetState = isOn;
            OnEditorDataChanged?.Invoke();
            OnEditorAttributeChanged?.Invoke();
        }

        public void SetBeatAccuracy(string beatAccuracyStr)
        {
            if (!int.TryParse(beatAccuracyStr, out int beatAccuracy) ||
                beatAccuracy <= 0)
            {
                OnEditorDataChanged?.Invoke();
                OnEditorAttributeChanged?.Invoke();
                return;
            }

            if (BeatAccuracy == beatAccuracy)
            {
                return;
            }

            BeatAccuracy = beatAccuracy;
            OnEditorDataChanged?.Invoke();
            OnEditorAttributeChanged?.Invoke();
        }

        public void SetBeatZoom(string beatZoomStr)
        {
            if (!float.TryParse(beatZoomStr, out float beatZoom) ||
                beatZoom <= 0)
            {
                // 不修改值，触发刷新
                OnEditorDataChanged?.Invoke();
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
            OnEditorDataChanged?.Invoke();
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
            OnChartPackDataChanged?.Invoke();
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

            OnChartPackDataChanged?.Invoke();
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
            OnChartPackDataChanged?.Invoke();
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

            OnChartPackDataChanged?.Invoke();
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

            OnChartPackDataChanged?.Invoke();
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

            OnChartPackDataChanged?.Invoke();
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

            OnChartPackDataChanged?.Invoke();
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

            OnChartPackDataChanged?.Invoke();
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

            OnChartDataChanged?.Invoke();
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

                    OnChartDataChanged?.Invoke();
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

            OnChartDataChanged?.Invoke();
            OnBpmGroupChanged?.Invoke();

            return BpmGroupDatas;
        }

        public BpmGroupItem PopBpmGroupItem(int index)
        {
            BpmGroupItem bpmGroupItem = BpmGroupDatas[index];
            BpmGroupDatas.RemoveAt(index);

            OnChartDataChanged?.Invoke();
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

            OnChartDataChanged?.Invoke();
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

            OnChartDataChanged?.Invoke();
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

            OnChartDataChanged?.Invoke();
            OnSpeedGroupChanged?.Invoke();
        }

        #endregion

        #region 音符组管理

        /// <summary>
        /// 为选中的 Note 设置判定拍。null 代表不修改此字段而保留原值，以兼容框选多个 note 统一修改。
        /// </summary>
        public void SetNotesJudgeBeat(int? integerPart = null, int? numerator = null, int? denominator = null)
        {
            if (integerPart is null && numerator is null && denominator is null)
            {
                OnEditorDataChanged?.Invoke();
                OnNoteAttributeChanged?.Invoke();
                return;
            }

            bool isChangedFlag = false;
            foreach (int id in SelectedNoteIDs)
            {
                if (!SearchNote(id, out ModelChartNoteData note))
                {
                    Debug.LogWarning($"EditorModel: 未找到 ID 为 {id} 的 Note");
                    continue;
                }

                Beat newJudgeBeat = new Beat(
                    integerPart ?? note.NoteData.JudgeBeat.IntegerPart,
                    numerator ?? note.NoteData.JudgeBeat.Numerator,
                    denominator ?? note.NoteData.JudgeBeat.Denominator
                );

                if (note.NoteData.JudgeBeat == newJudgeBeat)
                {
                    continue;
                }

                note.NoteData.JudgeBeat = newJudgeBeat;
                isChangedFlag = true;
            }

            if (isChangedFlag)
            {
                OnEditorDataChanged?.Invoke();
                OnNoteAttributeChanged?.Invoke();
            }
        }

        #endregion

        // TODO: ChartTrackData 轨道拓展数据下个版本再说

        #endregion
    }
}
