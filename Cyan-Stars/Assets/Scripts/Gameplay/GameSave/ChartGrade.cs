namespace CyanStars.Gameplay.GameSave
{
    /// <summary>
    /// 音游谱面成绩评级
    /// </summary>
    /// <remarks>
    /// 可为null，代表玩家从未游玩/完成过该谱面
    /// </remarks>
    public enum ChartGrade
    {
        Clear,
        FullCombo,
        FullComboPlus,
        AllExact,
        AllExactPlus,
        UltraPure
    }
}
