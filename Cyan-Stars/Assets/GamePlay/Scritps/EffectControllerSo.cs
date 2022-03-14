using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectControllerSo : MonoBehaviour
{
    public Button startButton;

    [System.Serializable]
    public class KeyFrame
    {
        [Header("时间")]
        public float time;
        [Header("特效")]
        public GameObject effect;
        [Header("位置")]
        public Vector3 position;
    }

    [Header("关键帧")]
    public List<KeyFrame> keyFrames;
    private float timer;
    private int index = 0;
    private bool isStart = false;

    void Start()
    {
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
        
    }
}
