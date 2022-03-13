using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraControllerSo : MonoBehaviour
{
    [Header("相机")]
    public CameraController cameraController;
    [Header("相机默认位置")]
    public Vector3 defaultPosition;

    [Header("开始按钮")]
    public Button startButton;

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
    private float timer;
    private int index = 0;
    private bool isStart = false;

    void Start()
    {
        cameraController.defaultPosition = defaultPosition;
        startButton.onClick.AddListener(OnBtnStartClick);
    }
    void OnBtnStartClick()
    {
        timer = 0;
        index = 0;
        isStart = true;
    }
    void Update()
    {
        if(isStart && !cameraController.onMove)
        {
            if(index < keyFrames.Count)
            {
                cameraController.MoveCamera(keyFrames[index].position, keyFrames[index].rotation,
                keyFrames[index].time - timer, keyFrames[index].smoothType);
                timer = keyFrames[index++].time;
            }
        }
    }
}
