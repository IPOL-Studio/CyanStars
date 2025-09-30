namespace CyanStars.Chart
{
    /// <summary>
    /// 运行时谱包数据
    /// </summary>
    public class RuntimeChartPack
    {
        public readonly ChartPackData ChartPackData;

        /// <summary>
        /// 是否为内置谱面，内置谱面的定数可用于计算玩家实力水平，内置谱面的定数要求为 1~20 的整数
        /// </summary>
        /// <remarks>
        /// 此值在加载时根据加载方式确定
        /// </remarks>
        public readonly bool IsInternal;

        /// <summary>
        /// 谱包工作区的绝对路径
        /// </summary>
        /// <remarks>谱包索引文件所在的目录，基于此路径读取其中的谱面和资源文件</remarks>
        public readonly string WorkspacePath;


        public RuntimeChartPack(ChartPackData chartPackData, bool isInternal, string workspacePath)
        {
            ChartPackData = chartPackData;
            IsInternal = isInternal;
            WorkspacePath = workspacePath;
        }
    }
}
