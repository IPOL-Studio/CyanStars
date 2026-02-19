#nullable enable

using System;
using System.Collections.Generic;
using CyanStars.Chart;

namespace CyanStars.Utils.SpeedTemplate
{
    public static class SpeedTemplateHelper
    {
        /// <summary>
        /// 在游戏和制谱器内烘焙变速模板时的采样时间
        /// </summary>
        /// <remarks>为游戏和制谱器采用相同的采样时间，以确保游戏效果和制谱器内预览一致</remarks>
        public const int SampleIntervalMsTime = 1;

        /// <summary>
        /// 完整烘焙速度-时间和路程-时间列表
        /// </summary>
        public static void Bake(SpeedTemplateData speedTemplateData, float playerSpeed, out List<float> speedList, out List<float> distanceList)
        {
            // count = dataMaxMsTime / SampleIntervalMsTime 并向上取整
            int dataMaxMsTime = speedTemplateData.BezierCurves[^1].PositionPoint.MsTime;
            int count = (dataMaxMsTime + SampleIntervalMsTime - 1) / SampleIntervalMsTime;

            BakeByCount(speedTemplateData, playerSpeed, count, out speedList, out distanceList);
        }

        /// <summary>
        /// 在指定的时间前部分烘焙速度-时间和路程-时间列表
        /// </summary>
        /// <remarks>给定时间超过 data 最大时间时，将以 data 最大时间完整烘焙</remarks>
        public static void Bake(SpeedTemplateData speedTemplateData, float playerSpeed, int stopAfterMs, out List<float> speedList, out List<float> distanceList)
        {
            int dataMaxMsTime = speedTemplateData.BezierCurves[^1].PositionPoint.MsTime;
            int time = Math.Min(dataMaxMsTime, stopAfterMs);
            int count = (time + SampleIntervalMsTime - 1) / SampleIntervalMsTime; // time 与 SampleIntervalMsTime 相除后向上取整

            BakeByCount(speedTemplateData, playerSpeed, count, out speedList, out distanceList);
        }

        /// <summary>
        /// 根据采样点数量烘焙
        /// </summary>
        private static void BakeByCount(SpeedTemplateData speedTemplateData, float playerSpeed, int count, out List<float> speedList, out List<float> distanceList)
        {
            speedList = new List<float>();
            distanceList = new List<float>();

            if (speedTemplateData.Type == SpeedGroupType.Absolute)
            {
                playerSpeed = 1f;
            }

            // 烘焙 speedList
            for (int i = 0; i < count; i++)
            {
                int msTime = i * SampleIntervalMsTime;
                float speed = speedTemplateData.BezierCurves.EvaluateValue(msTime) * playerSpeed;
                speedList.Add(speed);
            }

            // 烘焙 distanceList
            distanceList.Add(0f); // i=0 对应 logicTimeDistance=0 时 distance=0
            float sumDistance = 0f;
            for (int i = 1; i < count; i++)
            {
                sumDistance += (speedList[i - 1] + speedList[i]) / 2 * SampleIntervalMsTime / 1000f; // 取相邻两个速度采样的平均值乘以时间来计算路程
                distanceList.Add(sumDistance);
            }
        }
    }
}
