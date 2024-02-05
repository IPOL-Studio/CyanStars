using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CyanStars.Framework;
using CyanStars.Framework.Asset;
using CyanStars.Framework.Event;
using CyanStars.Framework.Timeline;
using CyanStars.Framework.Logging;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音游数据模块
    /// </summary>
    public class MusicGameModule : BaseDataModule
    {
        /// <summary>
        /// 谱面清单列表
        /// </summary>
        private List<MapManifest> mapManifests;

        /// <summary>
        /// 当前时间轴长度
        /// </summary>
        public float CurTimelineLength { get; set; }

        /// <summary>
        /// 是否为自动模式
        /// </summary>
        public bool IsAutoMode { get; set; }

        /// <summary>
        /// 谱面序号
        /// </summary>
        public int MapIndex { get; set; } = 0;

        /// <summary>
        /// 当前运行中的时间轴
        /// </summary>
        public Timeline RunningTimeline { get; set; }

        /// <summary>
        /// 输入映射数据文件名
        /// </summary>
        public string InputMapDataName { get; private set; }

        /// <summary>
        /// 内置谱面列表文件名
        /// </summary>
        public string InternalMapListName { get; private set; }

        /// <summary>
        /// 音符类型 -> 音符预制体名称
        /// </summary>
        public Dictionary<NoteType,string> NotePrefabNameDict{ get; private set; }

        /// <summary>
        /// 音符类型 -> 音符打击特效预制体名称
        /// </summary>
        public Dictionary<NoteType,string> HitEffectPrefabNameDict{ get; private set; }

        /// <summary>
        /// 特效预制体名称列表
        /// </summary>
        public List<string> EffectNames { get; private set; }

        private string loggerCategoryName;
        public ICysLogger Logger { get; private set; }


#region 玩家游戏过程中的实时数据

        public int Combo = 0; //Combo数量
        public float Score = 0; //分数
        public EvaluateType Grade = default; //评分
        public float CurrentDeviation = 0; //当前精准度
        public List<float> DeviationList = new List<float>(); //各个音符的偏移
        public float MaxScore = 0; //理论最高分
        public int ExactNum = 0;
        public int GreatNum = 0;
        public int RightNum = 0;
        public int BadNum = 0;
        public int MissNum = 0;
        public float FullScore = 0; //全谱总分

#endregion


        public override void OnInit()
        {
            InputMapDataName = "Assets/BundleRes/ScriptObjects/InputMap/InputMapData.asset";
            InternalMapListName = "Assets/BundleRes/ScriptObjects/InternalMap/InternalMapList.asset";

            NotePrefabNameDict = new Dictionary<NoteType, string>()
            {
                {NoteType.Tap,"Assets/BundleRes/Prefabs/Notes/Tap.prefab"},
                {NoteType.Hold,"Assets/BundleRes/Prefabs/Notes/Hold.prefab"},
                {NoteType.Drag,"Assets/BundleRes/Prefabs/Notes/Drag.prefab"},
                {NoteType.Click,"Assets/BundleRes/Prefabs/Notes/Click.prefab"},
                {NoteType.Break,"Assets/BundleRes/Prefabs/Notes/Break.prefab"},
            };

            HitEffectPrefabNameDict = new Dictionary<NoteType, string>()
            {
                {NoteType.Tap,"Assets/BundleRes/Prefabs/Effect/TapHitEffect.prefab"},
                {NoteType.Hold,"Assets/BundleRes/Prefabs/Effect/HoldHitEffect.prefab"},
                {NoteType.Drag,"Assets/BundleRes/Prefabs/Effect/DragHitEffect.prefab"},
                {NoteType.Click,"Assets/BundleRes/Prefabs/Effect/ClickHitEffect.prefab"},
                {NoteType.Break,"Assets/BundleRes/Prefabs/Effect/BreakHitEffect.prefab"},
            };

            EffectNames = new List<string>()
            {
                //"Assets/BundleRes/Prefabs/Effect/VEG/MeteoriteEffect(VEG).prefab",  //640w面大烟花
                "Assets/BundleRes/Prefabs/Effect/VEG/FireworkEffect(VEG).prefab", //先用另一个代替

                "Assets/BundleRes/Prefabs/Effect/VEG/Fiery_trees_and_silver_flowers_Effect(VEG).prefab",
                "Assets/BundleRes/Prefabs/Effect/VEG/FireworkEffect(VEG).prefab",
                "Assets/BundleRes/Prefabs/Effect/VEG/GalaxyEffect(VEG).prefab",
                "Assets/BundleRes/Prefabs/Effect/VEG/TriangleEffect(VEG).prefab",
                "Assets/BundleRes/Prefabs/Effect/VEG/VortexEffect(VEG).prefab",
                "Assets/BundleRes/Prefabs/Effect/VEG/ParticleGushingEffect(VEG).prefab",
                "Assets/BundleRes/Prefabs/Effect/VEG/BlockEffect(VEG).prefab",
                "Assets/BundleRes/Prefabs/Effect/VEG/LineEffect(VEG).prefab",
                "Assets/BundleRes/Prefabs/Effect/VEG/BlockRainEffect(VEG).prefab",
                "Assets/BundleRes/Prefabs/Effect/VEG/CircleFireEffect(VEG).prefab",
                "Assets/BundleRes/Prefabs/Effect/VEG/SpaceJumpEffect(VEG).prefab"
            };
        }

        /// <summary>
        /// 加载内置谱面
        /// </summary>
        /// <returns></returns>
        public async Task LoadInternalMaps()
        {
            InternalMapListSO internalMapListSo =
                await GameRoot.Asset.LoadAssetAsync<InternalMapListSO>(InternalMapListName);
            mapManifests = internalMapListSo.InternalMaps;
            GameRoot.Asset.UnloadAsset(internalMapListSo);
        }

        public void InitLogger(string categoryName)
        {
            loggerCategoryName = categoryName;
            Logger = GameRoot.Logger.GetOrCreateLogger(loggerCategoryName);
        }

        /// <summary>
        /// 获取谱面清单
        /// </summary>
        public MapManifest GetMap(int index)
        {
            return mapManifests[index];
        }

        /// <summary>
        /// 获取所有谱面清单
        /// </summary>
        /// <returns></returns>
        public List<MapManifest> GetMaps()
        {
            return mapManifests;
        }

        /// <summary>
        /// 计算全谱总分
        /// </summary>
        public void CalFullScore(NoteTrackData noteTrackData)
        {
            FullScore = 0;
            foreach (var layer in noteTrackData.LayerDatas)
            {
                foreach (var timeAxis in layer.TimeAxisDatas)
                {
                    foreach (var note in timeAxis.NoteDatas)
                    {
                        FullScore += note.GetFullScore();
                    }
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

            Combo = 0; //Combo数量
            Score = 0; //分数
            Grade = default; //评分
            CurrentDeviation = 0; //当前精准度
            DeviationList.Clear(); //各个音符的偏移
            MaxScore = 0; //理论最高分
            ExactNum = 0;
            GreatNum = 0;
            RightNum = 0;
            BadNum = 0;
            MissNum = 0;
            FullScore = 0; //全谱总分
        }

        /// <summary>
        /// 刷新玩家游戏中的数据
        /// </summary>
        public void RefreshPlayingData(int addCombo, float addScore, EvaluateType grade, float currentDeviation)
        {
            if (addCombo < 0)
            {
                Combo = 0;
            }
            else
            {
                Combo += addCombo;
                Score += addScore;
            }

            Grade = grade;

            _ = grade switch
            {
                EvaluateType.Exact => ExactNum++,
                EvaluateType.Great => GreatNum++,
                EvaluateType.Right => RightNum++,
                EvaluateType.Out => RightNum++,
                EvaluateType.Bad => BadNum++,
                EvaluateType.Miss => MissNum++,
                _ => throw new ArgumentException(nameof(grade))
            };


            if (currentDeviation < 10000)
            {
                CurrentDeviation = currentDeviation;
                DeviationList.Add(currentDeviation);
            }

            GameRoot.Event.Dispatch(EventConst.MusicGameDataRefreshEvent, this,EmptyEventArgs.Create());
        }
    }
}
