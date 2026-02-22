#nullable enable

using System;
using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.Chart.BezierCurve;
using NUnit.Framework;

namespace Test.EditMode
{
    /// <summary>
    /// 测试 SpeedTemplateHelper 是否正常烘焙贝塞尔曲线组在采样点时的速度和位移
    /// </summary>
    public class SpeedTemplateHelperTest
    {
        /// <summary>
        /// 测试位移时允许的误差
        /// </summary>
        private const float DisplacementEpsilon = 0.001f;

        /// <summary>
        /// 测试速度时允许的误差
        /// </summary>
        private const float SpeedEpsilon = 0.001f;


        // 准备用于测试的贝塞尔曲线组
        private static BezierCurves bezierCurves1 =
            new BezierCurves(
                new BezierPoint(
                    new BezierPointPos(0, 0f),
                    new BezierPointPos(0, 0f),
                    new BezierPointPos(0, 0f)
                )
            );

        private static BezierCurves bezierCurves2 =
            new BezierCurves(
                new BezierPoint(
                    new BezierPointPos(0, 0f),
                    new BezierPointPos(0, 0f),
                    new BezierPointPos(0, 0f)
                )
            )
            {
                new BezierPoint(
                    new BezierPointPos(5, 10000f),
                    new BezierPointPos(5, 10000f),
                    new BezierPointPos(5, 10000f)
                ),
                new BezierPoint(
                    new BezierPointPos(10, 0f),
                    new BezierPointPos(10, 0f),
                    new BezierPointPos(10, 0f)
                ),
                new BezierPoint(
                    new BezierPointPos(15, -10000f),
                    new BezierPointPos(15, -10000f),
                    new BezierPointPos(15, -10000f)
                ),
                new BezierPoint(
                    new BezierPointPos(20, 0f),
                    new BezierPointPos(20, 0f),
                    new BezierPointPos(20, 0f)
                ),
                new BezierPoint(
                    new BezierPointPos(40, 10000f),
                    new BezierPointPos(40, 10000f),
                    new BezierPointPos(40, 10000f)
                )
            };


        private static IEnumerable<TestCaseData> SampleCountTestCase
        {
            get
            {
                yield return new TestCaseData(
                        new SpeedTemplateData(SpeedGroupType.Absolute, bezierCurves1),
                        2f,
                        SpeedTemplateHelper.SampleIntervalMsTime,
                        0
                    )
                    .SetName("单点曲线采样数量测试 1");
                yield return new TestCaseData(
                        new SpeedTemplateData(SpeedGroupType.Relative, bezierCurves1),
                        2f,
                        SpeedTemplateHelper.SampleIntervalMsTime,
                        0
                    )
                    .SetName("单点曲线采样数量测试 2");
                yield return new TestCaseData(
                        new SpeedTemplateData(SpeedGroupType.Absolute, bezierCurves2),
                        2f,
                        SpeedTemplateHelper.SampleIntervalMsTime,
                        4
                    )
                    .SetName("多点曲线采样数量测试 1");
                yield return new TestCaseData(
                        new SpeedTemplateData(SpeedGroupType.Relative, bezierCurves2),
                        2f,
                        SpeedTemplateHelper.SampleIntervalMsTime,
                        4
                    )
                    .SetName("多点曲线采样数量测试 2");
            }
        }

        private static IEnumerable<TestCaseData> GetFinalDisplacementTestCase
        {
            get
            {
                yield return new TestCaseData(
                        new SpeedTemplateData(SpeedGroupType.Absolute, bezierCurves1),
                        2f,
                        0
                    )
                    .SetName("单点曲线位移测试 1");
                yield return new TestCaseData(
                        new SpeedTemplateData(SpeedGroupType.Relative, bezierCurves1),
                        2f,
                        0
                    )
                    .SetName("单点曲线位移测试 2");
                yield return new TestCaseData(
                        new SpeedTemplateData(SpeedGroupType.Absolute, bezierCurves2),
                        2f,
                        100
                    )
                    .SetName("多点曲线位移测试 1");
                yield return new TestCaseData(
                        new SpeedTemplateData(SpeedGroupType.Relative, bezierCurves2),
                        2f,
                        200
                    )
                    .SetName("多点曲线位移测试 2");
            }
        }

        private static IEnumerable<TestCaseData> SampleValuesTestCase // TODO: 之后整点随机精确数据试试
        {
            get
            {
                yield return new TestCaseData(
                        new SpeedTemplateData(SpeedGroupType.Absolute, bezierCurves1),
                        2f,
                        new float[] { },
                        new float[] { }
                    )
                    .SetName("单点曲线采样测试 1");
                yield return new TestCaseData(
                        new SpeedTemplateData(SpeedGroupType.Relative, bezierCurves1),
                        2f,
                        new float[] { },
                        new float[] { }
                    )
                    .SetName("单点曲线采样测试 2");
                yield return new TestCaseData(
                        new SpeedTemplateData(SpeedGroupType.Absolute, bezierCurves2),
                        2f,
                        new[] { 0f, 0f, 0f, 5000f },
                        new[] { 0f, 50f, 0f, 25f }
                    )
                    .SetName("多点曲线采样测试 1");
                yield return new TestCaseData(
                        new SpeedTemplateData(SpeedGroupType.Relative, bezierCurves2),
                        2f,
                        new[] { 0f, 0f, 0f, 10000f },
                        new[] { 0f, 100f, 0f, 50f }
                    )
                    .SetName("多点曲线采样测试 2");
            }
        }


        /// <summary>
        /// 测试采样点数量是否正确
        /// </summary>
        [Test, TestCaseSource(nameof(SampleCountTestCase))]
        public void SampleCountTest(SpeedTemplateData speedTemplateData, float playerSpeed, int sampleIntervalMsTime, int expectedResult)
        {
            SpeedTemplateHelper.Bake(speedTemplateData, playerSpeed, out List<float> speedList, out List<float> displacementList);
            Assert.AreEqual(expectedResult, speedList.Count);
            Assert.AreEqual(expectedResult, displacementList.Count);
        }

        /// <summary>
        /// 测试计算整组曲线最终位移是否正确
        /// </summary>
        [Test, TestCaseSource(nameof(GetFinalDisplacementTestCase))]
        public void GetFinalDisplacementTest(SpeedTemplateData speedTemplateData, float playerSpeed, double expectedResult)
        {
            double displacement = SpeedTemplateHelper.GetFinalDisplacement(speedTemplateData, playerSpeed);
            Assert.AreEqual(expectedResult, displacement, DisplacementEpsilon);
        }

        /// <summary>
        /// 测试采样点结果是否正确
        /// </summary>
        [Test, TestCaseSource(nameof(SampleValuesTestCase))]
        public void SampleValuesTest(SpeedTemplateData speedTemplateData, float playerSpeed, float[] speeds, float[] displacements)
        {
            SpeedTemplateHelper.Bake(speedTemplateData, playerSpeed, out List<float> speedList, out List<float> displacementList);

            for (int i = 0; i < Math.Max(speedList.Count, speeds.Length); i++)
            {
                Assert.AreEqual(speeds[i], speedList[i], SpeedEpsilon);
                Assert.AreEqual(displacements[i], displacementList[i], DisplacementEpsilon);
            }
        }
    }
}
