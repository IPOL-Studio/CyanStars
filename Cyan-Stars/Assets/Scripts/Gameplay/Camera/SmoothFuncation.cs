using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFuncation
{
    // b:开始值  e:结束值 t:当前时间，dt:持续时间

    //对Vector3类型的缓动
    public static Vector3 LinearFunction(Vector3 b,Vector3 e,float t,float dt)//线性匀速运动效果
    {
        return b + (e - b) * t / dt;
    }
    public static Vector3 SinFunctionEaseIn(Vector3 b, Vector3 e, float t, float dt)//正弦曲线的缓动（sin(t)）/ 从0开始加速的缓动，也就是先慢后快
    {
    return -(e - b) * Mathf.Cos(t/dt * (Mathf.PI/2)) + (e - b) + b;
    }
    public static Vector3 SinFunctionEaseOut(Vector3 b, Vector3 e, float t, float dt)//正弦曲线的缓动（sin(t)）/ 减速到0的缓动，也就是先快后慢
    {
        return (e - b) * Mathf.Sin(t / dt * (Mathf.PI / 2)) + b;
    }
    public static Vector3 SinFunctionEaseInOut(Vector3 b, Vector3 e, float t, float dt)//正弦曲线的缓动（sin(t)）/ 前半段从0开始加速，后半段减速到0的缓动
    {
        return -(e - b) / 2 * (Mathf.Cos(Mathf.PI * t / dt) - 1) + b;
    }
    public static Vector3 BackEaseIn(Vector3 b, Vector3 e, float t, float dt)//超过范围的三次方缓动（(s+1)*t^3 – s*t^2）/ 从0开始加速的缓动，也就是先慢后快
    {
        float s = 1f;
        return (e - b) * (t /= dt) * t * ((s + 1) * t - s) + b;
    }

    //对float类型的缓动
    public static float LinearFunction(float b, float e, float t, float dt)//线性匀速运动效果
    {
        return b + (e - b) * t / dt;
    }
    public static float SinFunctionEaseIn(float b, float e, float t, float dt)//正弦曲线的缓动（sin(t)）/ 从0开始加速的缓动，也就是先慢后快
    {
        return -(e - b) * Mathf.Cos(t / dt * (Mathf.PI / 2)) + (e - b) + b;
    }
    public static float SinFunctionEaseOut(float b, float e, float t, float dt)//正弦曲线的缓动（sin(t)）/ 减速到0的缓动，也就是先快后慢
    {
        return (e - b) * Mathf.Sin(t / dt * (Mathf.PI / 2)) + b;
    }
    public static float SinFunctionEaseInOut(float b, float e, float t, float dt)//正弦曲线的缓动（sin(t)）/ 前半段从0开始加速，后半段减速到0的缓动
    {
        return -(e - b) / 2 * (Mathf.Cos(Mathf.PI * t / dt) - 1) + b;
    }
    public static float BackEaseIn(float b, float e, float t, float dt)//超过范围的三次方缓动（(s+1)*t^3 – s*t^2）/ 从0开始加速的缓动，也就是先慢后快
    {
        float s = 1f;
        return (e - b) * (t /= dt) * t * ((s + 1) * t - s) + b;
    }

    //特殊函数
    public static float CubicFuncation(float b,float e,float t,float dt)
    {
        return ((e-b)/(dt*dt*dt))*(-t + dt)*(-t + dt)*(-t + dt) + b;
    }
}
