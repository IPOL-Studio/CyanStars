using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour//摄像头控制脚本
{
    /*  使用说明：
     *  将此脚本挂载在需要改变位置的Camera下
     *  调用一次MoveCamera(Vector3 newPos, Vector3 newRot, float dTime,SmoothFuncationType type = SmoothFuncationType.Linear)
     *  newPos为移动后的位置（相对于默认位置）
     *  newRot为移动后的朝向（绝对位置）
     *  dTime为移动，单位毫秒
     *  type见下
     */

    public static CameraController Instance;//单例模式
    public Vector3 startPos;//摄像头初始位置
    public Vector3 startRot;//摄像头初始旋转
    Transform oldTransform;//旧的位置
    private bool onMove = false;//是否在移动

    void Start()
    {
        Instance = this;
        transform.position = startPos;
        transform.eulerAngles = startRot;
    }

    public void MoveCamera(Vector3 newPos, Vector3 newRot, float dTime,SmoothFuncationType type = SmoothFuncationType.Linear)//移动摄像头
    {
        oldTransform = transform;//记录旧的位置
        StartCoroutine(MoveCameraCoroutine(newPos, newRot, dTime, type));//开启协程
    }

    IEnumerator MoveCameraCoroutine(Vector3 newPos,Vector3 newRot,float dtime,SmoothFuncationType type)//移动摄像头协程
    {
        Vector3 oldPos = oldTransform.position;//记录旧的位置
        Vector3 oldRot = oldTransform.localEulerAngles;//记录旧的角度
        float timer = 0;//计时器
        onMove = true;//开始移动
        while(timer <= dtime)
        {
            timer += Time.deltaTime * 1000;//计时器加上时间
            switch (type)//缓动
            {
                case SmoothFuncationType.Linear:
                    transform.position = LinearFunction(oldPos, newPos,timer,dtime);
                    transform.localEulerAngles = LinearFunction(oldRot, newRot,timer,dtime);
                    break;
                case SmoothFuncationType.SineaseIn:
                    transform.position = SinFunctionEaseIn(oldPos, newPos, timer, dtime);
                    transform.localEulerAngles = SinFunctionEaseIn(oldRot, newRot, timer, dtime);
                    break;
                case SmoothFuncationType.SineaseOut:
                    transform.position = SinFunctionEaseOut(oldPos, newPos, timer, dtime);
                    transform.localEulerAngles = SinFunctionEaseOut(oldRot, newRot, timer, dtime);
                    break;
                case SmoothFuncationType.SineaseInOut:
                    transform.position = SinFunctionEaseInOut(oldPos, newPos, timer, dtime);
                    transform.localEulerAngles = SinFunctionEaseInOut(oldRot, newRot, timer, dtime);
                    break;
                case SmoothFuncationType.BackeaseIn:
                    transform.position = BackEaseIn(oldPos, newPos, timer, dtime);
                    transform.localEulerAngles = BackEaseIn(oldRot, newRot, timer, dtime);
                    break;

            }
            yield return null;
        }
        onMove = false;//结束移动
        yield break;
    }

    //t:当前时间 dt:持续时间 b:开始值 e:结束值
    private Vector3 LinearFunction(Vector3 b,Vector3 e,float t,float dt)//线性缓动
    {
        return b + (e - b) * t / dt;
    }

    private Vector3 SinFunctionEaseIn(Vector3 b, Vector3 e, float t, float dt)//正弦曲线的缓动（sin(t)）/ 从0开始加速的缓动，也就是先慢后快
    {
    return -(e - b) * Mathf.Cos(t/dt * (Mathf.PI/2)) + (e - b) + b;
    }

    private Vector3 SinFunctionEaseOut(Vector3 b, Vector3 e, float t, float dt)//正弦曲线的缓动（sin(t)）/ 减速到0的缓动，也就是先快后慢
    {
        return (e - b) * Mathf.Sin(t / dt * (Mathf.PI / 2)) + b;
    }

    private Vector3 SinFunctionEaseInOut(Vector3 b, Vector3 e, float t, float dt)//正弦曲线的缓动（sin(t)）/ 前半段从0开始加速，后半段减速到0的缓动
    {
        return -(e - b) / 2 * (Mathf.Cos(Mathf.PI * t / dt) - 1) + b;
    }

    private Vector3 BackEaseIn(Vector3 b, Vector3 e, float t, float dt)//超过范围的三次方缓动（(s+1)*t^3 – s*t^2）/ 从0开始加速的缓动，也就是先慢后快
    {
        float s = 1.70158f;
        return (e - b) * (t /= dt) * t * ((s + 1) * t - s) + b;
    }
}
