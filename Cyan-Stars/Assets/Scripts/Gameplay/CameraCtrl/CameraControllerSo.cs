using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraControllerSo : MonoBehaviour
{
    [Header("相机默认位置")]
    public Vector3 defaultPosition;

    [System.Serializable]
    public class KeyFrame
    {
        [Header("时间")]
        public float time;
        [Header("目标")]
        public Vector3 position;
        public Vector3 rotation;

        [Header("缓动方式")]
        public SmoothFuncationType smoothType;
    }

    [Header("关键帧")]
    public List<KeyFrame> keyFrames;
    
}
