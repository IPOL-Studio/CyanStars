using CyanStars.EditorExtension;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Star : MonoBehaviour
    {
        [SerializeField]
        private GameObject imageObj;

        [SerializeField]
        private GameObject staffLabelObj;

        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;

        public float Alpha
        {
            get => canvasGroup.alpha;
            set => canvasGroup.alpha = Mathf.Clamp01(value);
        }

        public bool Interactable
        {
            get => canvasGroup.interactable;
            set => canvasGroup.interactable = value;
        }

        /// <summary>
        /// 这个星星可以显示 Staff 标签吗？
        /// </summary>
        public bool CanShowStaff { get; set; }

        /// <summary>
        /// 这个星星在第几组显示？
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// 位移灵敏度
        /// </summary>
        public Vector3 PosParallax { get; set; }

        /// <summary>
        /// 透明度灵敏度
        /// </summary>
        public float AlphaParallax { get; set; }

        /// <summary>
        /// 该组件在可用页时的位置比例（以左下为原点，相对于PanelWidth）
        /// </summary>
        [Active(ActiveMode.Edit)]
        public Vector3 PosRatio;

        public float EnabledAlpha { get; set; } = 1f;

        private Vector3 enabledSize;
        public Vector3 EnabledSize
        {
            get => enabledSize;
            set
            {
                enabledSize = value;
                (imageObj.transform as RectTransform).localScale = value;
            }
        }

        public StaffLabel StaffLabel { get; private set; }

        private void Awake()
        {
            rectTransform = transform as RectTransform;

            StaffLabel = staffLabelObj.GetComponent<StaffLabel>();
            canvasGroup = GetComponent<CanvasGroup>();

            EnabledSize = Vector3.one;
        }

        public void SetStaffLabelActive(bool isActive) => staffLabelObj.SetActive(isActive);

        public void UpdateFromScreenRatio(RectTransform root, float ratio)
        {
            rectTransform.localPosition = CalculatePosFormScreenRatio(root, ratio);
            Alpha = CalculateAlpha(ratio);

            if (Alpha <= 0.1f)
            {
                Interactable = false;
            }
        }

        public Vector3 CalculatePosFormScreenRatio(RectTransform root, float ratio)
        {
            var panelSize = root.rect.size;
            float x = (PosRatio.x + ratio * PosParallax.x) * panelSize.x;
            float y = PosRatio.y * panelSize.y;

            return new Vector3(x, y, PosRatio.z);
        }

        public float CalculateAlpha(float ratio)
        {
            return EnabledAlpha - AlphaParallax * ratio;
        }
    }
}
