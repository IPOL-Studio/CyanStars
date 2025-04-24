using System.Collections.Generic;

namespace CyanStars.Gameplay.Chart
{
    /// <summary>
    /// 加载到游戏内的速度组
    /// </summary>
    public class SpeedGroup
    {
        /// <summary>
        /// 从曲线转为离散速度时，采样时间间隔（ms）
        /// </summary>
        private const int SampleInterval = 10;

        /// <summary>
        /// 缓存的速度列表
        /// </summary>
        /// <remarks>列表从 0 时间差开始，每隔 SampleInterval 个时间向前采样，计算提前一段时间的速度</remarks>
        private List<float> speedList = new List<float>();

        /// <summary>
        /// 缓存的距离（视图层时间）列表
        /// </summary>
        /// <remarks>列表从 0 时间差开始，每隔 SampleInterval 个时间向前采样，计算提前一段时间的距离</remarks>
        private List<float> distanceList = new List<float>();


        /// <summary>
        /// 构造实例并生成速度和距离采样点
        /// </summary>
        public SpeedGroup(SpeedGroupData speedGroupData, float playerSpeed = 1f) // TODO: 外部传入玩家速度
        {
            float ps = playerSpeed;
            SpeedGroupType type = speedGroupData.Type;
            if (type == SpeedGroupType.Absolute)
            {
                ps = 1f;
            }

            // 根据持续时间计算采样点数量
            int length =
                speedGroupData.BezierCurve.CubicBeziers[speedGroupData.BezierCurve.CubicBeziers.Count - 1].P3.Time;
            int count = length / SampleInterval + 1;

            float sumDistance = 0f;

            for (int i = 0; i < count; i++)
            {
                float speed = speedGroupData.BezierCurve.GetSpeed(i * SampleInterval) * ps; // 从前方向后移动时速度为负
                speedList.Add(speed);
                sumDistance += speed * SampleInterval / 1000f; // 提前时距离为负数
                distanceList.Add(sumDistance);
            }
        }

        /// <summary>
        /// 根据逻辑时间差值获取速度
        /// </summary>
        /// <remarks>当前时间提前于音符判定时间时 logicTimeDistance 为负值</remarks>
        public float GetSpeed(float logicTimeDistance)
        {
            if (logicTimeDistance > 0)
            {
                return speedList[0];
            }

            if (-logicTimeDistance >= speedList.Count * SampleInterval)
            {
                return speedList[speedList.Count - 1];
            }

            return speedList[(int)-logicTimeDistance / SampleInterval];
        }

        /// <summary>
        /// 根据逻辑时间差值获取距离（视图层时间差值）
        /// </summary>
        /// <remarks>当前时间提前于音符判定时间时 logicTimeDistance 为负值</remarks>
        public float GetDistance(float logicTimeDistance)
        {
            if (logicTimeDistance > 0)
            {
                // 过线后按照当前速度计算距离，而非从采样点缓存读取
                return speedList[0] * logicTimeDistance / 1000f;
            }

            if (-logicTimeDistance >= distanceList.Count * SampleInterval)
            {
                // 在进入首个采样点前按照速度计算距离
                return speedList[distanceList.Count - 1] * logicTimeDistance / 1000f;
            }

            return distanceList[(int)-logicTimeDistance / SampleInterval];
        }
    }
}
