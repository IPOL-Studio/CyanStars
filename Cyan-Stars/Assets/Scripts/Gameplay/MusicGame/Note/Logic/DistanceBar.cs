using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public static class DistanceBar
    {
        /// <summary>
        /// 每个条带占据的误差时间（单位ms）
        /// </summary>
        private const int IntervalTime = 6;
        /// <summary>
        /// 每个条带初始&最小高度比例，高度样式待修改
        /// </summary>
        private const float MinHeight = 0f;
        /// <summary>
        /// 限定每个条带的最大高度比例，样式待修改
        /// </summary>
        private const float MaxHeighe = 1f;
        /// <summary>
        /// 每次判定增加这个高度比例，高度样式待修改
        /// </summary>
        private const float AddF = 0.25f;
        /// <summary>
        /// 每帧都逐渐减少高度比例，每秒钟总共减少这个高度，高度样式待修改
        /// </summary>
        private const float ReduceF = 0.1f;

        /// <summary>
        /// 左右边界代表最大的时间范围（单位ms）
        /// </summary>
        private static float rangeTime;
        /// <summary>
        /// 最中间一条条带的 Index
        /// </summary>
        private static int centerIndex;
        /// <summary>
        /// 用数组表示条带组中每个条带对应的高度
        /// </summary>
        public static float[] BarsHeight;


        /// <summary>
        /// 根据判定区间创建条带
        /// </summary>
        /// <param name="evaluateRange">当前判定区间</param>
        public static void CreateBar(EvaluateRange evaluateRange)
        {
            // 以 Bad 或 Right 两者中绝对值较小的一个作为边界
            rangeTime = Mathf.Min(Mathf.Abs(evaluateRange.Bad), Mathf.Abs(evaluateRange.Right)) * 1000;
            Debug.Log($"判定边界：±{rangeTime}ms");

            // 创建条带，条带总数必定为奇数
            int barsNum = Mathf.CeilToInt(rangeTime / IntervalTime) * 2 - 1;
            centerIndex = Mathf.FloorToInt(barsNum / 2);  // 这个Index位于数组&屏幕中央
            BarsHeight = new float[barsNum];

            for (int i = 0; i < barsNum; i++)
            {
                // 初始化条带长度
                BarsHeight[i] = MinHeight;
            }

            Debug.Log($"根据分割时间{IntervalTime}，创建了{barsNum}个条带");
        }

        /// <summary>
        /// 根据传入的误差时间，增加对应条带的高度
        /// </summary>
        /// <param name="distanceTime">误差时间（单位s）</param>
        public static void AddHeight(float distanceTime)
        {
            int index;
            distanceTime = -distanceTime;   // 这里是由于distanceTime正负与其他代码不一致引起的，ToFix https://github.com/IPOL-Studio/CyanStars/issues/231
            distanceTime *= 1000;   // 将s转换为ms
            if (distanceTime <= -rangeTime)
            {
                // 超出下界时选择最左边的一条条带
                index = 0;
            }
            else if (distanceTime >= rangeTime)
            {
                // 超出上界时选择最右边的一条条带
                index = BarsHeight.Length - 1;
            }
            else
            {
                index = centerIndex + ((int)distanceTime - IntervalTime / 2) / IntervalTime;
            }

            BarsHeight[index] = Mathf.Min(BarsHeight[index] + AddF, MaxHeighe);

            Debug.Log($"条带组长度更新：{BarsHeight}");
        }

        /// <summary>
        /// （方法重载）不传入误差时间，而作为Miss判定处理
        /// </summary>
        public static void AddHeight()
        {
            int index;

            // miss 时选择最右边的一条条带
            index = BarsHeight.Length - 1;

            BarsHeight[index] = Mathf.Min(BarsHeight[index] + AddF, MaxHeighe);

            Debug.Log($"条带组长度更新：{BarsHeight}");
        }

        /// <summary>
        /// 根据时间回落条带高度
        /// </summary>
        /// <param name="deltaTime">经过的时间</param>
        public static void ReduceHeight(float deltaTime)
        {
            for (int i = 0; i < BarsHeight.Length; i++)
            {
                if (BarsHeight[i] - MinHeight < 0.001f)
                    continue;   // 差距够小时认为相等

                BarsHeight[i] -= ReduceF * deltaTime;
                BarsHeight[i] = Mathf.Max(BarsHeight[i], MinHeight);    // BarsHeight[i] 必须大于 RawHeight
            }
        }
    }
}
