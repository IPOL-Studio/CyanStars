namespace CyanStars.Gameplay.Evaluate
{
    public static class EvaluateHelper
    {
        /// <summary>
        /// 输入时间点大于这个时间 不处理输入
        /// </summary>
        public const float CheckInputStartTime = 0.201f;

        /// <summary>
        /// Tap Hold Click Tap音符倒计时小于这个时间 就自动Miss
        /// </summary>
        public const float CheckInputEndTime = -0.231f;

        /// <summary>
        /// Drag音符的判定时间范围
        /// </summary>
        public const float DragTimeRange = 0.1f;

        /// <summary>
        /// Click音符的判定时间范围
        /// </summary>
        public const float ClickTimeRange = 0.03f;

        /// <summary>
        /// 根据Tap音符命中时间获取评价类型
        /// </summary>
        public static EvaluateType GetTapEvaluate(float hitTime)
        {
            //80
            if (hitTime <= 0.08f && hitTime >= -0.08f)
            {
                return EvaluateType.Exact;
            }

            //81-140
            if (hitTime <= 0.14f && hitTime >= -0.14f)
            {
                return EvaluateType.Great;
            }

            //141-200（早）
            if (hitTime <= 0.2f && hitTime >= 0)
            {
                return EvaluateType.Bad;
            }

            //141-230（晚）
            if (hitTime >= -0.23f)
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
            if (value <= 0.07f)
            {
                return EvaluateType.Exact;
            }

            return EvaluateType.Out;
        }

        public static float GetScoreWithEvaluate(EvaluateType et)//由评价获取分数倍率
        {
            return et switch
            {
                EvaluateType.Exact => 1,
                EvaluateType.Great => 0.75f,
                EvaluateType.Right => 0.5f,
                EvaluateType.Out => 0.5f,
                EvaluateType.Bad => 0,
                EvaluateType.Miss => 0,
                _ => throw new System.NotImplementedException()
            };
        }
    }
}