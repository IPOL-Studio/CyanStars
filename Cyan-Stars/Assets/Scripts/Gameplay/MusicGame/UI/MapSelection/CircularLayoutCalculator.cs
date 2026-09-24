#nullable enable

using System.Diagnostics.Contracts;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 环形/椭圆排列的纯计算工具。
    /// 谱包轮盘和谱面难度轮盘共用这里的角度与椭圆坐标计算，避免两处各自维护三角函数。
    /// </summary>
    public static class CircularLayoutCalculator
    {
        /// <summary>
        /// 计算相邻 item 之间的角度间隔。
        /// </summary>
        /// <param name="padding">相邻 item 沿圆弧的间距。</param>
        /// <param name="radius">圆弧半径。</param>
        /// <returns>相邻 item 的角度间隔（度），半径为 0 时返回 0。</returns>
        [Pure]
        public static float CalculatePaddingAngle(float padding, float radius)
        {
            return radius != 0f ? padding / (2f * Mathf.PI * radius) * 360f : 0f;
        }

        /// <summary>
        /// 计算圆弧中央的角度。
        /// </summary>
        /// <param name="startAngle">圆弧起始角度（度）。</param>
        /// <param name="endAngle">圆弧结束角度（度）。</param>
        [Pure]
        public static float CalculateCenterAngle(float startAngle, float endAngle)
        {
            return (startAngle + endAngle) / 2f;
        }

        /// <summary>
        /// 计算所有 item 整体所占的角度。
        /// </summary>
        /// <param name="itemCount">item 数量。</param>
        /// <param name="paddingAngle">相邻 item 之间的角度间隔（度）。</param>
        [Pure]
        public static float CalculateItemsTotalAngle(int itemCount, float paddingAngle)
        {
            return Mathf.Max(0, itemCount - 1) * paddingAngle;
        }

        /// <summary>
        /// 计算椭圆上指定角度对应的局部坐标。
        /// 角度从正上方开始顺时针增加：x = sin(angle) * radius * scaleX，y = cos(angle) * radius。
        /// scaleX 为 1 时是正圆，否则是横向拉伸/压缩后的椭圆。
        /// </summary>
        /// <param name="angle">椭圆上的角度（度）。</param>
        /// <param name="radius">椭圆纵向半径。</param>
        /// <param name="scaleX">椭圆横向缩放。</param>
        /// <returns>以圆心为原点的椭圆局部坐标。</returns>
        [Pure]
        public static Vector3 CalculateEllipsePosition(float angle, float radius, float scaleX)
        {
            float radians = angle * Mathf.Deg2Rad;
            return new Vector3(
                Mathf.Sin(radians) * radius * scaleX,
                Mathf.Cos(radians) * radius,
                0f
            );
        }

        /// <summary>
        /// 将纵向归一化坐标映射为椭圆右半边的横向归一化坐标。
        /// 视口中心对应椭圆圆心、视口半高对应椭圆纵向半径，
        /// 由纵向坐标反推椭圆角度后，取椭圆右半边的横向坐标并归一化。
        /// </summary>
        /// <param name="normalizedY">纵向归一化坐标，范围 [-1, 1]，1 表示椭圆正上方。</param>
        /// <param name="radius">椭圆纵向半径。</param>
        /// <param name="scaleX">椭圆横向缩放。</param>
        /// <returns>横向归一化坐标，0 表示椭圆圆心所在纵轴，1 表示椭圆最右侧。</returns>
        [Pure]
        public static float CalculateRightEllipseNormalizedX(float normalizedY, float radius, float scaleX)
        {
            float clampedNormalizedY = Mathf.Clamp(normalizedY, -1f, 1f);
            float angle = Mathf.Acos(clampedNormalizedY) * Mathf.Rad2Deg;

            float horizontalRadius = Mathf.Abs(radius * scaleX);
            if (horizontalRadius == 0f)
                return 0f;

            Vector3 ellipsePosition = CalculateEllipsePosition(angle, radius, scaleX);
            return Mathf.Clamp01(Mathf.Abs(ellipsePosition.x) / horizontalRadius);
        }
    }
}
