using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CyanStars.Framework;
using CyanStars.Framework.Event;
using CyanStars.Framework.Timeline;
using CyanStars.Framework.Logging;
using CyanStars.Chart;
using UnityEngine;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音游数据模块
    /// </summary>
    public class MusicGamePlayingDataModule : BaseDataModule
    {
        // --- --- 谱包和谱面 --- ---

        /// <summary>
        /// 选中的谱包序号
        /// </summary>
        public int ChartPackIndex { get; set; }

        /// <summary>
        /// 选中的谱面难度
        /// </summary>
        public ChartDifficulty Difficulty { get; set; }

        /// <summary>
        /// 选中的音乐版本序号
        /// </summary>
        public int MusicVersionIndex { get; set; } = 0;


        // --- --- 时间轴和游玩时数据 --- ---

        /// <summary>
        /// 当前运行中的时间轴
        /// </summary>
        public Timeline RunningTimeline { get; set; }

        /// <summary>
        /// 当前运行的时间轴的总长度（s）
        /// </summary>
        public float CurTimelineLength { get; set; }

        /// <summary>
        /// 是否为自动模式
        /// </summary>
        public bool IsAutoMode { get; set; }

        /// <summary>
        /// 误差指示条数据
        /// </summary>
        public DistanceBarData DistanceBarData { get; private set; }

        /// <summary>
        /// 用于计算杂率
        /// </summary>
        private float deviationsSum;

        /// <summary>
        /// 游玩时动态数据
        /// </summary>
        public MusicGamePlayData MusicGamePlayData = new MusicGamePlayData { DeviationList = new List<float>() };


        // --- --- 索引文件文件名 --- ---

        /// <summary>
        /// 输入映射数据文件名
        /// </summary>
        public string InputMapDataName { get; private set; }

        /// <summary>
        /// 内置谱面列表文件名
        /// </summary>
        public string InternalMapListName { get; private set; }

        /// <summary>
        /// 特效预制体名称列表
        /// </summary>
        public List<string> EffectNames { get; private set; }


        // --- --- 预制体映射 --- ---

        /// <summary>
        /// 音符类型 -> 音符预制体名称
        /// </summary>
        public Dictionary<NoteType, string> NotePrefabNameDict { get; private set; }

        /// <summary>
        /// 音符类型 -> 音符打击特效预制体名称
        /// </summary>
        public Dictionary<NoteType, string> HitEffectPrefabNameDict { get; private set; }


        // --- --- 日志 --- ---

        private string loggerCategoryName;

        public ICysLogger Logger { get; private set; }


        #region 从路径加载数据

        public override void OnInit()
        {
            InputMapDataName = "Assets/BundleRes/ScriptObjects/InputMap/InputMapData.asset";
            InternalMapListName = "Assets/BundleRes/ScriptObjects/InternalMap/InternalMapList.asset";

            NotePrefabNameDict = new Dictionary<NoteType, string>()
            {
                { NoteType.Tap, "Assets/BundleRes/NoteModelsV1.2/Tap.prefab" },
                { NoteType.Hold, "Assets/BundleRes/NoteModelsV1.2/Hold.prefab" },
                { NoteType.Drag, "Assets/BundleRes/NoteModelsV1.2/Drag.prefab" },
                { NoteType.Click, "Assets/BundleRes/NoteModelsV1.2/Click.prefab" },
                { NoteType.Break, "Assets/BundleRes/NoteModelsV1.2/Break.prefab" },
            };

            HitEffectPrefabNameDict = new Dictionary<NoteType, string>()
            {
                { NoteType.Tap, "Assets/BundleRes/Prefabs/Effect/TapHitEffect.prefab" },
                { NoteType.Hold, "Assets/BundleRes/Prefabs/Effect/HoldHitEffect.prefab" },
                { NoteType.Drag, "Assets/BundleRes/Prefabs/Effect/DragHitEffect.prefab" },
                { NoteType.Click, "Assets/BundleRes/Prefabs/Effect/ClickHitEffect.prefab" },
                { NoteType.Break, "Assets/BundleRes/Prefabs/Effect/BreakHitEffect.prefab" },
            };

            EffectNames = new List<string>()
            {
                //"Assets/CysMultimediaAssets/VEG/MeteoriteEffect(VEG).prefab",  //640w面大烟花
                "Assets/VEG/FireworkEffect(VEG).prefab", //先用另一个代替

                "Assets/VEG/Fiery_trees_and_silver_flowers_Effect(VEG).prefab",
                "Assets/VEG/FireworkEffect(VEG).prefab",
                "Assets/VEG/GalaxyEffect(VEG).prefab",
                "Assets/VEG/TriangleEffect(VEG).prefab",
                "Assets/VEG/VortexEffect(VEG).prefab",
                "Assets/VEG/ParticleGushingEffect(VEG).prefab",
                "Assets/VEG/BlockEffect(VEG).prefab",
                "Assets/VEG/LineEffect(VEG).prefab",
                "Assets/VEG/BlockRainEffect(VEG).prefab",
                "Assets/VEG/CircleFireEffect(VEG).prefab",
                "Assets/VEG/SpaceJumpEffect(VEG).prefab"
            };
        }

        #endregion


        public void InitLogger(string categoryName)
        {
            loggerCategoryName = categoryName;
            Logger = GameRoot.Logger.GetOrCreateLogger(loggerCategoryName);
        }

        public void InitDistanceBarData(EvaluateRange evaluateRange)
        {
            DistanceBarData = new DistanceBarData(evaluateRange);
        }

        /// <summary>
        /// 计算全谱总分
        /// </summary>
        public void CalFullScore(List<BaseChartNoteData> baseChartNoteDatas)
        {
            MusicGamePlayData.FullScore = 0;
            foreach (BaseChartNoteData noteData in baseChartNoteDatas)
            {
                MusicGamePlayData.FullScore += noteData.Type switch
                {
                    NoteType.Tap => 1,
                    NoteType.Hold => 2,
                    NoteType.Drag => 0.25f,
                    NoteType.Click => 2,
                    NoteType.Break => 2,
                    _ => throw new System.NotFiniteNumberException()
                };
            }
        }

        /// <summary>
        /// 重置玩家游戏中的数据
        /// </summary>
        public void ResetPlayingData()
        {
            RunningTimeline = null;
            Logger = null;
            GameRoot.Logger.RemoveLogger(loggerCategoryName);
            loggerCategoryName = null;
            DistanceBarData = null;
            deviationsSum = 0;

            MusicGamePlayData = new MusicGamePlayData { DeviationList = new List<float>() };
        }

        /// <summary>
        /// 刷新玩家游戏中的数据
        /// </summary>
        public void RefreshPlayingData(int addCombo, float addScore, EvaluateType grade, float currentDeviation)
        {
            if (addCombo < 0)
            {
                MusicGamePlayData.Combo = 0;
            }
            else
            {
                MusicGamePlayData.Combo += addCombo;
                MusicGamePlayData.MaxCombo = Mathf.Max(MusicGamePlayData.Combo, MusicGamePlayData.MaxCombo);
                MusicGamePlayData.Score += addScore;
            }

            MusicGamePlayData.Grade = grade;

            _ = grade switch
            {
                EvaluateType.Exact => MusicGamePlayData.ExactNum++,
                EvaluateType.Great => MusicGamePlayData.GreatNum++,
                EvaluateType.Right => MusicGamePlayData.RightNum++,
                EvaluateType.Out => MusicGamePlayData.OutNum++,
                EvaluateType.Bad => MusicGamePlayData.BadNum++,
                EvaluateType.Miss => MusicGamePlayData.MissNum++,
                _ => throw new ArgumentException(nameof(grade))
            };

            // 仅部分音符计算偏移值/杂率，详见 NoteJudger.cs 代码
            // currentDeviation 为玩家按下的时间相对于 Note 判定时间之差，单位 s
            // 玩家提前按下为-，延后按下为+
            if (currentDeviation < 10000)
            {
                MusicGamePlayData.CurrentDeviation = currentDeviation;
                MusicGamePlayData.DeviationList.Add(currentDeviation);
                deviationsSum += Mathf.Abs(currentDeviation);
                MusicGamePlayData.ImpurityRate = deviationsSum / MusicGamePlayData.DeviationList.Count;
                MusicGamePlayData.ImpurityRate =
                    (float)Mathf.CeilToInt(MusicGamePlayData.ImpurityRate * 1000000) / 1000; // 将杂率转换为 00.000ms 格式并向上取整
            }

            GameRoot.Event.Dispatch(EventConst.MusicGameDataRefreshEvent, this, EmptyEventArgs.Create());
        }
    }
}
