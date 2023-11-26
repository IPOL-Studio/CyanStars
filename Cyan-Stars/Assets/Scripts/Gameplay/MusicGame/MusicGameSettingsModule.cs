using System;
using System.Collections.Generic;
using CyanStars.Framework;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class MusicGameSettingsModule : BaseDataModule
    {
        private static class EvaluateRanges
        {
            public static readonly EvaluateRange Normal;
            public static readonly EvaluateRange Hard;
            public static readonly EvaluateRange Easy;

            static EvaluateRanges()
            {
                Normal = new EvaluateRange(0.08f, 0.14f, 0.2f, -0.23f);
                Hard = new EvaluateRange(0.04f, 0.1f, 0.2f, -0.16f);
                Easy = new EvaluateRange(0.12f, 0.18f, 0.2f, -0.23f);
            }
        }

        private EvaluateMode evaluateMode;
        /// <summary>
        /// 当前判定模式
        /// </summary>
        public EvaluateMode EvaluateMode
        {
            get => evaluateMode;
            set
            {
                if (evaluateMode != value)
                {
                    UpdateEvaluateMode(value);
                }
            }
        }

        /// <summary>
        /// 当前判定模式的判定范围
        /// </summary>
        public EvaluateRange EvaluateRange { get; private set; }


        private bool enableCameraTrack = true;
        /// <summary>
        /// 是否启用摄像机Track
        /// </summary>
        public bool EnableCameraTrack
        {
            get
            {
#if UNITY_ANDROID || UNITY_IOS
                return false;  // 移动平台总是禁用 Camera 动效，避免 Camera 乱动导致无法交互
#else
                return enableCameraTrack;
#endif
            }
            set => enableCameraTrack = value;
        }

        /// <summary>
        /// 是否启用歌词Track
        /// </summary>
        public bool EnableLyricTrack { get; set; } = true;

        /// <summary>
        /// 是否启用特效Track
        /// </summary>
        public bool EnableEffectTrack { get; set; } = true;

        /// <summary>
        /// 要使用的默认打击音效
        /// <para>prompt name -> prompt asset path</para>
        /// </summary>
        public IReadOnlyDictionary<string, string> BuiltInPromptTones { get; private set; }

        public override void OnInit()
        {
            UpdateEvaluateMode(EvaluateMode.Normal);

            BuiltInPromptTones = new Dictionary<string, string>()
            {
                { "NsKa", "Assets/BundleRes/Audio/PromptTone/ns_ka.ogg" },
                { "NsDing", "Assets/BundleRes/Audio/PromptTone/ns_ding.ogg" },
                { "NsTambourine", "Assets/BundleRes/Audio/PromptTone/ns_tambourine.ogg" }
            };
        }

        private void UpdateEvaluateMode(EvaluateMode mode)
        {
            evaluateMode = mode;
            EvaluateRange = mode switch
            {
                EvaluateMode.Easy => EvaluateRanges.Easy,
                EvaluateMode.Normal => EvaluateRanges.Normal,
                EvaluateMode.Hard => EvaluateRanges.Hard,
                _ => throw new ArgumentOutOfRangeException()
            };

            Debug.Log($"Update evaluate mode succeed, current evaluate range: {EvaluateRange}");
        }
    }
}
