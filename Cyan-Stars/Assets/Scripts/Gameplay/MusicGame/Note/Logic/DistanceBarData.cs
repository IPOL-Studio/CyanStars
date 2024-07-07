using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class DistanceBarData
    {
        /// <summary>
        /// 每个条带占据的误差时间（单位ms）
        /// </summary>
        private const int IntervalTime = 10;
        /// <summary>
        /// 每个条带初始&最小高度比例，高度样式待修改
        /// </summary>
        private const float MinHeight = 0.05f;
        /// <summary>
        /// 限定每个条带的最大高度比例，样式待修改
        /// </summary>
        private const float MaxHeight = 1f;
        /// <summary>
        /// 每次判定增加这个高度比例，高度样式待修改
        /// </summary>
        private const float AddF = 0.6f;
        /// <summary>
        /// 每帧都逐渐减少高度比例，每秒钟总共减少这个高度，高度样式待修改
        /// </summary>
        private const float ReduceF = 0.3f;

        /// <summary>
        /// 左右边界代表最大的时间范围（单位ms）
        /// </summary>
        private float rangeTime;
        /// <summary>
        /// 最中间一条条带的 Index
        /// </summary>
        private int centerIndex;
        /// <summary>
        /// 每个条带对应的高度，范围 0 - 1
        /// </summary>
        internal float[] BarHeights;

        public int BarDataChangedCount { get; private set; }


        public float this[int index] => BarHeights[index];
        public int Length => BarHeights.Length;


        /// <summary>
        /// 根据判定区间创建条带
        /// </summary>
        /// <param name="evaluateRange">当前判定区间</param>
        public DistanceBarData(EvaluateRange evaluateRange)
        {
            // 以 Bad 或 Right 两者中绝对值较小的一个作为边界
            rangeTime = Mathf.Min(Mathf.Abs(evaluateRange.Bad), Mathf.Abs(evaluateRange.Right)) * 1000;
            Debug.Log($"判定边界：±{rangeTime}ms");

            // 创建条带，条带总数必定为奇数
            int barsNum = Mathf.CeilToInt(rangeTime / IntervalTime) * 2 - 1;
            centerIndex = Mathf.FloorToInt(barsNum / 2);  // 这个Index位于数组&屏幕中央
            BarHeights = new float[barsNum];

            for (int i = 0; i < barsNum; i++)
            {
                // 初始化条带长度
                BarHeights[i] = MinHeight;
            }

            Debug.Log($"根据分割时间{IntervalTime}，创建了{barsNum}个条带");
        }

        /// <summary>
        /// 根据传入的误差时间，增加对应条带的高度
        /// </summary>
        /// <param name="distanceTime">误差时间（单位s）</param>
        public void AddHeight(float distanceTime)
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
                index = BarHeights.Length - 1;
            }
            else
            {
                index = centerIndex + ((int)distanceTime - IntervalTime / 2) / IntervalTime;
            }

            BarHeights[index] = Mathf.Min(BarHeights[index] + AddF, MaxHeight);
            BarDataChangedCount++;

            Debug.Log($"条带组长度更新：{BarHeights}");
        }

        public void AddHeightWithMiss()
        {
            int index;

            // miss 时选择最右边的一条条带
            index = BarHeights.Length - 1;

            BarHeights[index] = Mathf.Min(BarHeights[index] + AddF, MaxHeight);
            BarDataChangedCount++;

            Debug.Log($"条带组长度更新：{BarHeights}");
        }

        /// <summary>
        /// 根据时间回落条带高度
        /// </summary>
        /// <param name="deltaTime">经过的时间</param>
        public void ReduceHeight(float deltaTime)
        {
            if (deltaTime <= 0)
            {
                Debug.Log("delta time should be great or equal 0");
                return;
            }

            for (int i = 0; i < BarHeights.Length; i++)
            {
                if (BarHeights[i] - MinHeight < 0.001f)
                    continue;   // 差距够小时认为相等

                BarHeights[i] -= ReduceF * deltaTime;
                BarHeights[i] = Mathf.Max(BarHeights[i], MinHeight);    // BarsHeight[i] 必须大于 RawHeight
            }

            BarDataChangedCount++;
        }

        public bool IsDataChanged(int preChangedCount)
        {
            return preChangedCount < BarDataChangedCount;
        }
    }
}
