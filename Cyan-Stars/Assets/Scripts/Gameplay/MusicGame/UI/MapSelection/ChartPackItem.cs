#nullable enable

using System;
using CyanStars.Framework.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CyanStars
{
    public class ChartPackItem : BaseUIItem
    {
        [SerializeField]
        private Button button = null!;

        [SerializeField]
        private RawImage coverImage = null!;

        [SerializeField]
        private TMP_Text titleText = null!;


        /// <summary>
        /// 从对象池获取此物体后立刻调用此方法来初始化和绑定
        /// </summary>
        public void Init(Texture2D? coverTexture, string titleString, UnityAction clickedAction)
        {
            coverImage.texture = coverTexture;
            titleText.text = titleString;
            button.onClick.AddListener(clickedAction);
        }

        public override void OnRelease()
        {
            button.onClick.RemoveAllListeners();
            base.OnRelease();
        }


        public void SetButtonXPos(float xPosRatio)
        {
            if (xPosRatio < 0 || 1 < xPosRatio)
                throw new ArgumentOutOfRangeException($"{nameof(xPosRatio)} 应当介于 0~1，当前为 {xPosRatio}。");

            // 获取容器宽度
            var frameRect = (RectTransform)transform;
            float frameWeight = frameRect.rect.width;

            // 获取按钮宽度
            var buttonRect = (RectTransform)button.transform;
            float buttonWeight = buttonRect.rect.width;

            // 计算按钮横向位置
            if (buttonWeight <= frameWeight)
            {
                buttonRect.offsetMin = new Vector2(0, buttonRect.offsetMin.y);
                return;
            }

            var deltaWeight = frameWeight - buttonWeight;
            var offsetX = deltaWeight * xPosRatio;
            buttonRect.offsetMin = new Vector2(offsetX, buttonRect.offsetMin.y);
        }
    }
}
