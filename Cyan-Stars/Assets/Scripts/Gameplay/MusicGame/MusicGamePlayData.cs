using System.Collections.Generic;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 玩家游玩音游时的数据的结构
    /// </summary>
    public struct MusicGamePlayData
    {
        /* 以下的“分数”和“总分”等为Note计分：
         * Tap/Hold头/Hold尾/Click头/Click尾，各1分
         * Drag，0.25分
         * Break，2分 */
        public int Combo; // Combo数量
        public int MaxCombo; // 玩家在此次游玩时的最大连击数量
        public float Score; // 当前分数
        public float MaxScore; // 当前的理论最高分
        public float FullScore; // 全谱总分
        public EvaluateType Grade; // 当前Note的判定评价
        public float ImpurityRate; // 杂率
        public float CurrentDeviation; // 当前Note的偏移
        public List<float> DeviationList; // 各个音符的偏移
        public int ExactNum;
        public int GreatNum;
        public int RightNum;
        public int OutNum;
        public int BadNum;
        public int MissNum;
    }
}
