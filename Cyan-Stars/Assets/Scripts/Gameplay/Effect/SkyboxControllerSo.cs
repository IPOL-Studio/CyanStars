using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        [Header("开始按钮")]
        public Button StartButton;

        [Header("Skybox")]
        public Skybox Skybox;

        [Header("BPM")]
        public float BPM;

        [Header("默认亮度")]
        public float DefaultBrightness;

        [Header("默认角度")]
        public float DefaultAngle;

        [System.Serializable]
        public class TwinkleKeyFrame
        {
            [Header("时间")]
            public float Time;

            [Header("类型")]
            public TwinkleType Type;

            [Header("持续时间（仅对TwinkleBreath生效）")]
            public float Duration;

            [Header("最小亮度（仅对TwinkleBreath生效）")]
            public float MinIntensity;

            [Header("最大亮度（仅对TwinkleBreath生效）")]
            public float MaxIntensity;
        }

        [System.Serializable]
        public class RevolveKeyFrame
        {
            [Header("时间")]
            public float Time;

            [Header("角度")]
            public float Angle;

            [Header("缓动类型")]
            public EasingFunctionType EaseType;
        }

        [Header("旋转关键帧")]
        public List<RevolveKeyFrame> RevolveKeyFrames;

        [Header("闪烁关键帧")]
        public List<TwinkleKeyFrame> TwinkleKeyFrames;

        private float currentTime = 0;
        private float currentAngle = 0;
        private float revolveTimer = 0;
        private int revolveIndex = 0;
        private int twinkleIndex = 0;
        private bool isStart = false;
        public bool IsRevolving = false;

        void Start()
        {
            StartButton.onClick.AddListener(OnStartButtonClick);
            for (var i = 0; i < TwinkleKeyFrames.Count; i++)
            {
                if (TwinkleKeyFrames[i].Type == TwinkleType.TwinkleBreath)
                {
                    //提前从波谷入场
                    TwinkleKeyFrames[i].Time = Mathf.Max(0, TwinkleKeyFrames[i].Time - 30000 / BPM);
                }
            }
        }

        void OnStartButtonClick()
        {
            isStart = true;
            currentTime = 0;
            currentAngle = 0;
            revolveIndex = 0;
            Skybox.material.SetFloat("_Rotation", DefaultAngle);
            Skybox.material.SetFloat("_Exposure", DefaultBrightness);
        }

        void Update()
        {
            //currentTime = GameManager.Instance.TimelineTime * 1000;
            if (isStart)
            {
                if (revolveIndex < RevolveKeyFrames.Count)
                {
                    if (!IsRevolving)
                    {
                        StartCoroutine(Revolve(RevolveKeyFrames[revolveIndex].Angle,
                            RevolveKeyFrames[revolveIndex].Time - revolveTimer,
                            RevolveKeyFrames[revolveIndex].EaseType));
                        revolveIndex++;
                    }
                }

                if (twinkleIndex < TwinkleKeyFrames.Count)
                {
                    if (currentTime >= TwinkleKeyFrames[twinkleIndex].Time)
                    {
                        StartCoroutine(TwinkleBreath(TwinkleKeyFrames[twinkleIndex].MinIntensity,
                            TwinkleKeyFrames[twinkleIndex].MaxIntensity, TwinkleKeyFrames[twinkleIndex].Duration));
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
                    (0.5f * Mathf.Cos(BPM * 2 * Mathf.PI * (currentTime - timePoint - 30000 / BPM) / 60000) + 0.5f)
                    * maxIntensity + minIntensity;
                Skybox.material.SetFloat("_Exposure", intensity);
                yield return null;
            }
        }

        IEnumerator Revolve(float angle, float dTime, EasingFunctionType easeType)
        {
            IsRevolving = true;
            float timer = 0; //计时器

            while (timer <= dTime)
            {
                timer += Time.deltaTime * 1000; //计时器加上时间(ms)
                switch (easeType) //缓动
                {
                    case EasingFunctionType.Linear:
                        Skybox.material.SetFloat("_Rotation", LinearFunction(currentAngle, angle, timer, dTime));
                        break;
                    case EasingFunctionType.SineaseIn:
                        Skybox.material.SetFloat("_Rotation", SinFunctionEaseIn(currentAngle, angle, timer, dTime));
                        break;
                    case EasingFunctionType.SineaseOut:
                        Skybox.material.SetFloat("_Rotation", SinFunctionEaseOut(currentAngle, angle, timer, dTime));
                        break;
                    case EasingFunctionType.SineaseInOut:
                        Skybox.material.SetFloat("_Rotation", SinFunctionEaseInOut(currentAngle, angle, timer, dTime));
                        break;
                    case EasingFunctionType.BackeaseIn:
                        Skybox.material.SetFloat("_Rotation", BackEaseIn(currentAngle, angle, timer, dTime));
                        break;
                }

                IsRevolving = false;
                yield return null;
            }

            currentAngle = angle;
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
