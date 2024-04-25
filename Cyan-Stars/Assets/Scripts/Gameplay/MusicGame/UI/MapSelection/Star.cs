using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Star : MonoBehaviour
{
    // Star 用于控制每个星星预制体的属性和运动

    /// <summary>
    /// 实例被StarsGenerator创建后由其调用InitializeProperties()传入一些固定变量，此变量不再改变
    /// 涉及到屏幕动态缩放（由Panel处理）、页面进度（由PageController调用）等改变后触发特定方法，改变相关变量
    /// Update()根据固定变量和相关变量计算每个实例的实时位置
    /// </summary>

    // 这些变量在预制体被克隆后从外部设置，随后始终固定不变
    GameObject panel;
    float xPos;     // 初始x坐标比例，介于0-2，以一个屏幕宽度为单位，超出1的部分随页面进度向左切入屏幕
    float yPos;     // 介于0-1，以一个屏幕高度为单位
    float alpha;
    float size;
    float parallax; // 视差灵敏度，灵敏度越大，随页面进度位移越大

    // 这些变量将会变化
    float panelWidth;
    float panelHeight;
    float currentPageProgress = 1;

    // 创建物体后调用此方法，以初始化固有变量
    public void InitializeProperties(GameObject _panel, float _xPos, float _yPos, float _alpha, float _size, float _parallax)
    {
        panel = _panel;
        xPos = _xPos;
        yPos = _yPos;
        alpha = _alpha;
        size = _size;
        parallax = _parallax;

        // 根据传入的参数初始化颜色和大小
        Image image = this.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, alpha);
        RectTransform rectTransform = this.GetComponent<RectTransform>();
        rectTransform.localScale = new Vector3(size, size);
        RefreshPanelVariable();    // 初始化后刷新一次Panel相关变量
    }

    void OnRectTransformDimensionsChange()
    {
        // RectTransform（游戏窗口）大小改变时刷新位置
        RefreshPanelVariable();
    }

    void RefreshPanelVariable()
    {
        // 根据 Panel 长宽（在运行时改变分辨率的情况下）修改变量
        RectTransform panelRectTransform = panel.GetComponent<RectTransform>();
        panelWidth = panelRectTransform.rect.width;
        panelHeight = panelRectTransform.rect.height;
    }

    public void RefreshPageVariable(float _currentPageProgress)
    {
        currentPageProgress = _currentPageProgress;
    }

    private void Update()
    {
        // 确定最终在屏幕上的位置
        RectTransform rectTransform = this.GetComponent<RectTransform>();
        float screenXPos = xPos * panelWidth - parallax * ((currentPageProgress - 1) * panelWidth);
        float screenYPos = yPos * panelHeight;
        rectTransform.localPosition = new Vector3(screenXPos, screenYPos);
    }
}
