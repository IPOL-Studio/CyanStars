using System.Collections.Generic;

namespace CyanStars.Chart
{
    /// <summary>
    /// 运行时谱包数据
    /// </summary>
    public class RuntimeChartPack
    {
        public readonly ChartPackData ChartPackData;

        /// <summary>
        /// 是否为内置谱包，内置谱包的定数可用于计算玩家实力水平，内置谱包的定数要求为 1~20 的 float
        /// </summary>
        /// <remarks>
        /// 此值在加载时根据加载方式确定
        /// </remarks>
        public readonly bool IsInternal;

        /// <summary>
        /// 谱包各个难度的定数，用于计算玩家实力
        /// </summary>
        /// <remarks>社区谱包此值无意义</remarks>
        public readonly ChartPackLevels Levels;

        /// <summary>
        /// 谱包工作区的绝对路径
        /// </summary>
        /// <remarks>谱包索引文件所在的目录，基于此路径读取其中的谱面和资源文件</remarks>
        public readonly string WorkspacePath;

        /// <summary>
        /// 校验后可供游玩的难度，即有且仅有一个对应谱面的难度
        /// </summary>
        public readonly HashSet<ChartDifficulty> DifficultiesAbleToPlay;


        public RuntimeChartPack(ChartPackData chartPackData, bool isInternal, ChartPackLevels levels,
            string workspacePath, HashSet<ChartDifficulty> difficultiesAbleToPlay)
        {
            ChartPackData = chartPackData;
            IsInternal = isInternal;
            Levels = levels;
            WorkspacePath = workspacePath;
            DifficultiesAbleToPlay = difficultiesAbleToPlay;
        }
    }
}
