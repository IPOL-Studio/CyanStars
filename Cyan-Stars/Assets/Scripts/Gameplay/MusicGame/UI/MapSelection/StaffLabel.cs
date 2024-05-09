using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaffLabel : MonoBehaviour
{
    public GameObject DutyTextObj;
    public GameObject NameTextObj;

    public GameObject QuasiFrame;
    public GameObject InfoFrame;
    public GameObject DutyFrame;
    public GameObject NameFrame;

    public Image[] Images;
    public TMP_Text[] TextMeshes;

    public float GradientTime;

    /// <summary>
    /// 刷新各UI元素的大小
    /// </summary>
    public void RefreshLength()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        RectTransform dutyFrameRectTransform = DutyFrame.GetComponent<RectTransform>();
        RectTransform nameFrameRectTransform = NameFrame.GetComponent<RectTransform>();
        RectTransform infoFrameRectTransform = InfoFrame.GetComponent<RectTransform>();
        RectTransform quasiFrameRectTransform = QuasiFrame.GetComponent<RectTransform>();

        float x, y;

        x = CalculateStringWidth(DutyTextObj.GetComponentInChildren<TMP_Text>().text) + 8;
        y = dutyFrameRectTransform.sizeDelta.y;
        dutyFrameRectTransform.sizeDelta = new Vector2(x, y);

        x = CalculateStringWidth(NameTextObj.GetComponentInChildren<TMP_Text>().text) + 10;
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
        float gradientTime = GradientTime;
        if (isForce)
        { gradientTime = 0; }

        if (isAble)
        {
            foreach (var item in Images)
            {
                item.enabled = true;
                item.DOFade(1, gradientTime).SetEase(Ease.OutQuart);
            }
            foreach (var item in TextMeshes)
            {
                item.enabled = true;
                item.DOFade(1, gradientTime).SetEase(Ease.OutQuart);
            }
        }
        else
        {
            foreach (var item in Images)
            {
                item.DOFade(0, gradientTime).SetEase(Ease.OutQuart).OnComplete(() =>
                {
                    item.enabled = false;
                });
            }
            foreach (var item in TextMeshes)
            {
                item.DOFade(0, gradientTime).SetEase(Ease.OutQuart).OnComplete(() =>
                {
                    item.enabled = false;
                });
            }
        }
    }

    void Start()
    {
        Images = GetComponentsInChildren<Image>();
        TextMeshes = GetComponentsInChildren<TMP_Text>();
        foreach (var item in Images)
        { item.enabled = false; }
        foreach (var item in TextMeshes)
        { item.enabled = false; }
    }
}
