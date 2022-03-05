using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Camera objectCamera; //需要作用的Camera

    public Vector3 defaultCameraPos;    //摄像机的默认位置

    public Vector3 newPos;  //变化后的相对位置（相对于默认位置）
    public Vector3 newRot;  //变化后的绝对角度
    public float dTime;     //变化持续时间，单位毫秒
    public bool smooth;     //是否使用三角函数缓动

    private Vector3 oldPos;  //开始变化时的相对位置
    private Vector3 oldRot;  //开始变化时的绝对角度
    private float sumTime = 0;
    private bool onChange;
    private Vector3 dPos, dRot;

    void ChangeCameraPosAndRot()
    {
        oldPos = objectCamera.transform.position - defaultCameraPos;
        oldRot = objectCamera.transform.localEulerAngles;
        dPos = newPos - oldPos;
        dRot = newRot - oldRot;
        sumTime = 0;
        onChange = true;
    }

    private void Update()
    {
        sumTime += Time.deltaTime * 1000;
        if (onChange)
        {
            if (smooth)
            {
                objectCamera.transform.position = defaultCameraPos + oldPos + new Vector3(SmoothFormula(dPos.x, dTime, sumTime), SmoothFormula(dPos.y, dTime, sumTime), SmoothFormula(dPos.z, dTime, sumTime));
                objectCamera.transform.localEulerAngles = oldRot + new Vector3(SmoothFormula(dRot.x, dTime, sumTime), SmoothFormula(dRot.y, dTime, sumTime), SmoothFormula(dRot.z, dTime, sumTime));
            }
            else
            {
                objectCamera.transform.position = defaultCameraPos + oldPos + new Vector3(StraigntFormula(dPos.x, dTime, sumTime), StraigntFormula(dPos.y, dTime, sumTime), StraigntFormula(dPos.z, dTime, sumTime));
                objectCamera.transform.localEulerAngles = oldRot + new Vector3(StraigntFormula(dRot.x, dTime, sumTime), StraigntFormula(dRot.y, dTime, sumTime), StraigntFormula(dRot.z, dTime, sumTime));
            }
            if (sumTime >= dTime) { onChange = false; } //超时后停止
        }
    }

    float SmoothFormula(float a, float dt, float x)
    {
        return (0.5f * a * Mathf.Sin(1 / dt * Mathf.PI * x - 0.5f * Mathf.PI) + 0.5f * a);
    }

    float StraigntFormula(float a, float dt, float x)
    {
        return (a / dt * x);
    }
}
