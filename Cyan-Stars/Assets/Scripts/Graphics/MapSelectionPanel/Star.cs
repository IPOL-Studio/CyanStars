using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Star : MonoBehaviour
{
    // Star 用于控制每个星星的属性和运动
    [Header("Debug, readonly")]
    [Tooltip("用作基准缩放长宽的 Panel")]
    public GameObject Panel;
    [Tooltip("横坐标比例（相对于屏幕宽度）")]
    public float XPos;
    [Tooltip("纵坐标比例（相对于屏幕高度）")]
    public float YPos;
    [Tooltip("星星透明度")]
    public float Alpha;
    [Tooltip("大小")]
    public float Size;
    [Tooltip("由视差引起的位移（以1倍屏幕宽度为单位）")]
    public float Parallax;
    [Tooltip("得到的Panel宽度")]
    public float PanelWidth;
    [Tooltip("得到的Panel高度")]
    public float PanelHeight;

    void Start()
    {
        // 根据传入的参数初始化颜色和大小
        Image image = this.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, Alpha);
        RectTransform rectTransform = this.GetComponent<RectTransform>();
        rectTransform.localScale = new Vector3(Size, Size);
    }

    void OnRectTransformDimensionsChange()
    {
        // 根据 Panel 长宽（在运行时改变分辨率的情况下）、视差灵敏度（ToDo）、动画进度（ToDo）动态调整横向位置
        RectTransform panelRectTransform = Panel.GetComponent<RectTransform>();
        PanelWidth = panelRectTransform.rect.width;
        PanelHeight = panelRectTransform.rect.height;
        RectTransform rectTransform = this.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(XPos * PanelWidth, YPos * PanelHeight);
    }
}
