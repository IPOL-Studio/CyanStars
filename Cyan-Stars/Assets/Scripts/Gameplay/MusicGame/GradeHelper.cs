using CyanStars.Gameplay.GameSave;

namespace CyanStars.Gameplay.MusicGame
{
    public static class GradeHelper
    {
        /// <summary>
        /// 按照游玩数据返回评级
        /// </summary>
        /// <remarks>
        /// 评级规则：
        /// - 全 Exact：
        ///   - 且杂率小于等于 30ms: UltraPure
        ///   - 且杂率小于等于 50ms: AllExactPlus
        ///   - 否则: AllExact
        /// - 无 Miss 和 Bad：
        ///   - 且杂率小于等于 50ms: FullComboPlus
        ///   - 否则: FullCombo
        /// - 否则：
        ///   - Clear
        /// </remarks>
        /// <param name="musicGamePlayData">游玩数据</param>
        /// <returns>成绩评级</returns>
        public static ChartGrade GetGrade(MusicGamePlayData musicGamePlayData)
        {
            if (musicGamePlayData.GreatNum +
                musicGamePlayData.RightNum +
                musicGamePlayData.OutNum +
                musicGamePlayData.BadNum +
                musicGamePlayData.MissNum == 0)
            {
                if (musicGamePlayData.ImpurityRate <= 30f)
                {
                    return ChartGrade.UltraPure;
                }
                else if (musicGamePlayData.ImpurityRate <= 50f)
                {
                    return ChartGrade.AllExactPlus;
                }
                else
                {
                    return ChartGrade.AllExact;
                }
            }
            else if (musicGamePlayData.BadNum +
                     musicGamePlayData.MissNum == 0)
            {
                if (musicGamePlayData.ImpurityRate <= 50f)
                {
                    return ChartGrade.FullComboPlus;
                }
                else
                {
                    return ChartGrade.FullCombo;
                }
            }
            else
            {
                return ChartGrade.Clear;
            }
        }
    }
}
