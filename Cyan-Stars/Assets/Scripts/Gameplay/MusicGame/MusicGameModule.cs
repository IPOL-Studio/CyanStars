using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CyanStars.Framework;
using CyanStars.Framework.Event;
using CyanStars.Framework.Timeline;
using CyanStars.Framework.Logging;
using UnityEngine;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音游数据模块
    /// </summary>
    public class MusicGameModule : BaseDataModule
    {
        /// <summary>
        /// 谱包清单列表
        /// </summary>
        private List<ChartPack> chartPackManifests = new List<ChartPack>();

        /// <summary>
        /// 当前时间轴长度
        /// </summary>
        public float CurTimelineLength { get; set; }

        /// <summary>
        /// 是否为自动模式
        /// </summary>
        public bool IsAutoMode { get; set; }

        /// <summary>
        /// 谱包序号
        /// </summary>
        public int MapIndex { get; set; } = 0;

        /// <summary>
        /// 谱面难度
        /// </summary>
        public ChartDifficulty ChartDifficulty { get; set; }

        /// <summary>
        /// 当前运行中的时间轴
        /// </summary>
        public Timeline RunningTimeline { get; set; }

        /// <summary>
        /// 输入映射数据文件名
        /// </summary>
        public string InputMapDataName { get; private set; }

        /// <summary>
        /// 内置谱包列表文件名
        /// </summary>
        public string InternalChartListName { get; private set; }

        /// <summary>
        /// 音符类型 -> 音符预制体名称
        /// </summary>
        public Dictionary<NoteType, string> NotePrefabNameDict { get; private set; }

        /// <summary>
        /// 音符类型 -> 音符打击特效预制体名称
        /// </summary>
        public Dictionary<NoteType, string> HitEffectPrefabNameDict { get; private set; }

        /// <summary>
        /// 特效预制体名称列表
        /// </summary>
        public List<string> EffectNames { get; private set; }

        private string loggerCategoryName;
        public ICysLogger Logger { get; private set; }

        public DistanceBarData DistanceBarData { get; private set; }

        /// <summary>
        /// 用于计算杂率
        /// </summary>
        private float deviationsSum;

        public MusicGamePlayData MusicGamePlayData = new MusicGamePlayData { DeviationList = new List<float>() };


        public override void OnInit()
        {
            InputMapDataName = "Assets/BundleRes/ScriptObjects/InputMap/InputMapData.asset";
            InternalChartListName = "Assets/BundleRes/ScriptObjects/InternalChart/InternalChartPathList.asset";

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

        /// <summary>
        /// 加载内置谱面
        /// </summary>
        /// <returns></returns>
        public async Task LoadInternalMaps()
        {
            InternalChartPathListSO internalChartPathListSO =
                await GameRoot.Asset.LoadAssetAsync<InternalChartPathListSO>(InternalChartListName);
            GameRoot.Asset.UnloadAsset(internalChartPathListSO);

            foreach (string chartPath in internalChartPathListSO.InternalChartPaths)
            {
                if (!JsonUtility.JsonUtility.FromJson<ChartPackData>(chartPath, out ChartPackData chartPackData))
                {
                    Logger.LogError($"未能加载谱包数据：{chartPath}");
                    continue;
                }
                ChartPack chartPack = new ChartPack()
                {
                    IsInternal = true,
                    ChartPackData = chartPackData,
                    Music = await GameRoot.Asset.LoadAssetAsync<AudioClip>(chartPackData.MusicFilePath)
                };
                chartPackManifests.Add(chartPack);

                GameRoot.Asset.UnloadAsset(chartPackData.MusicFilePath);
            }
        }

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
        /// 获取谱面清单
        /// </summary>
        public ChartPack GetChartPack(int index)
        {
            return chartPackManifests[index];
        }

        /// <summary>
        /// 获取所有谱包清单
        /// </summary>
        /// <returns></returns>
        public List<ChartPack> GetChartPacks()
        {
            return chartPackManifests;
        }

        /// <summary>
        /// 计算全谱总分
        /// </summary>
        public void CalFullScore(ChartData chartData)
        {
            MusicGamePlayData.FullScore = 0;
            foreach (BaseChartNote note in chartData.Notes)
            {
                switch (note.Type)
                {
                    case NoteType.Tap:
                        MusicGamePlayData.FullScore += 1;
                        break;
                    case NoteType.Drag:
                        MusicGamePlayData.FullScore += 0.25f;
                        break;
                    case NoteType.Hold:
                        MusicGamePlayData.FullScore += 2f;
                        break;
                    case NoteType.Click:
                        MusicGamePlayData.FullScore += 2f;
                        break;
                    case NoteType.Break:
                        MusicGamePlayData.FullScore += 2f;
                        break;
                }
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
            // 玩家提前按下为+，延后按下为-
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
