using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.Dialogue
{
    [ExecuteAlways]
    public class CircleContractionController : MonoBehaviour
    {
        private static readonly int RadiusID = Shader.PropertyToID("_Radius");
        private static readonly int RenderTargetSizeID = Shader.PropertyToID("_RenderTargetSize");

        public Image Image;

        private TweenerCore<float, float, FloatOptions> tweener;

        public bool IsRunning => tweener?.IsPlaying() ?? false;

        private Action onCompleteCallback;


        private void Awake()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                Image.material.SetFloat(RadiusID, 0);
#else
            Image.material.SetFloat(RadiusID, 0);
#endif
            Image.material.SetVector(RenderTargetSizeID, Image.rectTransform.sizeDelta);
        }

        //TODO: 中断当前并运行新动画

        public void Enter(float duration, Action onComplete = null)
        {
            onCompleteCallback = onComplete;
            tweener = Image.material.DOFloat(0, RadiusID, duration).SetEase(Ease.InOutExpo).OnComplete(OnComplete);
        }

        public void Exit(float duration, Action onComplete = null)
        {
            onCompleteCallback = onComplete;
            tweener = Image.material.DOFloat(1, RadiusID, duration).SetEase(Ease.InOutExpo).OnComplete(OnComplete);
        }

        public void Complete()
        {
            tweener?.Complete();
        }

        private void OnComplete()
        {
            tweener = null;
            onCompleteCallback?.Invoke();
            onCompleteCallback = null;
        }
    }
}
