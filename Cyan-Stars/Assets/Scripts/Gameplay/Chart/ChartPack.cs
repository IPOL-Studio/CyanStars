namespace CyanStars.Gameplay.Chart
{
    /// <summary>
    /// 运行时谱包数据
    /// </summary>
    public class ChartPack
    {
        /// <summary>
        /// 是否为内置谱面，内置谱面的定数可用于计算玩家实力水平，内置谱面的定数要求为 1~20 的整数
        /// </summary>
        /// <remarks>
        /// 此值在加载时根据加载方式确定
        /// </remarks>
        public bool IsInternal;

        public ChartPackData ChartPackData;
    }
}
