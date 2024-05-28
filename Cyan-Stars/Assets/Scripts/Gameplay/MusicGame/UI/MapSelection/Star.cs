using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 用于控制每个星星预制体的属性和运动
    /// Star 较其他 PageControlAble 变化较大
    /// </summary>
    public class Star : PageControlAble
    {
        [SerializeField]
        private GameObject imageObj;
        [SerializeField]
        private GameObject staffLabelObj;

        /// <summary>
        /// 这个星星可以显示 Staff 标签吗？
        /// </summary>
        public bool CanShowStaff { get; set; }

        /// <summary>
        /// 这个星星在第几组显示？
        /// </summary>
        public int Group { get; set; }

        public StaffLabel StaffLabel { get; private set; }

        public override void Start()
        {
            CurrentPageProgress = 1f;
            RectTransform = GetComponent<RectTransform>();
            imageObj.GetComponent<RectTransform>().localScale = Size;
            Images = imageObj.GetComponentsInChildren<Image>();

            StaffLabel = staffLabelObj.GetComponent<StaffLabel>();
        }

        public override void Update()
        {
            float xPos = (PosRatio.x + (CurrentPageProgress - 1) * PosParallax.x) * PanelSize.x;
            float yPos = PosRatio.y * PanelSize.y;
            RectTransform.localPosition = new Vector3(xPos, yPos, PosRatio.z);
            float deltaPage = Mathf.Abs(CurrentPageProgress - AblePage);
            ChangeAlpha(deltaPage);
        }

        public void SetStaffLabelActive(bool active)
        {
            staffLabelObj.SetActive(active);
        }
    }
}
