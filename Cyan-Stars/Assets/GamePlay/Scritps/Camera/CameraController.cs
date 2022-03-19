using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*  使用说明：
*  将此脚本挂载在需要改变位置的Camera下
*  调用一次MoveCamera(Vector3 newPos, Vector3 newRot, float dTime,SmoothFuncationType type = SmoothFuncationType.Linear)
*  newPos为移动后的位置
*  newRot为移动后的朝向
*  dTime为移动，单位毫秒
*  type见SmoothFuncationType.cs
*/

public class CameraController : MonoBehaviour//摄像头控制脚本
{
    //Transform oldTransform;//旧的位置
    public Vector3 oldPos;
    public Vector3 oldRot;
    public bool onMove = false;//是否在移动
    public Vector3 defaultPosition;//默认位置
    void Awake()
    {
        transform.position = defaultPosition;//设置默认位置
        
        oldPos = transform.position;//记录旧位置
        oldRot = transform.eulerAngles;//设置旧的位置
    }
    public void MoveCamera(Vector3 newPos, Vector3 newRot, float dTime,SmoothFuncationType type = SmoothFuncationType.Linear)//移动摄像头
    {
        if(onMove)return;//如果正在移动，则返回

        onMove = true;//开始移动

        StartCoroutine(MoveCameraCoroutine(newPos, newRot, dTime, type));//开启协程
    }
    IEnumerator MoveCameraCoroutine(Vector3 newPos,Vector3 newRot,float dtime,SmoothFuncationType type)//移动摄像头协程
    {
        newPos = defaultPosition + newPos;//相对于默认位置的位置

        float timer = 0;//计时器

        while(timer <= dtime)
        {
            timer += Time.deltaTime * 1000;//计时器加上时间(ms)
            switch (type)//缓动
            {
                case SmoothFuncationType.Linear:
                    transform.position = SmoothFuncation.LinearFunction(oldPos, newPos,timer,dtime);
                    transform.localEulerAngles = SmoothFuncation.LinearFunction(oldRot, newRot,timer,dtime);
                    break;
                case SmoothFuncationType.SineaseIn:
                    transform.position = SmoothFuncation.SinFunctionEaseIn(oldPos, newPos, timer, dtime);
                    transform.localEulerAngles = SmoothFuncation.SinFunctionEaseIn(oldRot, newRot, timer, dtime);
                    break;
                case SmoothFuncationType.SineaseOut:
                    transform.position = SmoothFuncation.SinFunctionEaseOut(oldPos, newPos, timer, dtime);
                    transform.localEulerAngles = SmoothFuncation.SinFunctionEaseOut(oldRot, newRot, timer, dtime);
                    break;
                case SmoothFuncationType.SineaseInOut:
                    transform.position = SmoothFuncation.SinFunctionEaseInOut(oldPos, newPos, timer, dtime);
                    transform.localEulerAngles = SmoothFuncation.SinFunctionEaseInOut(oldRot, newRot, timer, dtime);
                    break;
                case SmoothFuncationType.BackeaseIn:
                    transform.position = SmoothFuncation.BackEaseIn(oldPos, newPos, timer, dtime);
                    transform.localEulerAngles = SmoothFuncation.BackEaseIn(oldRot, newRot, timer, dtime);
                    break;

            }
            yield return null;
        }
        onMove = false;

        oldPos = newPos;//记录旧的位置
        oldRot = newRot;//记录旧的角度

        yield break;
    }
}
