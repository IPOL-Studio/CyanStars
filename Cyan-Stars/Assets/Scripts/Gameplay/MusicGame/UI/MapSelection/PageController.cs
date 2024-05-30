using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 用于控制选曲UI的页面状态（在选曲页/Staff页？）
    /// </summary>
    public class PageController : MonoBehaviour
    {
        #region
        [SerializeField]
        [Header("下一步按钮")]
        private Button nextStepButton;

        [SerializeField]
        [Header("上一步按钮")]
        private Button backButton;

        [SerializeField]
        [Header("StarsGenerator 组件")]
        private StarsGenerator starsGenerator;

        [Header("Panel.RectTransform 组件")]
        public RectTransform PanelRectTransform;

        /// <summary>
        /// 目标页面进度
        /// <para>1或2，1=选曲页，2=Staff页</para>
        /// <para>currentPage将会缓动趋近targetPage</para>
        /// </summary>
        private int targetPage = 1;

        /// <summary>
        /// 当前页面进度
        /// <para>一个介于1-2之间的小数</para>
        /// </summary>
        private float currentPage = 1;

        /// <summary>
        /// 缓动相关变量
        /// </summary>
        private const float FadeTime = 1.5f;    // 缓动持续时间
        private float startPageNum;           // 缓动开始时的页面进度
        private float startTime;           // 缓动开始时间，当 targetPage 变动时请将它设为 Time.time

        /// <summary>
        /// 获取到的 PageControlAble 组件
        /// </summary>
        private PageControlAble[] pageControlAbles;
        #endregion


        private void Start()
        {
            nextStepButton.onClick.AddListener(() =>
            {
                startTime = Time.time;
                startPageNum = currentPage;
                targetPage++;
                starsGenerator.TargetPage = targetPage;
            });
            backButton.onClick.AddListener(() =>
            {
                if (targetPage == 1)
                {
                    //  ToDo:退出选曲界面，返回游戏主页
                    Debug.LogWarning("返回主页的功能尚未实现，如果你有兴趣可以加入我们一起设计和开发，我们的链接是..[系统屏蔽广告信息]");
                }
                else
                {
                    startTime = Time.time;
                    startPageNum = currentPage;
                    targetPage--;
                    starsGenerator.TargetPage = targetPage;
                }
            });
            SetPageControlAbles();      // 更新 PageControlAbles 列表
            OnRectTransformDimensionsChange();
        }

        private void Update()
        {
            // 根据缓动函数计算 currentPage，并传递给 pageControlAbles
            if ((Time.time < startTime + FadeTime) && (targetPage != currentPage))
            {
                currentPage = EasingFunction.EaseOutQuart(startPageNum, targetPage, Time.time - startTime, FadeTime);
                foreach (PageControlAble pageControlAble in pageControlAbles)
                {
                    pageControlAble.CurrentPageProgress = currentPage;
                }
            }
        }

        public void OnOpen()
        {
            currentPage = 1;
            targetPage = 1;
            PageControlAble[] pageControlAbles = GetComponentsInChildren<PageControlAble>();
            foreach (PageControlAble pageControlAble in pageControlAbles)
            {
                pageControlAble.CurrentPageProgress = 1f;
            }
        }

        /// <summary>
        /// 获取当前组件下子节点中的 PageControlAble
        /// </summary>
        private void SetPageControlAbles()
        {
            pageControlAbles = GetComponentsInChildren<PageControlAble>();
        }

        /// <summary>
        /// Start() 或屏幕大小变化时，将新的 PanelSize 传给 PageControlAble
        /// </summary>
        private void OnRectTransformDimensionsChange()
        {
            // 将 Panel 宽高传给每一个 PageControlAble
            if (pageControlAbles == null)
            {
                return;
            }

            foreach (PageControlAble pageControlAble in pageControlAbles)
            {
                pageControlAble.PanelSize = new Vector2(PanelRectTransform.rect.width, PanelRectTransform.rect.height);
            }
        }
    }
}
