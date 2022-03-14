using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EffectType
{
    FrameBreath,
    FrameOnce,
    Particle,
}

public class EffectControllerSo : MonoBehaviour
{
    [Header("开始按钮")]
    public Button startButton;
    [Header("特效种类")]
    public List<GameObject> effectList;

    [System.Serializable]
    public class KeyFrame
    {
        [Header("种类")]
        public EffectType type;
        [Header("时间")]
        public float time;
        [Header("特效序号(仅对particle有效)")]
        public int index;
        [Header("位置(仅对particle有效)")]
        public Vector3 position;
        [Header("朝向(仅对particle有效)")]
        public Vector3 rotation;
        [Header("持续时间(仅对particle和frame.breath有效)")]
        public float duration;
        [Header("颜色(仅对frame有效)")]
        public Color color;
        [Header("强度(仅对frame有效)")]
        [Range(0,1)]public float intensity;
        [Header("衰减系数(仅对frame.once有效)")]
        [Range(0,1)]public float decay;
        [Header("播放次数(仅对frame.once有效)")]
        public int frequency;
    }
    [Header("边框")]
    public Image frame;

    [Header("BPM")]
    public float bpm;

    [Header("关键帧")]
    public List<KeyFrame> keyFrames;
    private float timer;
    private int index = 0;
    private bool isStart = false;

    void Start()
    {
        startButton.onClick.AddListener(OnBtnStartClick);
        for(var i = 0; i < keyFrames.Count; i++)
        {
            if(keyFrames[i].type == EffectType.FrameBreath)
            {
                //提前从波谷入场
                keyFrames[i].time = Mathf.Max(0,keyFrames[i].time -30000/bpm);
            }
        }
    }

    void OnBtnStartClick()
    {
        timer = 0;
        index = 0;
        isStart = true;
    }

    void Update()
    {
        timer = GameMgr.Instance.TimelineTime * 1000;
        if(!isStart)
        {
            return;
        }
        if(index < keyFrames.Count)
        {
            while(index < keyFrames.Count && timer >= keyFrames[index].time)
            {   
                if(keyFrames[index].type == EffectType.Particle)
                {
                    if(keyFrames[index].index >= effectList.Count)
                    {
                        Debug.LogError("特效序号超出范围");
                        return;
                    }
                    GameObject effectObj = Instantiate(effectList[keyFrames[index].index], 
                    keyFrames[index].position, Quaternion.Euler(keyFrames[index].rotation));
                    effectObj.gameObject.transform.SetParent(transform);
                    effectObj.GetComponent<EffectObj>().destroyTime = keyFrames[index].duration;
                }
                else if(keyFrames[index].type == EffectType.FrameOnce)
                {
                    frame.color = keyFrames[index].color;
                    frame.pixelsPerUnitMultiplier = 1 - keyFrames[index].intensity;
                    StopAllCoroutines();
                    StartCoroutine(FrameFade(keyFrames[index].decay, keyFrames[index].frequency));
                }
                else if(keyFrames[index].type == EffectType.FrameBreath)
                {
                    frame.color = keyFrames[index].color;
                    frame.pixelsPerUnitMultiplier = 1 - keyFrames[index].intensity;
                    StopAllCoroutines();
                    StartCoroutine(CosFrameFade(keyFrames[index].duration));
                }
                
                index++;
            }
        }
        else
        {
            isStart = false;
        }
    }
    IEnumerator FrameFade(float decay,int frequency = 1)
    {
        //每秒透明度减小decay
        Color color = frame.color;
        for(int i = 0;i < frequency;i++)
        {
            float alpha = 1;
            while(alpha > 0)
            {
                alpha -= decay * 0.1f;
                color.a = alpha;
                frame.color = color;
                yield return new WaitForSeconds(0.1f);
            }   
            yield return new WaitForSeconds(60/bpm);
        }
    }
    
    IEnumerator CosFrameFade(float dTime)
    {
        //使用cos函数控制透明度
        Color color = frame.color;
        float alpha = 0;
        float timePoint = timer;
        while(timer - timePoint < dTime)
        {
            alpha = 0.5f * Mathf.Cos(bpm * 2 * Mathf.PI * (timer - timePoint) / 60000 - 30000/bpm) + 0.5f;
            color.a = alpha;
            frame.color = color;
            yield return null;
        }
        yield break;
    }
}