#nullable enable

using UnityEngine;

namespace CyanStars.Utils.SpeedTemplate
{
    /// <summary>
    /// 贝塞尔曲线计算工具类
    /// </summary>
    /// <remarks>所有 VectorX 默认为 System.Numerics 命名空间下，而非 UnityEngine，以提高效率</remarks>
    public static class BezierHelper
    {
        // 二分法根据 x 查找 t 时的深度和精度
        private const int MaxIterations = 20; // 查找深度
        private const float Epsilon = 0.0001f; // 查找精度


        /// <summary>
        /// 二分法查找给定 x 值在贝塞尔曲线段上的参数 t
        /// </summary>
        public static float FindTForX(float x, float p0X, float p1X, float p2X, float p3X)
        {
            float tLow = 0;
            float tHigh = 1;
            float t;
            float currentX;
            int iterations = 0;

            do
            {
                t = (tLow + tHigh) / 2;
                currentX = CalculateVForT(t, p0X, p1X, p2X, p3X);

                if (currentX > x)
                {
                    tHigh = t;
                }
                else
                {
                    tLow = t;
                }

                iterations++;
            } while (Mathf.Abs(currentX - x) > Epsilon && iterations < MaxIterations);

            return t;
        }

        /// <summary>
        /// 计算一维三次贝塞尔曲线在 t 处的对应值
        /// </summary>
        /// <remarks>
        /// 后面四个参数接受统一的 x 或 y 坐标，返回值代表 t 在对应 x 或 y 轴上的值
        /// </remarks>
        public static float CalculateVForT(float t, float p0, float p1, float p2, float p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            float p = uuu * p0; // (1-t)^3 * P0
            p += 3 * uu * t * p1; // 3 * (1-t)^2 * t * P1
            p += 3 * u * tt * p2; // 3 * (1-t) * t^2 * P2
            p += ttt * p3; // t^3 * P3

            return p;
        }

        /// <summary>
        /// 直接计算三次贝塞尔曲线与 X 轴围成的面积
        /// </summary>
        /// <param name="t">进度 [0,1]</param>
        /// <param name="p0">起点</param>
        /// <param name="p1">控制点1</param>
        /// <param name="p2">控制点2</param>
        /// <param name="p3">终点</param>
        /// <returns>面积</returns>
        public static double CalculateBezierArea(double t, BezierPointPos p0, BezierPointPos p1, BezierPointPos p2, BezierPointPos p3)
        {
            // 1. 提取坐标并转为 double
            double x0 = p0.MsTime, y0 = p0.Value;
            double x1 = p1.MsTime, y1 = p1.Value;
            double x2 = p2.MsTime, y2 = p2.Value;
            double x3 = p3.MsTime, y3 = p3.Value;

            // 2. 计算贝塞尔多项式系数: P(t) = At^3 + Bt^2 + Ct + D

            // --- X轴系数 (用于求导) ---
            // x'(t) = 3*Ax*t^2 + 2*Bx*t + Cx
            // Ax = x3 - 3x2 + 3x1 - x0
            // Bx = 3x2 - 6x1 + 3x0
            // Cx = 3x1 - 3x0
            double ax = x3 - 3.0 * x2 + 3.0 * x1 - x0;
            double bx = 3.0 * x2 - 6.0 * x1 + 3.0 * x0;
            double cx = 3.0 * (x1 - x0);

            // --- Y轴系数 (用于积分) ---
            // y(t) = Ay*t^3 + By*t^2 + Cy*t + Dy
            double ay = y3 - 3.0 * y2 + 3.0 * y1 - y0;
            double by = 3.0 * y2 - 6.0 * y1 + 3.0 * y0;
            double cy = 3.0 * (y1 - y0);
            double dy = y0;

            // 3. 准备 x'(t) 的系数
            // x'(t) = dx_a * t^2 + dx_b * t + dx_c
            double dxA = 3.0 * ax;
            double dxB = 2.0 * bx;
            double dxC = cx;

            // 4. 多项式乘法: y(t) * x'(t) -> 得到一个 5 次多项式
            // I(t) = c5*t^5 + c4*t^4 + c3*t^3 + c2*t^2 + c1*t + c0

            double c5 = ay * dxA;
            double c4 = ay * dxB + by * dxA;
            double c3 = ay * dxC + by * dxB + cy * dxA;
            double c2 = by * dxC + cy * dxB + dy * dxA;
            double c1 = cy * dxC + dy * dxB;
            double c0 = dy * dxC;

            // 5. 积分求解
            // Area = (c5/6)t^6 + (c4/5)t^5 + ... + c0*t
            double t2 = t * t;
            double t3 = t2 * t;
            double t4 = t3 * t;
            double t5 = t4 * t;
            double t6 = t5 * t;

            return (
                (c5 / 6.0) * t6 +
                (c4 / 5.0) * t5 +
                (c3 / 4.0) * t4 +
                (c2 / 3.0) * t3 +
                (c1 / 2.0) * t2 +
                (c0) * t
            ) / 1000;
        }
    }
}
