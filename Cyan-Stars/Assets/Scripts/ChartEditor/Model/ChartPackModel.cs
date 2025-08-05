using System;
using System.Collections.Generic;
using CyanStars.Chart;

namespace CyanStars.ChartEditor.Model
{
    /// <summary>
    /// 谱面编辑器 Model 层
    /// </summary>
    public class ChartPackModel
    {
        public ChartPackData ChartPackData { get; private set; }

        public int ChartIndex { get; private set; }

        public ChartData ChartData { get; private set; }


        public List<MusicVersionData> MusicVersionDatas => ChartPackData.MusicVersionDatas;

        public List<BpmGroupItem> BpmGroupDatas => ChartData.BpmGroup.Groups;

        public List<SpeedGroupData> SpeedGroupDatas => ChartData.SpeedGroupDatas;


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


        #region 谱包信息和谱面元数据管理

        /// <summary>
        /// 从指定路径加载谱包和谱面
        /// </summary>
        /// <param name="chartPackPath">谱包路径</param>
        /// <param name="chartIndex">选定的谱面下标</param>
        /// <returns>谱包数据</returns>
        /// <exception cref="System.NotImplementedException">没做完</exception>
        public ChartPackData LoadChartPack(string chartPackPath, int chartIndex)
        {
            throw new NotImplementedException();
            // TODO: 加载谱包
            // ChartPackData = ;
            // ChartIndex = chartIndex;
            // ChartData = ;
            // OnChartDataChanged?.Invoke();
        }

        /// <summary>
        /// 创建新谱包和默认谱面
        /// </summary>
        /// <param name="title">谱包标题，会同时用于文件夹名和文件内标题</param>
        /// <returns>谱包数据</returns>
        public ChartPackData CreateChartPackData(string title)
        {
            ChartData chartData = new ChartData();
            ChartPackData = new ChartPackData(title);
            // TODO: 获取谱面保存的相对路径，写到谱包元数据中，并创建谱面文件
            // ChartPackData.Charts = new List<ChartMetadata>(new ChartMetadata(filePath: ________));
            OnChartPackDataChanged?.Invoke();
            return ChartPackData;
        }

        /// <summary>
        /// 保存谱包到文件
        /// </summary>
        /// <exception cref="NotImplementedException">没做完</exception>
        public void SaveChartPack()
        {
            throw new NotImplementedException();
            // TODO: 保存谱包
        }

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

        public bool CreateChart(ChartDifficulty? chartDifficulty = null)
        {
            if (chartDifficulty != null)
            {
                foreach (ChartMetadata chartMetadata in ChartPackData.ChartMetaDatas)
                {
                    if (chartMetadata.Difficulty == chartDifficulty)
                    {
                        return false;
                    }
                }
            }

            throw new NotImplementedException();
            // TODO: 创建谱面并将谱面相对路径写入谱包元数据
        }

        public bool DeleteChart(int index, out ChartMetadata chartMetadata, out ChartData chartData)
        {
            throw new NotImplementedException();
            // TODO: 删除谱面和元数据
        }

        public bool UpdateChart(int index, ChartMetadata chartMetadata, ChartData chartData)
        {
            throw new NotImplementedException();
            // TODO:更新指定 index 的谱面
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

        # region 谱面管理

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
        /// 获取谱面下的变速组
        /// </summary>
        /// <returns>变速组列表</returns>
        public IReadOnlyList<SpeedGroupData> GetSpeedGroups()
        {
            return SpeedGroupDatas;
        }

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

        #endregion
    }
}
