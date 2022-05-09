﻿using System;
using System.Collections.Generic;
using CyanStars.Framework;
using CyanStars.Framework.Timeline;
using CyanStars.Gameplay.Data;
using CyanStars.Gameplay.Evaluate;
using CyanStars.Gameplay.Event;
using CyanStars.Gameplay.Input;
using CyanStars.Gameplay.Procedure;

namespace CyanStars.Gameplay
{
    /// <summary>
    /// 音游数据模块
    /// </summary>
    public class MusicGameModule : BaseDataModule
    {
        
        /// <summary>
        /// 音游数据文件名
        /// </summary>
        public string MusicGameDataName { get; set; }
        
        /// <summary>
        /// 时间轴当前时间
        /// </summary>
        public Timeline timeline{ get; set; }
        
        /// <summary>
        /// 输入映射数据文件名
        /// </summary>
        public string InputMapDataName { get; private set;}

        public string TapPrefabName { get; private set; }
        public string HoldPrefabName { get; private set; }
        public string DragPrefabName { get; private set; }
        public string ClickPrefabName { get; private set; }
        public string BreakPrefabName { get; private set; }

        /// <summary>
        /// 特效预制体名称列表
        /// </summary>
        public List<string> EffectNames { get; private set; }

        /// <summary>
        /// 是否为自动模式
        /// </summary>
        public bool IsAutoMode { get; set; }
        

        
        #region 玩家游戏过程中的实时数据
        
        public int Combo;//Combo数量
        public float Score = 0; //分数
        public EvaluateType Grade; //评分
        public float CurrentDeviation = 0; //当前精准度
        public List<float> DeviationList = new List<float>(); //各个音符的偏移
        public float MaxScore = 0; //理论最高分
        public int ExcatNum = 0;
        public int GreatNum = 0;
        public int RightNum = 0;
        public int BadNum = 0;
        public int MissNum = 0;
        public float FullScore; //全谱总分

        #endregion
        
       
        
        public override void OnInit()
        {
            InputMapDataName = "Assets/BundleRes/ScriptObjects/InputMapData.asset";
            
            TapPrefabName = "Assets/BundleRes/Prefabs/Notes/Tap.prefab";
            HoldPrefabName = "Assets/BundleRes/Prefabs/Notes/Hold.prefab";
            DragPrefabName = "Assets/BundleRes/Prefabs/Notes/Drag.prefab";
            ClickPrefabName = "Assets/BundleRes/Prefabs/Notes/Click.prefab";
            BreakPrefabName = "Assets/BundleRes/Prefabs/Notes/Break.prefab";

            EffectNames = new List<string>()
            {
                "Assets/BundleRes/Prefabs/Effect/VEG/MeteoriteEffect(VEG).prefab",
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
                EvaluateType.Exact => ExcatNum++,
                EvaluateType.Great => GreatNum++,
                EvaluateType.Right => RightNum++,
                EvaluateType.Out => RightNum++,
                EvaluateType.Bad => BadNum++,
                EvaluateType.Miss => MissNum++,
                _ => throw new System.NotImplementedException()
            };


            if (currentDeviation < 10000)
            {
                CurrentDeviation = currentDeviation;
                DeviationList.Add(currentDeviation);
            }
            
            //procedure.RefreshPlayingUI(Combo,Score,grade.ToString());
            GameRoot.Event.Dispatch(EventConst.MusicGameDataRefreshEvent,this,EventArgs.Empty);
        }
    }
}