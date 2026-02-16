#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CyanStars.Chart.BezierCurve;
using UnityEngine;

namespace CyanStars.Chart
{

    public sealed class SpeedTemplateBaker : ISpeedTemplateBaker
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

        public bool IsSupportParallel => true;


        /// <inheritdoc />
        public bool Bake(SpeedTemplateData speedTemplateData,
                         float playerSpeed,
                         [NotNullWhen(true)] out List<float>? speedList,
                         [NotNullWhen(true)] out List<float>? displacementList)
        {
            speedList = null;
            displacementList = null;

            // 如果曲线组贝塞尔点小于等于 1 个，返回空列表。请直接用 [^1].Value 获取
            if (speedTemplateData.BezierCurves.Count <= 1)
            {
                return false;
            }

            if (speedTemplateData.Type == SpeedTemplateType.Absolute)
            {
                playerSpeed = 1;
            }

            int expectedSampledCount = speedTemplateData.BezierCurves[^1].PositionPoint.MsTime / SampleIntervalMsTime + 1;
            speedList = new List<float>(expectedSampledCount);
            displacementList = new List<float>(expectedSampledCount);

            int sampledCount = 0; // 已经处理的采样点数量，也可直接作为即将进行处理的采样点下标
            double sumDisplacement = 0; // 当前已经经过的位移

            // 迭代计算后续曲线段的 位移-时间 积分，然后根据曲线和积分采样速度和位移
            for (int i = 0; i <= speedTemplateData.BezierCurves.Count - 2; i++)
            {
                // 采样速度和位移
                // 如果结束时间正好落在采样频率整数倍上，暂不采样此时间点，交给下个曲线段处理。
                int nextMsTime = speedTemplateData.BezierCurves[i + 1].PositionPoint.MsTime;
                while (sampledCount * SampleIntervalMsTime < nextMsTime)
                {
                    // 二分法查找 T->t
                    float t = BezierHelper.FindTForX(
                        sampledCount * SampleIntervalMsTime,
                        speedTemplateData.BezierCurves[i].PositionPoint.MsTime,
                        speedTemplateData.BezierCurves[i].RightControlPoint.MsTime,
                        speedTemplateData.BezierCurves[i + 1].LeftControlPoint.MsTime,
                        speedTemplateData.BezierCurves[i + 1].PositionPoint.MsTime
                    );

                    // 对贝塞尔曲线 t->V 采样速度
                    speedList.Add(
                        playerSpeed *
                        (float)BezierHelper.CalculateValueForT(
                            t,
                            speedTemplateData.BezierCurves[i].PositionPoint.Value,
                            speedTemplateData.BezierCurves[i].RightControlPoint.Value,
                            speedTemplateData.BezierCurves[i + 1].LeftControlPoint.Value,
                            speedTemplateData.BezierCurves[i + 1].PositionPoint.Value
                        )
                    );

                    // 积分获取多项式面积，t->D 采样位移，将采样位移与积累位移相加后存入
                    double segmentDistance = BezierHelper.CalculateBezierArea(
                        t,
                        speedTemplateData.BezierCurves[i].PositionPoint,
                        speedTemplateData.BezierCurves[i].RightControlPoint,
                        speedTemplateData.BezierCurves[i + 1].LeftControlPoint,
                        speedTemplateData.BezierCurves[i + 1].PositionPoint
                    );

                    displacementList.Add((float)sumDisplacement + (float)segmentDistance * playerSpeed);

                    sampledCount++;
                }

                // 采样结束，将本条曲线 t=1 时的位移累加到缓存
                double distance = BezierHelper.CalculateBezierArea(
                    1,
                    speedTemplateData.BezierCurves[i].PositionPoint,
                    speedTemplateData.BezierCurves[i].RightControlPoint,
                    speedTemplateData.BezierCurves[i + 1].LeftControlPoint,
                    speedTemplateData.BezierCurves[i + 1].PositionPoint
                );
                sumDisplacement += distance * playerSpeed;
            }

            return true;
        }

        /// <inheritdoc />
        public double GetFinalDisplacement(SpeedTemplateData speedTemplateData, float playerSpeed)
        {
            if (speedTemplateData.BezierCurves.Count <= 1)
            {
                return 0.0;
            }

            if (speedTemplateData.Type == SpeedTemplateType.Absolute)
            {
                playerSpeed = 1f;
            }

            double sumDisplacement = 0.0;
            for (int i = 0; i <= speedTemplateData.BezierCurves.Count - 2; i++)
            {
                double distance = BezierHelper.CalculateBezierArea(
                    1,
                    speedTemplateData.BezierCurves[i].PositionPoint,
                    speedTemplateData.BezierCurves[i].RightControlPoint,
                    speedTemplateData.BezierCurves[i + 1].LeftControlPoint,
                    speedTemplateData.BezierCurves[i + 1].PositionPoint
                );
                sumDisplacement += distance * playerSpeed;
            }

            return sumDisplacement;
        }
    }
}
