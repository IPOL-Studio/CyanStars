using System;
using System.Collections.Generic;

namespace CyanStars.Gameplay.GameSave
{
    /// <summary>
    ///     玩家存档结构
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public int Version { get; set; } // 存档版本
        public DateTime CreateTime { get; set; } // 存档首次创建的时间
        public DateTime SaveTime { get; set; } // 存档末次保存的时间

        public MusicGameSaveData MusicGameData { get; set; }
        // public StorySaveData StoryData { get;set; } // Todo:剧情模式存档数据

        public string Verification { get; set; } // 存档数据校验

        /// <summary>
        ///     音游成绩类
        /// </summary>
        [Serializable]
        public class MusicGameSaveData
        {
            /// <summary>
            ///     string 为谱包哈希，此值在谱师导出谱面时固定
            /// </summary>
            public Dictionary<string, ChartPackSaveData> ChartPackSaveDatas { get; set; }

            [Serializable]
            public class ChartPackSaveData
            {
                /// <summary>
                ///     谱面难度
                /// </summary>
                public enum ChartDifficulty
                {
                    KuiXing = 0, // 窥星（最简单）
                    QiMing = 1, // 启明
                    TianShu = 2, // 天枢
                    WuYin = 3 // 无垠（最难）
                }

                public Dictionary<ChartDifficulty, ChartSaveData> ChartSaveDatas { get; set; }

                /// <summary>
                ///     每个谱面的成绩。
                ///     内置谱和社区谱通用的结构，如果找不到谱面但有分数，仍然记录分数，导入谱面后即可正常展示分数。
                /// </summary>
                [Serializable]
                public class ChartSaveData
                {
                    /// <summary>
                    ///     成绩评级
                    /// </summary>
                    public enum ScoreGrade
                    {
                        Clear,
                        FullCombo,
                        FullComboPlus,
                        AllExact,
                        AllExactPlus,
                        UltraPure
                    }

                    public int PlayCount { get; set; } // 玩家累计游玩此谱面的次数

                    public DateTime FirstPlayTime { get; set; } // 首次游玩时间（进入结算页时时间）

                    /*  注意：这部分的 BestImpurityRate 等（下方以星号标识），只有在 BestScore 刷新时才记录为对应的数据
                        换言之，即使打出了更好的 BestImpurityRate，但未打出新的最高分，也不更新这些属性
                        而打出新的最高分后，无论这些属性是否增加或降低，全部更新记录为此时的成绩  */
                    public int BestScore { get; set; } // 最高得分（0~1000000）
                    public DateTime BestScorePlayTime { get; set; } // 最佳成绩对应的游玩时间（进入结算页时时间）
                    public float BestImpurityRate { get; set; } // 最佳成绩对应的杂率（ms）*
                    public ScoreGrade BestGrade { get; set; } // 最佳成绩对应的评级 *
                    public int BestMaxCombo { get; set; } // 最佳成绩对应的最大连击数 *

                    public int LastScore { get; set; } // 末次游玩的得分（0~1000000）
                    public DateTime LastPlayTime { get; set; } // 末次游玩时间（进入结算页时时间）
                    public float LastImpurityRate { get; set; } // 末次杂率（ms）
                    public ScoreGrade LastGrade { get; set; } // 末次评级
                    public int LastMaxCombo { get; set; } // 末次最大连击数
                    public int LastEarlyNum { get; set; } // 末次 Early 数
                    public int LastLateNum { get; set; } // 末次 Late 数
                    public float LastAve { get; set; } // 末次平均误差时间（ms）
                    public int LastExactNum { get; set; } // 末次游玩的 Exact 数
                    public int LastGreatNum { get; set; }
                    public int LastRightNum { get; set; }
                    public int LastOutNum { get; set; }
                    public int LastBadAndMissNum { get; set; }
                }
            }
        }
    }
}
