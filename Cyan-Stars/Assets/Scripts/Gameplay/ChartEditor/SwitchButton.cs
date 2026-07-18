#nullable enable

using CyanStars.Utils.RadioButton;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor
{
    public class SwitchButton : RadioButtonItem
    {
        [SerializeField]
        private float tweenDuration = 0.15f;

        [SerializeField]
        private Ease tweenEase = Ease.OutCubic;

        [SerializeField]
        private RectTransform fillFrameRect = null!;

        [SerializeField]
        private Image fillImage = null!;

        [SerializeField]
        private RectTransform handleFrameRect = null!;

        [SerializeField]
        private Image handleImage = null!;


        protected override void Awake()
        {
            base.Awake();
            SetView(IsChecked, false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            OnValueChanged.AddListener(OnValueChangedCallback);
        }

        protected override void OnDisable()
        {
            OnValueChanged.RemoveListener(OnValueChangedCallback);
            fillImage.rectTransform.DOKill();
            handleImage.rectTransform.DOKill();
            base.OnDisable();
        }

        private void OnValueChangedCallback(bool isSelected)
        {
            SetView(isSelected, true);
        }

        private void SetView(bool isSelected, bool animated)
        {
            fillImage.rectTransform.DOKill();
            handleImage.rectTransform.DOKill();

            float targetFillWidth = isSelected ? fillFrameRect.rect.width : 0f;
            float targetHandlePosX = isSelected ? handleFrameRect.rect.width : 0f;

            if (animated)
            {
                if (isSelected)
                    fillImage.gameObject.SetActive(true);

                DOTween.To(
                        () => fillImage.rectTransform.rect.width,
                        x => fillImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x),
                        targetFillWidth,
                        tweenDuration
                    )
                    .SetTarget(fillImage.rectTransform)
                    .SetEase(tweenEase)
                    .OnComplete(() =>
                    {
                        if (!isSelected)
                            fillImage.gameObject.SetActive(false);
                    });

                handleImage.rectTransform.DOAnchorPosX(targetHandlePosX, tweenDuration)
                    .SetEase(tweenEase);
            }
            else
            {
                fillImage.gameObject.SetActive(isSelected);
                fillImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetFillWidth);

                Vector2 anchoredPos = handleImage.rectTransform.anchoredPosition;
                anchoredPos.x = targetHandlePosX;
                handleImage.rectTransform.anchoredPosition = anchoredPos;
            }
        }
    }
}
