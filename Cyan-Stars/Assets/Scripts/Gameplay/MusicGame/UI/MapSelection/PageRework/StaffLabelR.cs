using DG.Tweening;
using TMPro;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    [RequireComponent(typeof(CanvasGroup))]
    public class StaffLabelR : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text dutyText;
        [SerializeField]
        private TMP_Text nameText;

        [SerializeField]
        private RectTransform quasiFrameRectTransform;
        [SerializeField]
        private RectTransform infoFrameRectTransform;
        [SerializeField]
        private RectTransform dutyFrameRectTransform;
        [SerializeField]
        private RectTransform nameFrameRectTransform;

        public RectTransform CollisionArea;

        [SerializeField]
        private float gradientTime;


        private CanvasGroup canvasGroup;

        public string DutyText
        {
            get => dutyText.text;
            set => dutyText.text = value;
        }

        public string NameText
        {
            get => nameText.text;
            set => nameText.text = value;
        }

        /// <summary>
        /// 刷新各UI元素的大小
        /// </summary>
        public void RefreshLength()
        {
            RectTransform rectTransform = transform as RectTransform;

            float x, y;

            x = CalculateStringWidth(DutyText) + 8;
            y = dutyFrameRectTransform.sizeDelta.y;
            dutyFrameRectTransform.sizeDelta = new Vector2(x, y);

            x = CalculateStringWidth(NameText) + 10;
            y = nameFrameRectTransform.sizeDelta.y;
            nameFrameRectTransform.sizeDelta = new Vector2(x, y);

            x = dutyFrameRectTransform.sizeDelta.x + nameFrameRectTransform.sizeDelta.x;
            y = infoFrameRectTransform.sizeDelta.y;
            infoFrameRectTransform.sizeDelta = new Vector2(x, y);

            x = infoFrameRectTransform.sizeDelta.x + quasiFrameRectTransform.sizeDelta.x;
            y = rectTransform.sizeDelta.y;
            rectTransform.sizeDelta = new Vector2(x, y);
        }

        public void SetRender(bool isAble, bool isFade)
        {
            if (isAble)
            {
                gameObject.SetActive(true);
                if (isFade)
                {
                    canvasGroup.alpha = 0;
                    canvasGroup.DOFade(1, gradientTime).SetEase(Ease.OutQuart);
                }
                else
                {
                    canvasGroup.alpha = 1;
                }
            }
            else
            {
                if (isFade)
                {
                    canvasGroup.DOFade(0, gradientTime).SetEase(Ease.OutQuart).OnComplete(() => gameObject.SetActive(false));
                }
                else
                {
                    canvasGroup.alpha = 0;
                    gameObject.SetActive(false);
                }
            }
        }

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 判断字符是否类似于英文
        /// </summary>
        private bool IsEnglishLike(char c)
        {
            // 根据需求扩展英文字符集
            return char.IsLetterOrDigit(c) || char.IsUpper(c) || // 英文字母或数字
                   (c >= '\u0020' && c <= '\u007E') || // 基本符号
                   (c >= '\u2000' && c <= '\u206F') || // 常用标点
                   (c >= '\u3000' && c <= '\u303F');   // CJK标点
        }

        private int CalculateStringWidth(string rawText)
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
    }
}
