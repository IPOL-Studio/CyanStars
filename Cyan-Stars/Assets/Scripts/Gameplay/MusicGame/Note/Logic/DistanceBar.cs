using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class DistanceBar
    {
        /// <summary>
        /// 每个条带占据的误差时间（单位ms）
        /// </summary>
        const int IntervalTime = 6;
        /// <summary>
        /// 每个条带初始&最小高度，高度样式待修改
        /// </summary>
        const float RawHeight = 10f;
        /// <summary>
        /// 每次判定增加这个高度，高度样式待修改
        /// </summary>
        const float AddF = 5f;
        /// <summary>
        /// 每帧都逐渐减少高度，每秒钟总共减少这个高度，高度样式待修改
        /// </summary>
        const float ReduceF = 5f;

        /// <summary>
        /// 左右边界代表最大的时间范围（单位ms）
        /// </summary>
        float rangeTime;
        /// <summary>
        /// 用数组表示条带组中每个条带对应的高度
        /// </summary>
        public float[] BarsHeight;



        /// <summary>
        /// 根据判定区间创建条带
        /// </summary>
        /// <param name="evaluateRange">当前判定区间</param>
        public void CreateBar(EvaluateRange evaluateRange)
        {
            // 以 Bad 或 Right 两者中绝对值较小的一个作为边界
            rangeTime = Mathf.Min(Mathf.Abs(evaluateRange.Bad), Mathf.Abs(evaluateRange.Right)) * 1000;
            Debug.Log($"判定边界：±{rangeTime * 1000}ms");

            // 创建条带，条带总数必定为奇数
            int barsNum = Mathf.CeilToInt(rangeTime * 1000 / IntervalTime) * 2 - 1;
            BarsHeight = new float[barsNum];
            Debug.Log($"根据分割时间{IntervalTime}，创建了{barsNum}个条带");
        }

        /// <summary>
        /// 根据传入的误差时间，增加对应条带的高度
        /// </summary>
        /// <param name="distanceTime">误差时间（单位s）</param>
        public void AddHeight(float distanceTime)
        {
            int targetBarNum;
            distanceTime *= 1000;   //将s转换为ms
            if (distanceTime <= -rangeTime)
            {
                // 超出下界时选择最左边的一条条带
                targetBarNum = 0;
            }
            else if (distanceTime >= rangeTime)
            {
                // 超出上界时选择最右边的一条条带
                targetBarNum = BarsHeight.Length - 1;
            }
            else
            {
                int centerIndex = Mathf.FloorToInt(BarsHeight.Length / 2);  // 这个Index位于数组&屏幕中央

                int index = centerIndex + ((int)distanceTime - IntervalTime / 2) / IntervalTime;

                BarsHeight[index] += AddF;
            }
        }

        /// <summary>
        /// 根据时间回落条带高度
        /// </summary>
        /// <param name="deltaTime">经过的时间</param>
        public void ReduceHeight(float deltaTime)
        {
            for (int i = 0; i < BarsHeight.Length; i++)
            {
                if (BarsHeight[i] - RawHeight < 0.001f)
                    continue;   // 差距够小时认为相等

                BarsHeight[i] -= ReduceF * deltaTime;
                BarsHeight[i] = Mathf.Max(BarsHeight[i], RawHeight);    // BarsHeight[i] 必须大于 RawHeight
            }
        }
    }
}
