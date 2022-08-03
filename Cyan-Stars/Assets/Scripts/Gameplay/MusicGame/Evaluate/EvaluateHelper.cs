using CyanStars.Framework;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public static class EvaluateHelper
    {
        private static readonly EvaluateModule DataModule = GameRoot.GetDataModule<EvaluateModule>();

        /// <summary>
        /// 输入时间和判定时间的距离差大于此值，就不处理输入
        /// </summary>
        public const float CheckInputStartDistance = 0.201f;

        /// <summary>
        /// Drag音符的判定时间距离范围
        /// </summary>
        public const float DragJudgeDistanceRange = 0.1f;

        /// <summary>
        /// Tap Hold Click Tap音符逻辑层时间和判定时间的距离小于此值 就自动Miss
        /// </summary>
        public static float CheckInputEndDistance => DataModule.Current.Miss;

        /// <summary>
        /// 根据Tap音符命中时间和判定时间的距离获取评价类型
        /// </summary>
        public static EvaluateType GetTapEvaluate(float distance)
        {
            float d = Mathf.Abs(distance);
            var c = DataModule.Current;

            if (d <= c.Exact)
            {
                return EvaluateType.Exact;
            }

            if (d <= c.Great)
            {
                return EvaluateType.Great;
            }

            if (distance <= c.Bad && distance >= 0)
            {
                return EvaluateType.Bad;
            }

            if (distance >= c.Right)
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
            if (value <= 0.125f)
            {
                return EvaluateType.Exact;
            }

            return EvaluateType.Out;
        }

        /// <summary>
        /// //由评价获取分数倍率
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
    }
}
