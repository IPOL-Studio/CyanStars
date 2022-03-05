using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Camera objectCamera; //��Ҫ���õ�Camera

    public Vector3 defaultCameraPos;    //�������Ĭ��λ��

    public Vector3 newPos;  //�仯������λ�ã������Ĭ��λ�ã�
    public Vector3 newRot;  //�仯��ľ��ԽǶ�
    public float dTime;     //�仯����ʱ�䣬��λ����
    public bool smooth;     //�Ƿ�ʹ�����Ǻ�������

    private Vector3 oldPos;  //��ʼ�仯ʱ�����λ��
    private Vector3 oldRot;  //��ʼ�仯ʱ�ľ��ԽǶ�
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
            if (sumTime >= dTime) { onChange = false; } //��ʱ��ֹͣ
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
