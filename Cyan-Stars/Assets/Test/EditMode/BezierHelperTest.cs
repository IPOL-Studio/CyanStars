#nullable enable

using System.Collections.Generic;
using CyanStars.Chart.BezierCurve;
using NUnit.Framework;

namespace Test.EditMode
{
    /// <summary>
    /// 测试 BezierHelper：
    /// - 是否正确计算 P(t)
    /// - 是否正确二分获取 t(x)
    /// </summary>
    public class BezierHelperTest
    {
        /// <summary>
        /// 根据 x 二分查找 t 时，可容许的返回的 t 与真实 t 的误差量
        /// </summary>
        private const float Epsilon = 0.001f;

        // 准备测试数据
        private static readonly BezierPointPos[] Points1 = { new(0, 0), new(0, 0), new(1000000, 1), new(1000000, 1) };
        private static readonly BezierPointPos[] Points2 = { new(0, 0), new(1, 0), new(0, 100000), new(1, 100000) };
        private static readonly BezierPointPos[] Points3 = { new(0, 0), new(100, 100), new(900, -100), new(1000, 0) };

        private static IEnumerable<TestCaseData> FindTForXTestCase
        {
            get
            {
                yield return new TestCaseData(Points1, 0, 0f).SetName("1000s 平滑贝塞尔曲线段 获取开始 t");
                yield return new TestCaseData(Points1, 1000000, 1f).SetName("1000s 平滑贝塞尔曲线段 获取结尾 t");
                yield return new TestCaseData(Points2, 0, 0f).SetName("1ms 尖锐贝塞尔曲线段 获取开始 t");
                yield return new TestCaseData(Points2, 1, 1f).SetName("1ms 尖锐贝塞尔曲线段 获取结尾 t");
                yield return new TestCaseData(Points3, 0, 0f).SetName("常规贝塞尔曲线段 获取开始 t");
                yield return new TestCaseData(Points3, 500, 0.5f).SetName("常规贝塞尔曲线段 获取中间 t");
                yield return new TestCaseData(Points3, 1000, 1f).SetName("常规贝塞尔曲线段 获取结尾 t");
                yield return new TestCaseData(Points3, 133, 0.2f).SetName("常规贝塞尔曲线段 获取随机 t 1");
                yield return new TestCaseData(Points3, 980, 0.95f).SetName("常规贝塞尔曲线段 获取随机 t 2");
            }
        }

        [Test, TestCaseSource(nameof(FindTForXTestCase))]
        public void FindTForXTest(BezierPointPos[] points, int inputX, float expectedResult)
        {
            var result = BezierHelper.FindTForX(
                inputX,
                points[0].MsTime,
                points[1].MsTime,
                points[2].MsTime,
                points[3].MsTime
            );
            Assert.AreEqual(expectedResult, result, Epsilon);
        }

        // /// <summary>
        // /// 使用这个方法来根据 t 获取精确的 x 和 y
        // /// </summary>
        // private static void EvaluateBezier(float t, BezierPointPos[] points, out double x, out double y)
        // {
        //     double td = t;
        //
        //     double x0 = points[0].MsTime;
        //     double y0 = points[0].Value;
        //     double x1 = points[1].MsTime;
        //     double y1 = points[1].Value;
        //     double x2 = points[2].MsTime;
        //     double y2 = points[2].Value;
        //     double x3 = points[3].MsTime;
        //     double y3 = points[3].Value;
        //
        //     double ax = -x0 + 3.0 * x1 - 3.0 * x2 + x3;
        //     double bx = 3.0 * x0 - 6.0 * x1 + 3.0 * x2;
        //     double cx = -3.0 * x0 + 3.0 * x1;
        //     double dx = x0;
        //     double ay = -y0 + 3.0 * y1 - 3.0 * y2 + y3;
        //     double by = 3.0 * y0 - 6.0 * y1 + 3.0 * y2;
        //     double cy = -3.0 * y0 + 3.0 * y1;
        //     double dy = y0;
        //
        //     double t2 = td * td;
        //     double t3 = t2 * td;
        //
        //     x = dx + td * (cx + td * (bx + td * ax));
        //     y = dy + td * (cy + td * (by + td * ay));
        // }
    }
}
