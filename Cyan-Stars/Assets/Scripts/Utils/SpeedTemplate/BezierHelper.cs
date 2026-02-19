// #nullable enable
//
// using System.Numerics;
//
// namespace CyanStars.Utils.SpeedTemplate
// {
//     /// <summary>
//     /// 贝塞尔曲线计算工具类
//     /// </summary>
//     /// <remarks>所有 VectorX 默认为 System.Numerics 命名空间下，而非 UnityEngine，以提高效率</remarks>
//     public static class BezierHelper
//     {
//         /// <summary>
//         /// 获取两点之间贝塞尔曲线的长度
//         /// </summary>
//         public static float GetLength(BezierPoint pointX, BezierPoint pointY)
//         {
//             // 据说这个算法的计算精度比 float 的存储精度还要高，怎么算的你先别管，反正我觉得已经是魔法了
//
//             // 5点高斯-勒让德预设节点和权重，不要改动这些常数
//             float[] t = { 0.046910077f, 0.230765345f, 0.5f, 0.769234655f, 0.953089923f };
//             float[] w = { 0.118463442f, 0.239314335f, 0.284444444f, 0.239314335f, 0.118463442f };
//
//             float length = 0;
//             for (int i = 0; i < t.Length; i++)
//             {
//                 Vector2 derivative = GetDerivative(pointX, pointY, t[i]);
//                 length += derivative.Length() * w[i];
//             }
//
//             return length;
//         }
//
//         private static Vector2 GetDerivative(BezierPoint pointX, BezierPoint pointY, float t)
//         {
//             GetControlPoints(pointX, pointY, out Vector2 p0, out Vector2 p1, out Vector2 p2, out Vector2 p3);
//             float mt = 1 - t;
//             return 3 * mt * mt * (p1 - p0) +
//                    6 * mt * t * (p2 - p1) +
//                    3 * t * t * (p3 - p2);
//         }
//
//
//         /// <summary>
//         /// 将两个贝塞尔点转换成相邻贝塞尔曲线的四个控制点
//         /// </summary>
//         /// <remarks>注意返回 System.Numerics.Vector2，用于 Unity 时还需转换</remarks>
//         public static void GetControlPoints(BezierPoint pointX, BezierPoint pointY, out Vector2 p0, out Vector2 p1, out Vector2 p2, out Vector2 p3)
//         {
//             p0 = new Vector2(pointX.PositionPoint.MsTime, pointX.PositionPoint.Value);
//             p1 = new Vector2(pointX.RightControlPoint.MsTime, pointX.RightControlPoint.Value);
//             p2 = new Vector2(pointY.LeftControlPoint.MsTime, pointY.LeftControlPoint.Value);
//             p3 = new Vector2(pointY.PositionPoint.MsTime, pointY.PositionPoint.Value);
//         }
//     }
// }


