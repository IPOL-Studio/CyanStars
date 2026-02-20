#nullable enable

using System.Collections.Generic;
using CyanStars.Chart;

namespace CyanStars.Utils.SpeedTemplate
{
    public static class SpeedTemplateHelper
    {
        /// <summary>
        /// 在游戏和制谱器内烘焙变速模板时的采样时间
        /// </summary>
        /// <remarks>
        /// 为游戏和制谱器采用相同的采样时间，以确保游戏效果和制谱器内预览一致。
        /// 采样 Count 为 speedTemplateData.BezierCurves[^1].PositionPoint.MsTime/SampleIntervalMsTime，向下取整。
        /// 即：采样点一定落在曲线组范围内；曲线组最远端时间点一定不会参与采样，请直接用 [^1] 获取值。
        /// </remarks>
        public const int SampleIntervalMsTime = 10;


        /// <summary>
        /// 缓存的上次烘焙时的曲线，下次烘焙时可以跳过开头相等的部分来提高性能
        /// </summary>
        private static List<BezierPoint> tempedBezierPoints = new();

        /// <summary>
        /// 缓存的每个完整的贝塞尔曲线段所造成的总位移（即对贝塞尔曲线段积分后带入 t=1）
        /// </summary>
        private static List<double> tempedCurveFinalDisplacements = new();

        /// <summary>
        /// 缓存的速度采样点
        /// </summary>
        private static List<float> tempedSpeedList = new();

        /// <summary>
        /// 缓存的位移采样点
        /// </summary>
        private static List<float> tempedDisplacementList = new();


        /// <summary>
        /// 烘焙 速度-时间 和 位移-时间 列表
        /// </summary>
        public static void Bake(SpeedTemplateData speedTemplateData, float playerSpeed, out List<float> speedList, out List<float> distanceList)
        {
            speedList = new List<float>();
            distanceList = new List<float>();

            // 如果曲线组贝塞尔点小于等于 1 个，返回空列表。请直接用 [^1].Value 获取
            if (speedTemplateData.BezierCurves.Count <= 1)
            {
                return;
            }

            if (speedTemplateData.Type == SpeedGroupType.Absolute)
            {
                playerSpeed = 1;
            }

            int curveIndex = 0; // 当前即将进行处理的贝塞尔曲线段下标
            int sampleIndex = 0; // 当前即将进行处理的采样点下标
            double sumDisplacement = 0; // 当前已经经过的位移
            List<double> displacements = new(); // 本次计算中的曲线组最终位移，用于更新缓存

            // 从缓存校验曲线段并调取一致的采样点
            // curveIndex 和 curveIndex+1 两个贝塞尔点组成一条曲线段
            for (; curveIndex <= speedTemplateData.BezierCurves.Count - 2; curveIndex++)
            {
                // 如果远端贝塞尔点下标超出缓存下标（缓存中缺少元素），或与缓存数据不匹配，则结束从缓存调取，转为手动重算
                if (curveIndex + 1 <= tempedBezierPoints.Count ||
                    tempedBezierPoints[curveIndex] != speedTemplateData.BezierCurves[curveIndex] ||
                    tempedBezierPoints[curveIndex + 1] != speedTemplateData.BezierCurves[curveIndex + 1])
                {
                    break;
                }

                // 计算当前曲线段的结束时间对应的采样点下标。
                // 如果结束时间正好落在采样频率整数倍上，暂不采样此时间点，交给下个曲线段处理。
                int maxSampleIndex =
                    speedTemplateData.BezierCurves[curveIndex + 1].PositionPoint.MsTime / SampleIntervalMsTime;

                // 从缓存采样点中拷贝数据
                for (; sampleIndex <= maxSampleIndex; sampleIndex++)
                {
                    speedList.Add(tempedSpeedList[sampleIndex]);
                    distanceList.Add(tempedDisplacementList[sampleIndex]);
                }

                // 累加位移
                displacements.Add(tempedCurveFinalDisplacements[curveIndex]);
                sumDisplacement += tempedCurveFinalDisplacements[curveIndex];
            }

            // 缓存失效，需要继续迭代计算后续曲线段的 位移-时间 积分，然后根据曲线和积分采样速度和位移
            for (; curveIndex <= speedTemplateData.BezierCurves.Count - 2; curveIndex++)
            {
                // 计算当前曲线段的结束时间对应的采样点下标。
                // 如果结束时间正好落在采样频率整数倍上，暂不采样此时间点，交给下个曲线段处理。
                int maxSampleIndex =
                    speedTemplateData.BezierCurves[curveIndex + 1].PositionPoint.MsTime / SampleIntervalMsTime;

                // 采样速度和位移
                for (; sampleIndex < maxSampleIndex; sampleIndex++)
                {
                    // 二分法查找 T->t
                    float t = BezierHelper.FindTForX(
                        sampleIndex * SampleIntervalMsTime,
                        speedTemplateData.BezierCurves[curveIndex].PositionPoint.MsTime,
                        speedTemplateData.BezierCurves[curveIndex].RightControlPoint.MsTime,
                        speedTemplateData.BezierCurves[curveIndex + 1].LeftControlPoint.MsTime,
                        speedTemplateData.BezierCurves[curveIndex + 1].PositionPoint.MsTime
                    );

                    // 对贝塞尔曲线 t->V 采样速度
                    speedList.Add(
                        playerSpeed *
                        BezierHelper.CalculateVForT(
                            t,
                            speedTemplateData.BezierCurves[curveIndex].PositionPoint.Value,
                            speedTemplateData.BezierCurves[curveIndex].RightControlPoint.Value,
                            speedTemplateData.BezierCurves[curveIndex + 1].LeftControlPoint.Value,
                            speedTemplateData.BezierCurves[curveIndex + 1].PositionPoint.Value
                        )
                    );

                    // 积分获取多项式面积，t->D 采样位移，将采样位移与积累位移相加后存入
                    distanceList.Add(
                        (float)sumDisplacement +
                        playerSpeed *
                        (float)BezierHelper.CalculateBezierArea(
                            t,
                            speedTemplateData.BezierCurves[curveIndex].PositionPoint,
                            speedTemplateData.BezierCurves[curveIndex].RightControlPoint,
                            speedTemplateData.BezierCurves[curveIndex + 1].LeftControlPoint,
                            speedTemplateData.BezierCurves[curveIndex + 1].PositionPoint
                        )
                    );
                }

                // 采样结束，将本条曲线 t=1 时的位移累加到缓存
                double distance =
                    playerSpeed *
                    BezierHelper.CalculateBezierArea(
                        1,
                        speedTemplateData.BezierCurves[curveIndex].PositionPoint,
                        speedTemplateData.BezierCurves[curveIndex].RightControlPoint,
                        speedTemplateData.BezierCurves[curveIndex + 1].LeftControlPoint,
                        speedTemplateData.BezierCurves[curveIndex + 1].PositionPoint
                    );
                displacements.Add(distance);
                sumDisplacement += distance;
            }

            // 用本次计算的拷贝数据更新缓存
            tempedBezierPoints = new List<BezierPoint>(speedTemplateData.BezierCurves.Points);
            tempedCurveFinalDisplacements = displacements;
            tempedSpeedList = new List<float>(speedList);
            tempedDisplacementList = new List<float>(distanceList);
        }
    }
}
