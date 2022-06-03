using UnityEngine;

namespace CyanStars.Gameplay.Misc
{
    public static class EasingFunction
    {
        // b:开始值  e:结束值 t:当前时间，dt:持续时间

#region 对Vector3类型的缓动

        /// <summary>线性匀速运动效果</summary>
        public static Vector3 LinearFunction(Vector3 b, Vector3 e, float t, float dt)
        {
            return b + (e - b) * t / dt;
        }

        /// <summary>
        /// 正弦曲线的缓动（sin(t)）
        /// <para>从0开始加速的缓动，也就是先慢后快</para>
        /// </summary>
        public static Vector3 SinFunctionEaseIn(Vector3 b, Vector3 e, float t, float dt)
        {
            return -(e - b) * Mathf.Cos(t / dt * (Mathf.PI / 2)) + (e - b) + b;
        }

        /// <summary>
        /// 正弦曲线的缓动（sin(t)
        /// <para>减速到0的缓动，也就是先快后慢</para>
        /// </summary>
        public static Vector3 SinFunctionEaseOut(Vector3 b, Vector3 e, float t, float dt)
        {
            return (e - b) * Mathf.Sin(t / dt * (Mathf.PI / 2)) + b;
        }

        ///<summary>
        /// 正弦曲线的缓动（sin(t)）
        /// <para>前半段从0开始加速，后半段减速到0的缓动</para>
        /// </summary>
        public static Vector3 SinFunctionEaseInOut(Vector3 b, Vector3 e, float t, float dt)
        {
            return -(e - b) / 2 * (Mathf.Cos(Mathf.PI * t / dt) - 1) + b;
        }

        ///<summary>
        /// 超过范围的三次方缓动（(s+1)*t^3 – s*t^2）
        /// <para>从0开始加速的缓动，也就是先慢后快</para>
        /// </summary>
        public static Vector3 BackEaseIn(Vector3 b, Vector3 e, float t, float dt)
        {
            float s = 1.70158f;
            return (e - b) * (t /= dt) * t * ((s + 1) * t - s) + b;
        }

#endregion

#region 对float类型的缓动

        ///<summary>线性匀速运动效果</summary>
        public static float LinearFunction(float b, float e, float t, float dt)
        {
            return b + (e - b) * t / dt;
        }

        ///<summary>
        /// 正弦曲线的缓动（sin(t)）
        /// <para>从0开始加速的缓动，也就是先慢后快</para>
        /// </summary>
        public static float SinFunctionEaseIn(float b, float e, float t, float dt)
        {
            return -(e - b) * Mathf.Cos(t / dt * (Mathf.PI / 2)) + (e - b) + b;
        }

        ///<summary>
        /// 正弦曲线的缓动（sin(t)
        /// <para>减速到0的缓动，也就是先快后慢</para>
        /// </summary>
        public static float SinFunctionEaseOut(float b, float e, float t, float dt)
        {
            return (e - b) * Mathf.Sin(t / dt * (Mathf.PI / 2)) + b;
        }

        ///<summary>
        /// 正弦曲线的缓动（sin(t)
        /// <para>前半段从0开始加速，后半段减速到0的缓动</para>
        /// </summary>
        public static float
            SinFunctionEaseInOut(float b, float e, float t, float dt)
        {
            return -(e - b) / 2 * (Mathf.Cos(Mathf.PI * t / dt) - 1) + b;
        }

        ///<summary>
        /// 超过范围的三次方缓动（(s+1)*t^3 – s*t^2）
        /// <para>从0开始加速的缓动，也就是先慢后快</para></summary>
        public static float BackEaseIn(float b, float e, float t, float dt)
        {
            float s = 1.70158f;
            return (e - b) * (t /= dt) * t * ((s + 1) * t - s) + b;
        }

#endregion

        //特殊函数
        public static float CubicFunction(float b, float e, float t, float dt)
        {
            return ((e - b) / (dt * dt * dt)) * (-t + dt) * (-t + dt) * (-t + dt) + b;
        }
    }
}
