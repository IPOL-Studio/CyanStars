using System;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Chart
{
    /// <summary>
    /// 用于变速模板的贝塞尔曲线类
    /// </summary>
    /// <remarks>
    /// 此曲线由至少 1 个 BezierControlPointItem 组成，首个 BezierControlPointItem.Position.x == 0，
    /// 后续 BezierControlPointItem.Position.x 必须小于前一个 BezierControlPointItem.Position.x，详见 BezierControlPointItem 类
    /// 曲线 x 值相对于音符判定时间，由 x==0 开始，向左侧 x 轴负方向延伸。例如 x=-100 代表此时为对应音符到达判定线前 100ms
    /// </remarks>
    public class BezierCurve
    {
        private List<BezierControlPointItem> controlPoints;
        public IReadOnlyList<BezierControlPointItem> ControlPoints => controlPoints.AsReadOnly();

        public BezierCurve()
        {
            Vector2 vector2 = new Vector2(0, 1);
            controlPoints = new List<BezierControlPointItem> { new BezierControlPointItem(vector2, vector2, vector2) };
        }

        /// <summary>
        /// 校验并在列表合适位置插入新的控制点
        /// </summary>
        /// <param name="newPointItem">要插入的新控制点</param>
        /// <returns>如果成功插入则返回true，否则返回false</returns>
        public bool InsertPoint(BezierControlPointItem newPointItem)
        {
            if (newPointItem.Position.x >= 0)
            {
                Debug.LogError("无法在 x >= 0 的位置插入新点。");
                return false;
            }

            if (controlPoints.Exists(p => Mathf.Approximately(p.Position.x, newPointItem.Position.x)))
            {
                Debug.LogError($"已存在 x = {newPointItem.Position.x} 的控制点，无法插入。");
                return false;
            }

            int insertIndex = controlPoints.FindIndex(p => p.Position.x < newPointItem.Position.x);
            if (insertIndex == -1)
            {
                insertIndex = controlPoints.Count;
            }

            bool isValid = ValidatePoint(newPointItem, insertIndex);

            if (isValid)
            {
                controlPoints.Insert(insertIndex, newPointItem);
                return true;
            }

            Debug.LogError("新控制点校验失败，无法插入。");
            return false;
        }

        /// <summary>
        /// 校验待插入点的控制点是否满足曲线不折返的条件
        /// </summary>
        /// <param name="pointItemToValidate">待校验的点</param>
        /// <param name="index">该点将要插入的索引</param>
        /// <returns>是否通过校验</returns>
        private bool ValidatePoint(BezierControlPointItem pointItemToValidate, int index)
        {
            BezierControlPointItem rightNeighbor = (index > 0) ? controlPoints[index - 1] : null;
            BezierControlPointItem leftNeighbor = (index < controlPoints.Count) ? controlPoints[index] : null;

            if (rightNeighbor != null)
            {
                if (pointItemToValidate.RightControlPoint.x < pointItemToValidate.Position.x ||
                    pointItemToValidate.RightControlPoint.x > rightNeighbor.LeftControlPoint.x)
                {
                    Debug.LogError($"RightControlPoint.x ({pointItemToValidate.RightControlPoint.x}) 校验失败. " +
                                   $"它必须介于 [{pointItemToValidate.Position.x}, {rightNeighbor.LeftControlPoint.x}] 之间。");
                    return false;
                }
            }
            else
            {
                if (pointItemToValidate.RightControlPoint.x < pointItemToValidate.Position.x)
                {
                    Debug.LogError(
                        $"RightControlPoint.x ({pointItemToValidate.RightControlPoint.x}) 必须大于等于 Position.x ({pointItemToValidate.Position.x})。");
                    return false;
                }
            }


            if (leftNeighbor != null)
            {
                if (pointItemToValidate.LeftControlPoint.x > pointItemToValidate.Position.x ||
                    pointItemToValidate.LeftControlPoint.x < leftNeighbor.RightControlPoint.x)
                {
                    Debug.LogError($"LeftControlPoint.x ({pointItemToValidate.LeftControlPoint.x}) 校验失败. " +
                                   $"它必须介于 [{leftNeighbor.RightControlPoint.x}, {pointItemToValidate.Position.x}] 之间。");
                    return false;
                }
            }
            else
            {
                if (pointItemToValidate.LeftControlPoint.x > pointItemToValidate.Position.x)
                {
                    Debug.LogError(
                        $"LeftControlPoint.x ({pointItemToValidate.LeftControlPoint.x}) 必须小于等于 Position.x ({pointItemToValidate.Position.x})。");
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
            if (x >= 0)
            {
                return controlPoints[0].Position.y;
            }

            if (x <= controlPoints[controlPoints.Count - 1].Position.x)
            {
                return controlPoints[controlPoints.Count - 1].Position.y;
            }

            int segmentIndex = -1;
            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
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

            Vector2 p0 = controlPoints[segmentIndex].Position;
            Vector2 p1 = controlPoints[segmentIndex].LeftControlPoint;
            Vector2 p2 = controlPoints[segmentIndex + 1].RightControlPoint;
            Vector2 p3 = controlPoints[segmentIndex + 1].Position;

            float t = FindTForX(x, p0.x, p1.x, p2.x, p3.x);
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
            const int maxIterations = 100; // 查找深度
            const float epsilon = 0.0001f; // 查找精度

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
    /// 贝塞尔控制点元素
    /// </summary>
    /// <remarks>
    /// 每个 Item 由三个点组成：
    ///  - Position：位置点，曲线必然穿过此点
    ///  - LeftControlPoint：左形变控制点，调整本条曲线和下一条曲线的形状
    ///  - RightControlPoint：右形变控制点，调整本条曲线和上一条曲线的形状
    /// </remarks>
    public class BezierControlPointItem
    {
        public Vector2 Position;
        public Vector2 LeftControlPoint;
        public Vector2 RightControlPoint;

        public BezierControlPointItem(Vector2 position, Vector2 leftControlPoint, Vector2 rightControlPoint)
        {
            if (position.x > 0)
            {
                throw new ArgumentException("BezierCurve: 曲线时间必须小于等于0");
            }

            position = new Vector2((int)position.x, position.y);
            Position = position;
            LeftControlPoint = leftControlPoint;
            RightControlPoint = rightControlPoint;
        }
    }
}
