using System;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Chart
{
    /// <summary>
    /// 贝塞尔曲线
    /// </summary>
    /// <remarks>一整条曲线，即一个变速组，由多个贝赛尔曲线控制点组成</remarks>
    public class BezierCurve
    {
        private List<BezierControlPoint> controlPoints;
        public IReadOnlyList<BezierControlPoint> ControlPoints => controlPoints.AsReadOnly();

        public BezierCurve()
        {
            Vector2 vector2 = new Vector2(0, 1);
            controlPoints = new List<BezierControlPoint> { new BezierControlPoint(vector2, vector2, vector2) };
        }

        /// <summary>
        /// 校验并在列表合适位置插入新的控制点
        /// </summary>
        /// <param name="newPoint">要插入的新控制点</param>
        /// <returns>如果成功插入则返回true，否则返回false</returns>
        public bool InsertPoint(BezierControlPoint newPoint)
        {
            // 边界检查：不能插入到 x >= 0 的位置，因为x=0的点必须是第一个
            if (newPoint.Position.x >= 0)
            {
                Debug.LogWarning("无法在 x >= 0 的位置插入新点。");
                return false;
            }

            // 检查是否存在相同x坐标的点
            if (controlPoints.Exists(p => Mathf.Approximately(p.Position.x, newPoint.Position.x)))
            {
                Debug.LogWarning($"已存在 x = {newPoint.Position.x} 的控制点，无法插入。");
                return false;
            }

            // 寻找插入位置（列表按x坐标降序排列）
            // 找到第一个x坐标小于新点的现有控制点，新点应插入其前面
            int insertIndex = controlPoints.FindIndex(p => p.Position.x < newPoint.Position.x);
            if (insertIndex == -1)
            {
                // 如果没找到，说明新点的x坐标是最小的，应插在列表末尾
                insertIndex = controlPoints.Count;
            }

            // 校验新点
            bool isValid = ValidatePoint(newPoint, insertIndex);

            if (isValid)
            {
                controlPoints.Insert(insertIndex, newPoint);
                return true;
            }

            Debug.LogWarning("新控制点校验失败，无法插入。");
            return false;
        }

        /// <summary>
        /// 校验待插入点的控制点是否满足曲线不折返的条件
        /// </summary>
        /// <param name="pointToValidate">待校验的点</param>
        /// <param name="index">该点将要插入的索引</param>
        /// <returns>是否通过校验</returns>
        private bool ValidatePoint(BezierControlPoint pointToValidate, int index)
        {
            // 获取相邻点（基于插入后的位置）
            // rightNeighbor 是坐标轴上右边的点（x更大），在列表中索引更小
            BezierControlPoint rightNeighbor = (index > 0) ? controlPoints[index - 1] : null;
            // leftNeighbor 是坐标轴上左边的点（x更小），在列表中索引更大
            BezierControlPoint leftNeighbor = (index < controlPoints.Count) ? controlPoints[index] : null;

            // 校验RightControlPoint.x
            // 必须介于 [Position.x, 右侧相邻点的LeftControlPoint.x] 之间
            if (rightNeighbor != null)
            {
                if (pointToValidate.RightControlPoint.x < pointToValidate.Position.x ||
                    pointToValidate.RightControlPoint.x > rightNeighbor.LeftControlPoint.x)
                {
                    Debug.LogWarning($"RightControlPoint.x ({pointToValidate.RightControlPoint.x}) 校验失败. " +
                                     $"它必须介于 [{pointToValidate.Position.x}, {rightNeighbor.LeftControlPoint.x}] 之间。");
                    return false;
                }
            }
            else // 如果没有右侧相邻点（即插入点为第一个点，但在本代码中此情况已排除），则只需满足自身约束
            {
                if (pointToValidate.RightControlPoint.x < pointToValidate.Position.x)
                {
                    Debug.LogWarning(
                        $"RightControlPoint.x ({pointToValidate.RightControlPoint.x}) 必须大于等于 Position.x ({pointToValidate.Position.x})。");
                    return false;
                }
            }


            // 校验LeftControlPoint.x
            // 必须介于 [左侧相邻点的RightControlPoint.x, Position.x] 之间
            if (leftNeighbor != null)
            {
                if (pointToValidate.LeftControlPoint.x > pointToValidate.Position.x ||
                    pointToValidate.LeftControlPoint.x < leftNeighbor.RightControlPoint.x)
                {
                    Debug.LogWarning($"LeftControlPoint.x ({pointToValidate.LeftControlPoint.x}) 校验失败. " +
                                     $"它必须介于 [{leftNeighbor.RightControlPoint.x}, {pointToValidate.Position.x}] 之间。");
                    return false;
                }
            }
            else // 如果没有左侧相邻点（即插入点为最后一个点），则只需满足自身约束
            {
                if (pointToValidate.LeftControlPoint.x > pointToValidate.Position.x)
                {
                    Debug.LogWarning(
                        $"LeftControlPoint.x ({pointToValidate.LeftControlPoint.x}) 必须小于等于 Position.x ({pointToValidate.Position.x})。");
                    return false;
                }
            }


            return true;
        }

        /// <summary>
        /// 删除指定的控制点
        /// </summary>
        /// <param name="index">要删除的控制点的索引</param>
        /// <returns>如果成功删除则返回true</returns>
        public bool DeletePoint(int index)
        {
            // 控制点0是基准点，不能删除
            if (index == 0)
            {
                Debug.LogError("不能删除索引为0的控制点。");
                return false;
            }

            if (index < 0 || index >= controlPoints.Count)
            {
                Debug.LogError("索引超出范围。");
                return false;
            }

            controlPoints.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 根据传入的x（时间），返回唯一的y值
        /// </summary>
        /// <param name="x">x坐标（通常为负数）</param>
        /// <returns>对应的y坐标</returns>
        public float GetValue(int x)
        {
            // 当x大于0时，返回列表第一个控制点位置的y
            if (x >= 0)
            {
                return controlPoints[0].Position.y;
            }

            // 当x小于曲线最左侧控制点位置的x时，返回列表最后一个控制点位置的y
            if (x <= controlPoints[controlPoints.Count - 1].Position.x)
            {
                return controlPoints[controlPoints.Count - 1].Position.y;
            }

            // 寻找x所在的曲线段
            int segmentIndex = -1;
            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                // x 位于点 i 和点 i+1 之间
                if (x <= controlPoints[i].Position.x && x >= controlPoints[i + 1].Position.x)
                {
                    segmentIndex = i;
                    break;
                }
            }

            if (segmentIndex == -1)
            {
                // 理论上在边界检查后不会发生
                return controlPoints[controlPoints.Count - 1].Position.y;
            }

            // 获取定义该段曲线的四个点
            Vector2 p0 = controlPoints[segmentIndex].Position;
            Vector2 p1 = controlPoints[segmentIndex].LeftControlPoint;
            Vector2 p2 = controlPoints[segmentIndex + 1].RightControlPoint;
            Vector2 p3 = controlPoints[segmentIndex + 1].Position;

            // 使用二分法查找对应x的参数t (0到1之间)
            float t = FindTForX(x, p0.x, p1.x, p2.x, p3.x);

            // 根据t计算y值
            return CalculateBezierPoint(t, p0.y, p1.y, p2.y, p3.y);
        }

        /// <summary>
        /// 计算一维三次贝塞尔曲线在t处的值
        /// </summary>
        private float CalculateBezierPoint(float t, float p0, float p1, float p2, float p3)
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
        /// 二分法查找给定x值在贝塞尔曲线段上的参数t
        /// </summary>
        private float FindTForX(float x, float p0X, float p1X, float p2X, float p3X)
        {
            float tLow = 0;
            float tHigh = 1;
            float t = 0.5f;
            float currentX;
            int iterations = 0;
            const int maxIterations = 100; // 防止无限循环
            const float epsilon = 0.0001f; // 精度

            do
            {
                t = (tLow + tHigh) / 2;
                currentX = CalculateBezierPoint(t, p0X, p1X, p2X, p3X);

                if (currentX > x)
                {
                    tHigh = t;
                }
                else
                {
                    tLow = t;
                }

                iterations++;
            } while (Mathf.Abs(currentX - x) > epsilon && iterations < maxIterations);

            return t;
        }
    }


    /// <summary>
    /// 贝塞尔控制点
    /// </summary>
    /// <remarks>Vector2 的 x 单位为 ms，必须小于等于 0</remarks>
    public class BezierControlPoint
    {
        public Vector2 Position;
        public Vector2 LeftControlPoint;
        public Vector2 RightControlPoint;

        public BezierControlPoint(Vector2 position, Vector2 leftControlPoint, Vector2 rightControlPoint)
        {
            if (position.x > 0)
            {
                Debug.LogError("BezierCurve: 曲线时间必须小于等于0");
                throw new ArgumentException("BezierCurve: 曲线时间必须小于等于0");
            }

            position = new Vector2((int)position.x, position.y);
            Position = position;
            LeftControlPoint = leftControlPoint;
            RightControlPoint = rightControlPoint;
        }
    }
}
