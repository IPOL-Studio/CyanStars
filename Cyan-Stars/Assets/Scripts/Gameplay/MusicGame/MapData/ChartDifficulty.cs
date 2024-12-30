namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 谱面难度
    /// </summary>
    public enum ChartDifficulty
    {
        Undefined = 0, // 未定义的难度，可有多个存在于谱包内，可以被谱面编辑器加载，但不会被游戏加载
        KuiXing = 1, // 窥星（最简单）
        QiMing = 2, // 启明
        TianShu = 3, // 天枢
        WuYin = 4 // 无垠（最难）
    }
}
