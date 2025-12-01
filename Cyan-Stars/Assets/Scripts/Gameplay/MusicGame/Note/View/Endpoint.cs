using CyanStars.Utils;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class Endpoint : SingletonMono<Endpoint> //场景的两个端点
    {
        [SerializeField]
        private Transform leftTrans; //左端点

        [SerializeField]
        private Transform rightTrans; //右端点

        public Transform LeftTrans => leftTrans;
        public Transform RightTrans => rightTrans;

        /// <summary>
        /// 获取左端点的X坐标
        /// </summary>
        public float LeftPos => leftTrans.position.x;

        /// <summary>
        /// 获取右端点的X坐标
        /// </summary>
        public float RightPos => rightTrans.position.x;

        /// <summary>
        /// 场景宽度
        /// </summary>
        public float Length => Mathf.Abs(RightPos - LeftPos);

        /// <summary>
        /// 获取指定比例的X坐标
        /// </summary>
        public float GetPosWithRatio(float ratio)
        {
            return LeftPos + ratio * Length;
        }
    }
}
