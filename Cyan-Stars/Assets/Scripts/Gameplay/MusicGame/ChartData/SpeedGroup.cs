using System.Collections.Generic;

namespace CyanStars.Gameplay.MusicGame
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
                float speed = speedGroupData.BezierCurve.CalculateSpeed(i * SampleInterval * -1) * ps;
                speedList.Add(speed);
                sumDistance += speed * SampleInterval / 1000f;
                distanceList.Add(sumDistance);
            }
        }

        /// <summary>
        /// 根据逻辑时间差值获取速度
        /// </summary>
        /// <remarks>当前时间提前于音符判定时间时 distance 为负值</remarks>
        public float CalculateSpeed(float distance)
        {
            if (distance > 0)
            {
                return speedList[0];
            }

            if (-distance >= speedList.Count * SampleInterval)
            {
                return speedList[speedList.Count - 1];
            }

            return speedList[(int)-distance / SampleInterval];
        }

        /// <summary>
        /// 根据逻辑时间差值获取距离（视图层时间）
        /// </summary>
        /// <remarks>当前时间提前于音符判定时间时 distance 为负值</remarks>
        public float CalculateDistance(float distance)
        {
            if (distance > 0)
            {
                return distanceList[0];
            }

            if (-distance >= distanceList.Count * SampleInterval)
            {
                return distanceList[distanceList.Count - 1];
            }

            return distanceList[(int)-distance / SampleInterval];
        }
    }
}
