using UnityEngine;

namespace CyanStars.Gameplay.Note
{
    public class Endpoint : MonoBehaviour //场景的两个端点
    {
        public static Endpoint Instance; //单例
        public GameObject leftObj; //左端点
        public GameObject rightObj; //右端点

        void Start()
        {
            Instance = this; //设置单例
        }

        /// <summary>
        /// 获取左端点的X坐标
        /// </summary>
        public float GetLeftPos()
        {
            return leftObj.transform.position.x;
        }

        /// <summary>
        /// 获取右端点的X坐标
        /// </summary>
        public float GetRightPos()
        {
            return rightObj.transform.position.x;
        }

        /// <summary>
        /// 场景宽度
        /// </summary>
        public float GetLenth()
        {
            return rightObj.transform.position.x - leftObj.transform.position.x;
        }

        /// <summary>
        /// 获取指定比例的X坐标
        /// </summary>
        public float GetPosWithRatio(float ratio)
        {
            return leftObj.transform.position.x + ratio * (rightObj.transform.position.x - leftObj.transform.position.x);
        }
    }
}
