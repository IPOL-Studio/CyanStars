using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    Transform oldTransform;
    public List<Vector3> cameraPositions;
    public List<Vector3> cameraRotations;
    public List<float> cameraDTimes;
    public List<SmoothFuncationType> cameraSmoothTypes;
    public bool onMove = false;
    int i = 0;
    void Update()
    {
        if(!onMove && i < cameraPositions.Count)
        {
            moveCamera(cameraPositions[i],cameraRotations[i],cameraDTimes[i],cameraSmoothTypes[i]);
            i++;
        }
    }
    public void moveCamera(Vector3 newPos, Vector3 newRot, float dTime,SmoothFuncationType type = SmoothFuncationType.Linear)
    {
        oldTransform = transform;
        StartCoroutine(moveCameraCoroutine(newPos, newRot, dTime, type));
    }
    IEnumerator moveCameraCoroutine(Vector3 newPos,Vector3 newRot,float dtime,SmoothFuncationType type)
    {
        Vector3 oldPos = oldTransform.position;
        Vector3 oldRot = oldTransform.localEulerAngles;
        float timer = 0;
        onMove = true;
        while(timer <= dtime)
        {
            timer += Time.deltaTime;
            switch (type)
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
        onMove = false;
        yield break;
    }
    //t:当前时间，d:持续时间 b:开始值 e:结束值
    private Vector3 LinearFunction(Vector3 b,Vector3 e,float t,float dt)
    {
        return b + (e - b) * t / dt;
    }
    private Vector3 SinFunctionEaseIn(Vector3 b, Vector3 e, float t, float dt)
    {
    return -(e - b) * Mathf.Cos(t/dt * (Mathf.PI/2)) + (e - b) + b;
    }
    private Vector3 SinFunctionEaseOut(Vector3 b, Vector3 e, float t, float dt)
    {
        return (e - b) * Mathf.Sin(t / dt * (Mathf.PI / 2)) + b;
    }
    private Vector3 SinFunctionEaseInOut(Vector3 b, Vector3 e, float t, float dt)
    {
        return -(e - b) / 2 * (Mathf.Cos(Mathf.PI * t / dt) - 1) + b;
    }
    private Vector3 BackEaseIn(Vector3 b, Vector3 e, float t, float dt)
    {
        float s = 1.70158f;
        return (e - b) * (t /= dt) * t * ((s + 1) * t - s) + b;
    }
}
