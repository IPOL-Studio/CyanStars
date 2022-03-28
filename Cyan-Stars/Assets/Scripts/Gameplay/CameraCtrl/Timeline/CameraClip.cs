using System.Collections;
using System.Collections.Generic;
using CatTimeline;
using UnityEngine;

/// <summary>
/// 相机片段
/// </summary>
public class CameraClip : BaseClip
{
    private Vector3 position;
    private Vector3 rotation;
    private SmoothFuncationType smoothType;

    private Transform camTrans;
    private float length;
    
    private Vector3 newPos;
    private Vector3 newRot;
    private Vector3 oldPos;
    private Vector3 oldRot;

    public CameraClip(float startTime, float endTime, Vector3 position, Vector3 rotation, SmoothFuncationType smoothType, Transform camTrans) : base(startTime, endTime)
    {
        this.position = position;
        this.rotation = rotation;
        this.smoothType = smoothType;
        this.camTrans = camTrans;
    }

    public override void OnEnter()
    {
        length = EndTime - StartTime;
        
        newPos = ((CameraTrack) Owner).DefaultCameraPos + position;
        newRot = rotation;
        
        oldPos = camTrans.position;
        oldRot = camTrans.eulerAngles;
    }
    
    public override void Update(float currentTime, float previousTime)
    {
        float localTimer = currentTime - StartTime;
        
        switch (smoothType)//缓动
        {
            case SmoothFuncationType.Linear:
                camTrans.position = SmoothFuncation.LinearFunction(oldPos, newPos,localTimer,length);
                camTrans.localEulerAngles = SmoothFuncation.LinearFunction(oldRot, newRot,localTimer,length);
                break;
            case SmoothFuncationType.SineaseIn:
                camTrans.position = SmoothFuncation.SinFunctionEaseIn(oldPos, newPos, localTimer, length);
                camTrans.localEulerAngles = SmoothFuncation.SinFunctionEaseIn(oldRot, newRot, localTimer, length);
                break;
            case SmoothFuncationType.SineaseOut:
                camTrans.position = SmoothFuncation.SinFunctionEaseOut(oldPos, newPos, localTimer, length);
                camTrans.localEulerAngles = SmoothFuncation.SinFunctionEaseOut(oldRot, newRot, localTimer, length);
                break;
            case SmoothFuncationType.SineaseInOut:
                camTrans.position = SmoothFuncation.SinFunctionEaseInOut(oldPos, newPos, localTimer, length);
                camTrans.localEulerAngles = SmoothFuncation.SinFunctionEaseInOut(oldRot, newRot, localTimer, length);
                break;
            case SmoothFuncationType.BackeaseIn:
                camTrans.position = SmoothFuncation.BackEaseIn(oldPos, newPos, localTimer, length);
                camTrans.localEulerAngles = SmoothFuncation.BackEaseIn(oldRot, newRot, localTimer, length);
                break;
        }
    }


}
