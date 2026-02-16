using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Chart
{
    /// <summary>
    /// 加载到游戏内的速度组
    /// </summary>
    public class SpeedTemplate
    {
        /// <summary>
        /// 从曲线转为离散速度时，采样时间间隔（ms）
        /// </summary>
        private const int SampleInterval = 10;

        /// <summary>
        /// 缓存的速度列表
        /// </summary>
        /// <remarks>列表从 0 时间差开始，每隔 SampleInterval 个时间向前采样，计算提前一段时间的速度</remarks>
        private List<float> speedList = new List<float>();

        /// <summary>
        /// 缓存的距离（视图层时间）列表
        /// </summary>
        /// <remarks>列表从 0 时间差开始，每隔 SampleInterval 个时间向前采样，计算提前一段时间的距离</remarks>
        private List<float> distanceList = new List<float>();


        /// <summary>
        /// 构造实例并烘焙速度和距离采样点
        /// </summary>
        public SpeedTemplate(SpeedTemplateData speedTemplateData, float playerSpeed = 1f) // TODO: 外部传入玩家速度
        {
            if (speedTemplateData.Type == SpeedGroupType.Absolute)
            {
                playerSpeed = 1f;
            }

            // 根据持续时间计算采样点数量
            int length = Mathf.Abs(speedTemplateData.BezierCurve[^1].PositionPoint.MsTime);
            int count = length / SampleInterval + 1;

            // 烘焙 speedList
            for (int i = 0; i < count; i++)
            {
                int msTime = i * SampleInterval;
                float speed = speedTemplateData.BezierCurve.GetValue(msTime) * playerSpeed;
                speedList.Add(speed);
            }

            // 烘焙 distanceList
            distanceList.Add(0f); // i=0 对应 logicTimeDistance=0 时 distance=0
            float sumDistance = 0f;
            for (int i = 1; i < count; i++)
            {
                sumDistance += (speedList[i - 1] + speedList[i]) * SampleInterval / 1000f; // 取相邻两个速度采样的平均值乘以时间来计算路程
                distanceList.Add(sumDistance);
            }
        }

        /// <summary>
        /// 根据逻辑时间差值获取速度
        /// </summary>
        /// <remarks>当前时间提前于音符判定时间时 logicTimeDistance 为负值</remarks>
        public float GetSpeed(float logicTimeDistance)
        {
            if (logicTimeDistance > 0)
            {
                return speedList[0];
            }

            float absTime = -logicTimeDistance;
            if (absTime >= (speedList.Count - 1) * SampleInterval)
            {
                return speedList[speedList.Count - 1];
            }

            int index = (int)(absTime / SampleInterval);
            return speedList[index];
        }

        /// <summary>
        /// 根据逻辑时间差值获取距离（视图层时间差值）
        /// </summary>
        /// <remarks>当前时间提前于音符判定时间时 logicTimeDistance 为负值</remarks>
        public float GetDistance(float logicTimeDistance)
        {
            if (logicTimeDistance > 0)
            {
                return speedList[0] * logicTimeDistance / 1000f * -1;
            }

            float absTime = -logicTimeDistance;

            // 处理超过最远采样时间的情况
            if (absTime >= (distanceList.Count - 1) * SampleInterval)
            {
                int lastIndex = distanceList.Count - 1;
                float timeExceeded = absTime - (lastIndex * SampleInterval);
                return distanceList[lastIndex] + speedList[lastIndex] * timeExceeded / 1000f;
            }

            int index = (int)(absTime / SampleInterval);
            float remainder = absTime % SampleInterval;

            if (remainder == 0)
            {
                return distanceList[index];
            }
            else
            {
                // 线性插值处理余数时间
                int nextIndex = index + 1;
                if (nextIndex >= distanceList.Count)
                {
                    return distanceList[index];
                }

                float baseDistance = distanceList[index];
                float partialDistance = speedList[nextIndex] * remainder / 1000f;
                return baseDistance + partialDistance;
            }
        }
    }
}
