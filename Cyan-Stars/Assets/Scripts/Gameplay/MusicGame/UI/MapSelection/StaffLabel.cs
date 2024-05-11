using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SyanStars.Gameplay.MapSelection
{
    public class StaffLabel : MonoBehaviour
    {
        public TMP_Text DutyText;
        public TMP_Text NameText;

        [SerializeField]
        RectTransform quasiFrameRectTransform;
        [SerializeField]
        RectTransform infoFrameRectTransform;
        [SerializeField]
        RectTransform dutyFrameRectTransform;
        [SerializeField]
        RectTransform nameFrameRectTransform;

        List<Image> images = new List<Image>();
        List<TMP_Text> textMeshes = new List<TMP_Text>();

        [SerializeField]
        float gradientTime;

        /// <summary>
        /// 刷新各UI元素的大小
        /// </summary>
        public void RefreshLength()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();

            float x, y;

            x = CalculateStringWidth(DutyText.text) + 8;
            y = dutyFrameRectTransform.sizeDelta.y;
            dutyFrameRectTransform.sizeDelta = new Vector2(x, y);

            x = CalculateStringWidth(NameText.text) + 10;
            y = nameFrameRectTransform.sizeDelta.y;
            nameFrameRectTransform.sizeDelta = new Vector2(x, y);

            x = dutyFrameRectTransform.sizeDelta.x + nameFrameRectTransform.sizeDelta.x;
            y = infoFrameRectTransform.sizeDelta.y;
            infoFrameRectTransform.sizeDelta = new Vector2(x, y);

            x = infoFrameRectTransform.sizeDelta.x + quasiFrameRectTransform.sizeDelta.x;
            y = rectTransform.sizeDelta.y;
            rectTransform.sizeDelta = new Vector2(x, y);
        }

        int CalculateStringWidth(string rawText)
        {
            int sunWidth = 0;
            foreach (char c in rawText)
            {
                if (IsEnglishLike(c))
                {
                    sunWidth += 14;
                }
                else
                {
                    sunWidth += 34;
                }
            }
            return sunWidth;
        }

        /// <summary>
        /// 判断字符是否类似于英文
        /// </summary>
        bool IsEnglishLike(char c)
        {
            // 根据需求扩展英文字符集
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || // 英文字母
                   (c >= '0' && c <= '9') || // 数字
                   (c >= '\u0020' && c <= '\u007E') || // 基本符号
                   (c >= '\u2000' && c <= '\u206F') || // 常用标点
                   (c >= '\u3000' && c <= '\u303F');   // CJK标点
        }

        /// <summary>
        /// 启用或禁用 StaffLabel 的渲染
        /// </summary>
        ///<param name="isAble">是渐显(Alpha -> 1)还是渐隐(alpha -> 0)</param>
        ///<param name="isForce">不播放平滑动画，瞬间消失</param>
        public void SetRender(bool isAble, bool isForce = false)
        {
            float _gradientTime = gradientTime;
            if (isForce)
            {
                _gradientTime = 0;
            }

            if (isAble)
            {
                foreach (var item in images)
                {
                    item.enabled = true;
                    item.DOFade(1, _gradientTime).SetEase(Ease.OutQuart);
                }
                foreach (var item in textMeshes)
                {
                    item.enabled = true;
                    item.DOFade(1, _gradientTime).SetEase(Ease.OutQuart);
                }
            }
            else
            {
                foreach (var item in images)
                {
                    item.DOFade(0, _gradientTime).SetEase(Ease.OutQuart).OnComplete(() =>
                    {
                        item.enabled = false;
                    });
                }
                foreach (var item in textMeshes)
                {
                    item.DOFade(0, _gradientTime).SetEase(Ease.OutQuart).OnComplete(() =>
                    {
                        item.enabled = false;
                    });
                }
            }
        }

        void Start()
        {
            images = new List<Image>(GetComponentsInChildren<Image>());
            textMeshes = new List<TMP_Text>(GetComponentsInChildren<TMP_Text>());
            foreach (var item in images)
            {
                item.enabled = false;
            }
            foreach (var item in textMeshes)
            {
                item.enabled = false;
            }
        }
    }
}
