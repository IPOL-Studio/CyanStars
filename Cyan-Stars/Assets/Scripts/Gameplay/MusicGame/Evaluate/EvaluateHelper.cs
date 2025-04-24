using CyanStars.Framework;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public static class EvaluateHelper
    {
        private static readonly MusicGameSettingsModule DataModule = GameRoot.GetDataModule<MusicGameSettingsModule>();

        /// <summary>
        /// 输入时间和判定时间的距离差小于此值，就不处理输入
        /// </summary>
        public const float CheckInputStartDistance = -0.201f;

        /// <summary>
        /// Drag音符的判定时间距离范围
        /// </summary>
        public const float DragJudgeDistanceRange = 0.1f;

        /// <summary>
        /// 根据Tap音符命中时间和判定时间的距离获取评价类型
        /// </summary>
        public static EvaluateType GetTapEvaluate(float distance)
        {
            float d = Mathf.Abs(distance);
            var c = DataModule.EvaluateRange;

            if (d <= c.Exact)
            {
                return EvaluateType.Exact;
            }

            if (d <= c.Great)
            {
                return EvaluateType.Great;
            }

            if (distance >= c.Bad && distance <= 0)
            {
                return EvaluateType.Bad;
            }

            if (distance <= c.Right)
            {
                return EvaluateType.Right;
            }

            return EvaluateType.Miss;
        }

        /// <summary>
        /// 根据Hold音符命中比例获取评价类型
        /// </summary>
        public static EvaluateType GetHoldEvaluate(float value)
        {
            if (value >= 0.95f)
            {
                return EvaluateType.Exact;
            }

            if (value >= 0.85f)
            {
                return EvaluateType.Great;
            }

            if (value >= 0.75f)
            {
                return EvaluateType.Right;
            }

            return EvaluateType.Miss;
        }

        /// <summary>
        /// 根据Click音符命中时长获取评价类型
        /// </summary>
        public static EvaluateType GetClickEvaluate(float value)
        {
            return value <= 0.125f ? EvaluateType.Exact : EvaluateType.Out;
        }

        /// <summary>
        /// 由评价获取分数倍率
        /// </summary>
        public static float GetScoreWithEvaluate(EvaluateType et)
        {
            return et switch
            {
                EvaluateType.Exact => 1,
                EvaluateType.Great => 0.75f,
                EvaluateType.Right => 0.5f,
                EvaluateType.Out => 0.5f,
                EvaluateType.Bad => 0,
                EvaluateType.Miss => 0,
                _ => throw new System.ArgumentException(nameof(et))
            };
        }

        /// <summary>
        /// 音符是否应该判定为Miss
        /// </summary>
        /// <param name="distance">音符逻辑层时间和判定时间的距离</param>
        public static bool IsMiss(float distance)
        {
            return distance > DataModule.EvaluateRange.Right;
        }
    }
}
