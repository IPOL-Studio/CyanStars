using CyanStars.Framework;
using UnityEngine;

namespace CyanStars.Gameplay.Note
{
    public class Endpoint : SingletonMono<Endpoint> //场景的两个端点
    {
        public Transform leftTrans; //左端点
        public Transform rightTrans; //右端点

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
