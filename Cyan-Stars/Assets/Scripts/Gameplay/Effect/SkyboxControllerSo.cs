using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using CyanStars.Gameplay.Misc;

namespace CyanStars.Gameplay.Effect
{
    public enum TwinkleType
    {
        TwinkleOnce,
        TwinkleBreath,
    }

    public class SkyboxControllerSo : MonoBehaviour
    {
        [Header("开始按钮")] public Button startButton;

        [Header("Skybox")] public Skybox skybox;

        [Header("BPM")] public float bpm;

        [Header("默认亮度")] public float defaultBrightness;
        [Header("默认角度")] public float defaultAngle;

        [System.Serializable]
        public class TwinkleKeyFrame
        {
            [Header("时间")] public float time;
            [Header("类型")] public TwinkleType type;
            [Header("持续时间（仅对TwinkleBreath生效）")] public float duration;
            [Header("最小亮度（仅对TwinkleBreath生效）")] public float minIntensity;
            [Header("最大亮度（仅对TwinkleBreath生效）")] public float maxIntensity;
        }

        [System.Serializable]
        public class RevolveKeyFrame
        {
            [Header("时间")] public float time;
            [Header("角度")] public float angle;
            [Header("缓动类型")] public SmoothFuncationType easeType;
        }

        [Header("旋转关键帧")] public List<RevolveKeyFrame> revolveKeyFrames;

        [Header("闪烁关键帧")] public List<TwinkleKeyFrame> twinkleKeyFrames;

        private float currentTime = 0;
        private float currentAngle = 0;
        private float revolveTimer = 0;
        private int revolveIndex = 0;
        private int twinkleIndex = 0;
        private bool isStart = false;
        public bool onRevolve = false;

        void Start()
        {
            startButton.onClick.AddListener(OnStartButtonClick);
            for (var i = 0; i < twinkleKeyFrames.Count; i++)
            {
                if (twinkleKeyFrames[i].type == TwinkleType.TwinkleBreath)
                {
                    //提前从波谷入场
                    twinkleKeyFrames[i].time = Mathf.Max(0, twinkleKeyFrames[i].time - 30000 / bpm);
                }
            }
        }

        void OnStartButtonClick()
        {
            isStart = true;
            currentTime = 0;
            currentAngle = 0;
            revolveIndex = 0;
            skybox.material.SetFloat("_Rotation", defaultAngle);
            skybox.material.SetFloat("_Exposure", defaultBrightness);
        }

        void Update()
        {
            //currentTime = GameManager.Instance.TimelineTime * 1000;
            if (isStart)
            {
                if (revolveIndex < revolveKeyFrames.Count)
                {
                    if (!onRevolve)
                    {
                        StartCoroutine(Revolve(revolveKeyFrames[revolveIndex].angle,
                            revolveKeyFrames[revolveIndex].time - revolveTimer,
                            revolveKeyFrames[revolveIndex].easeType));
                        revolveIndex++;
                    }
                }

                if (twinkleIndex < twinkleKeyFrames.Count)
                {
                    if (currentTime >= twinkleKeyFrames[twinkleIndex].time)
                    {
                        StartCoroutine(TwinkleBreath(twinkleKeyFrames[twinkleIndex].minIntensity,
                            twinkleKeyFrames[twinkleIndex].maxIntensity, twinkleKeyFrames[twinkleIndex].duration));
                        twinkleIndex++;
                    }
                }
            }
        }

        IEnumerator TwinkleBreath(float minIntensity, float maxIntensity, float duration)
        {
            float timePoint = currentTime;
            while (currentTime - timePoint < duration)
            {
                float intensity =
                    (0.5f * Mathf.Cos(bpm * 2 * Mathf.PI * (currentTime - timePoint - 30000 / bpm) / 60000) + 0.5f)
                    * maxIntensity + minIntensity;
                skybox.material.SetFloat("_Exposure", intensity);
                yield return null;
            }
        }

        IEnumerator Revolve(float angle, float dTime, SmoothFuncationType easeType)
        {
            onRevolve = true;
            float timer = 0; //计时器

            while (timer <= dTime)
            {
                timer += Time.deltaTime * 1000; //计时器加上时间(ms)
                switch (easeType) //缓动
                {
                    case SmoothFuncationType.Linear:
                        skybox.material.SetFloat("_Rotation", LinearFunction(currentAngle, angle, timer, dTime));
                        break;
                    case SmoothFuncationType.SineaseIn:
                        skybox.material.SetFloat("_Rotation", SinFunctionEaseIn(currentAngle, angle, timer, dTime));
                        break;
                    case SmoothFuncationType.SineaseOut:
                        skybox.material.SetFloat("_Rotation", SinFunctionEaseOut(currentAngle, angle, timer, dTime));
                        break;
                    case SmoothFuncationType.SineaseInOut:
                        skybox.material.SetFloat("_Rotation", SinFunctionEaseInOut(currentAngle, angle, timer, dTime));
                        break;
                    case SmoothFuncationType.BackeaseIn:
                        skybox.material.SetFloat("_Rotation", BackEaseIn(currentAngle, angle, timer, dTime));
                        break;

                }

                onRevolve = false;
                yield return null;
            }

            currentAngle = angle;

            yield break;
        }

        // b:开始值  e:结束值 t:当前时间，dt:持续时间
        private float LinearFunction(float b, float e, float t, float dt) //线性匀速运动效果
        {
            return b + (e - b) * t / dt;
        }

        private float SinFunctionEaseIn(float b, float e, float t, float dt) //正弦曲线的缓动（sin(t)）/ 从0开始加速的缓动，也就是先慢后快
        {
            return -(e - b) * Mathf.Cos(t / dt * (Mathf.PI / 2)) + (e - b) + b;
        }

        private float SinFunctionEaseOut(float b, float e, float t, float dt) //正弦曲线的缓动（sin(t)）/ 减速到0的缓动，也就是先快后慢
        {
            return (e - b) * Mathf.Sin(t / dt * (Mathf.PI / 2)) + b;
        }

        private float SinFunctionEaseInOut(float b, float e, float t, float dt) //正弦曲线的缓动（sin(t)）/ 前半段从0开始加速，后半段减速到0的缓动
        {
            return -(e - b) / 2 * (Mathf.Cos(Mathf.PI * t / dt) - 1) + b;
        }

        private float BackEaseIn(float b, float e, float t, float dt) //超过范围的三次方缓动（(s+1)*t^3 – s*t^2）/ 从0开始加速的缓动，也就是先慢后快
        {
            float s = 1.70158f;
            return (e - b) * (t /= dt) * t * ((s + 1) * t - s) + b;
        }
    }
}
