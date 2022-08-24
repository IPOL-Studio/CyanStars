using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.Dialogue
{
    public class CircleContractionController : MonoBehaviour
    {
        private static readonly int SliderID = Shader.PropertyToID("_Slider");

        public Image Image;

        private TweenerCore<float, float, FloatOptions> tweener;

        public bool IsRunning => tweener?.IsPlaying() ?? false;

        private Action onCompleteCallback;


        private void Awake()
        {
            Image.material.SetFloat(SliderID, 0);
        }

        //TODO: 中断当前并运行新动画

        public void Enter(float duration, Action onComplete = null)
        {
            onCompleteCallback = onComplete;
            tweener = Image.material.DOFloat(0, SliderID, duration).SetEase(Ease.InOutExpo).OnComplete(OnComplete);
        }

        public void Exit(float duration, Action onComplete = null)
        {
            onCompleteCallback = onComplete;
            tweener = Image.material.DOFloat(1, SliderID, duration).SetEase(Ease.InOutExpo).OnComplete(OnComplete);
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
