using System;
using System.Collections.Generic;
using CyanStars.Chart;

namespace CyanStars.ChartEditor.Model
{
    /// <summary>
    /// 谱面编辑器 Model 层
    /// </summary>
    public class ChartModel
    {
        public ChartPackData ChartPackData { get; private set; }

        public int ChartIndex { get; private set; }

        public ChartData ChartData { get; private set; }


        public List<SpeedGroupData> SpeedGroupDatas => ChartData.SpeedGroupDatas;

        public List<BpmGroupItem> BpmGroupDatas => ChartData.BpmGroup.Groups;

        /// <summary>
        /// 谱包中任意内容发生变化
        /// </summary>
        public event Action OnChanged;

        /// <summary>
        /// Bpm 组发生变化
        /// </summary>
        public event Action OnBpmGroupChanged;

        /// <summary>
        /// 变速组发生变化
        /// </summary>
        public event Action OnSpeedGroupChanged;


        #region 谱包和谱面管理

        /// <summary>
        /// 从指定路径加载谱包和谱面
        /// </summary>
        /// <param name="chartPackPath">谱包路径</param>
        /// <param name="chartIndex">选定的谱面下标</param>
        /// <returns>谱包数据</returns>
        /// <exception cref="System.NotImplementedException">没做完</exception>
        public ChartPackData LoadChartPack(string chartPackPath, int chartIndex)
        {
            throw new System.NotImplementedException();
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
        public ChartPackData CreateChartPack(string title)
        {
            ChartData chartData = new ChartData();
            ChartPackData = new ChartPackData(title);
            // TODO: 获取谱面保存的相对路径，写到谱包元数据中，并创建谱面文件
            // ChartPackData.Charts = new List<ChartMetadata>(new ChartMetadata(filePath: ________));
            OnChanged?.Invoke();
            return ChartPackData;
        }

        #endregion

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
        /// 自动在合适的位置添加 BPM 元素
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

                    OnChanged?.Invoke();
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

            OnChanged?.Invoke();
            OnBpmGroupChanged?.Invoke();

            return BpmGroupDatas;
        }

        public BpmGroupItem PopBpmGroupItem(int index)
        {
            BpmGroupItem bpmGroupItem = BpmGroupDatas[index];
            BpmGroupDatas.RemoveAt(index);

            OnChanged?.Invoke();
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

            OnChanged?.Invoke();
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

            OnChanged?.Invoke();
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

            OnChanged?.Invoke();
            OnSpeedGroupChanged?.Invoke();
        }

        #endregion
    }
}
