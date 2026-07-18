#nullable enable

using CyanStars.Utils.RadioButton;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor
{
    public class SwitchButton : RadioButtonItem
    {
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
            SetView(IsChecked);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            OnValueChanged.AddListener(SetView);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnValueChanged.RemoveListener(SetView);
        }

        private void SetView(bool isSelected)
        {
            if (isSelected)
            {
                fillImage.gameObject.SetActive(true);
                ((RectTransform)fillImage.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillFrameRect.rect.width);

                RectTransform handleRect = handleImage.rectTransform;
                Vector2 anchoredPos = handleRect.anchoredPosition;
                anchoredPos.x = handleFrameRect.rect.width;
                handleRect.anchoredPosition = anchoredPos;
            }
            else
            {
                ((RectTransform)fillImage.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
                fillImage.gameObject.SetActive(false);

                RectTransform handleRect = handleImage.rectTransform;
                Vector2 anchoredPos = handleRect.anchoredPosition;
                anchoredPos.x = 0;
                handleRect.anchoredPosition = anchoredPos;
            }
        }
    }
}
