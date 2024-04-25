using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CyanStars.Gameplay.MusicGame;

public class PageController : MonoBehaviour
{
    #region 变量
    /// <summary>
    /// 用于控制选曲UI的页面状态（在选曲页/Staff页？）
    /// </summary>
    [Header("下一步按钮")]
    public Button NextStepButton;
    [Header("上一步按钮")]
    public Button BackButton;
    [Header("Panel 组件")]
    public GameObject Panel;
    [Header("StarsGenerator 组件")]
    public StarsGenerator Generator;


    /// <summary>
    /// 目标页面进度
    /// 1或2，1=选曲页，2=Staff页
    /// currentPage将会缓动趋近targetPage
    /// <summary>
    int targetPage = 1;

    /// <summary>
    /// 当前页面进度
    /// 一个介于1-2之间的小数
    /// <summary>
    float currentPage = 1;

    /// <summary>
    /// 缓动相关变量
    /// </summary>
    const float Dt = 1.2f;    // 缓动持续时间
    float sp;           // 缓动开始时的页面进度
    float st;           // 缓动开始时间，当 targetPage 变动时请将它设为 Time.time
    #endregion

    /// <summary>
    /// 获取到的 PageControlAble 组件
    /// </summary>
    PageControlAble[] pageControlAbles;

    void Start()
    {
        NextStepButton.onClick.AddListener(() =>
        {
            st = Time.time;
            sp = currentPage;
            targetPage++;
        });
        BackButton.onClick.AddListener(() =>
        {
            if (targetPage == 1)
            {
                //  ToDo:退出选曲界面，返回游戏主页
                Debug.LogWarning("返回主页的功能尚未实现，如果你有兴趣可以加入我们一起设计和开发，我们的链接是..[系统屏蔽广告信息]");
            }
            else
            {
                st = Time.time;
                sp = currentPage;
                targetPage--;
            }
        });
        Generator.GenerateStars();  // 先生成星星预制体
        SetPageControlAbles();      // 再更新 PageControlAbles 列表
        RefreshPanelSize();
    }

    void Update()
    {
        // 根据缓动函数计算 currentPage，并传递给 pageControlAbles
        if ((Time.time < st + Dt) && (targetPage != currentPage))
        {
            currentPage = EasingFunction.SinFunctionEaseOut(sp, targetPage, Time.time - st, Dt);
            foreach (PageControlAble pageControlAble in pageControlAbles)
            {
                pageControlAble.CurrentPage = currentPage;
            }
        }
    }

    /// <summary>
    /// 获取当前组件下子节点中的 PageControlAble
    /// </summary>
    public void SetPageControlAbles()
    {
        pageControlAbles = GetComponentsInChildren<PageControlAble>();
    }

    void OnRectTransformDimensionsChange()
    {
        RefreshPanelSize();
    }

    /// <summary>
    /// Start() 或屏幕大小变化时，将新的 PanelSize 传给 PageControlAble
    /// </summary>
    void RefreshPanelSize()
    {
        // 将 Panel 宽高传给每一个 PageControlAble
        if (pageControlAbles == null)
        { return; }

        RectTransform rectTransform = Panel.GetComponent<RectTransform>();
        foreach (PageControlAble pageControlAble in pageControlAbles)
        {
            pageControlAble.PanelSize = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
        }
    }
}
