using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageController : MonoBehaviour
{
    /// <summary>
    /// 用于控制选曲UI的页面状态（在选曲页/Staff页？）
    /// 并管理各元素随页面状态的位置、透明度等属性
    /// 并更新每个Star实例的相关变量
    /// </summary>

    [Header("下一步按钮")]
    public Button NextStepButton;

    [Header("上一步按钮")]
    public Button BackButton;

    [Header("星星生成器组件")]
    public GameObject StarsGenerator;

    /// <summary>
    /// 目标页面进度
    /// 1或2，1=选曲页，2=Staff页
    /// CurrentPageProgress将会缓动趋近TargetPageProgress
    /// <summary>
    int targetPageProgress;

    /// <summary>
    /// 当前页面进度
    /// 一个介于1-2之间的小数
    /// <summary>
    float currentPageProgress;

    /// <summary>
    /// 缓动速度
    /// <summary>
    float speed = 0.05f;


    private void Start()
    {
        targetPageProgress = 1;
        currentPageProgress = 1;

        NextStepButton.onClick.AddListener(() =>
        {
            targetPageProgress++;
        });

        BackButton.onClick.AddListener(() =>
        {
            if (targetPageProgress == 1)
            {
                //  ToDo:退出选曲界面，返回游戏主页
                Debug.LogWarning("返回主页的功能尚未实现，如果你有兴趣可以加入我们一起设计和开发，我们的链接是..[系统屏蔽广告信息]");
            }
            else
            {
                targetPageProgress--;
            }
        });
    }

    private void Update()
    {
        currentPageProgress += (targetPageProgress - currentPageProgress) * speed;

        // 遍历StarsGenerator下每一个Star实例，将currentPageProgress储存于Star同名变量中
        Star[] stars = StarsGenerator.GetComponentsInChildren<Star>();
        if (targetPageProgress != currentPageProgress)
        {
            foreach (Star star in stars)
            {
                star.RefreshPageVariable(currentPageProgress);
            }
        }
    }

}
