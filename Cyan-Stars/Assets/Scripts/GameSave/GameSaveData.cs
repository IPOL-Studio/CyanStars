using System;
using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.Gameplay.MusicGame;

namespace CyanStars.GameSave
{
    /// <summary>
    /// 玩家存档结构
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
        /// 音游成绩类
        /// </summary>
        [Serializable]
        public class MusicGameSaveData
        {
            /// <summary>
            /// string 为谱包哈希，此值在谱师导出谱面时固定
            /// </summary>
            public Dictionary<string, ChartPackSaveData> ChartPackSaveDatas { get; set; }

            [Serializable]
            public class ChartPackSaveData
            {
                public List<ChartSaveData> ChartSaveDatas { get; set; }

                /// <summary>
                /// 每个谱面的成绩。
                /// 内置谱和社区谱通用的结构，如果找不到谱面但有分数，仍然记录分数，导入谱面后即可正常展示分数。
                /// </summary>
                [Serializable]
                public class ChartSaveData
                {
                    public ChartDifficulty ChartDifficulty { get; set; } // 谱面难度

                    public int PlayCount { get; set; } // 玩家累计游玩此谱面的次数

                    public DateTime FirstPlayTime { get; set; } // 首次完成游玩的时间（进入结算页时时间）

                    public DateTime BestScorePlayTime { get; set; } // 最佳成绩对应的游玩时间（进入结算页时时间）
                    public MusicGamePlayData BestPlayData { get; set; } // 最佳成绩时的游玩数据

                    public DateTime LastPlayTime { get; set; } // 末次游玩时间（进入结算页时时间）
                    public MusicGamePlayData LastPlayData { get; set; } // 末次游玩时的游玩数据
                }
            }
        }
    }
}
