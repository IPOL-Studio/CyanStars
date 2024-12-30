namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 谱面难度
    /// </summary>
    /// <remarks>
    /// 可为null，代表未定义难度的谱面，可在编辑器内编辑，但不能被游戏读取
    /// </remarks>
    public enum ChartDifficulty
    {
        KuiXing = 0, // 窥星（最简单）
        QiMing = 1, // 启明
        TianShu = 2, // 天枢
        WuYin = 3 // 无垠（最难）
    }
}
