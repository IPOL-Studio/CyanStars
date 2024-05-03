using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaffLabel : PageControlAble
{
    public GameObject DutyTextObj;
    public GameObject NameTextObj;

    public GameObject InfoFrame;
    public GameObject DutyFrame;
    public GameObject NameFrame;

    public int Group { get; set; }

    public void RefreshLength()
    {
        RectTransform dutyFrameRectTransform = DutyFrame.GetComponent<RectTransform>();
        RectTransform nameFrameRectTransform = NameFrame.GetComponent<RectTransform>();

        float x, y;

        x = CalculateStringWidth(DutyTextObj.GetComponentInChildren<TMP_Text>().text) + 8;
        y = dutyFrameRectTransform.sizeDelta.y;
        dutyFrameRectTransform.sizeDelta = new Vector2(x, y);

        x = CalculateStringWidth(NameTextObj.GetComponentInChildren<TMP_Text>().text) + 10;
        y = nameFrameRectTransform.sizeDelta.y;
        nameFrameRectTransform.sizeDelta = new Vector2(x, y);

        // 很神奇，计算是正确的，但是在Unity显示的时候值并没有完成赋值，所以将这一步对InfoFrame的赋值操作放在了Update()
        // 我觉得这是Unity的问题（心虚
        //RectTransform infoFrameRectTransform = InfoFrame.GetComponent<RectTransform>();
        //x = dutyFrameRectTransform.sizeDelta.x + nameFrameRectTransform.sizeDelta.x;
        //y = infoFrameRectTransform.sizeDelta.y;
        //infoFrameRectTransform.sizeDelta = new Vector2(x, y);
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

    // 判断字符是否类似于英文
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
    /// 启用或禁用StaffLabel的渲染
    /// </summary>
    public void SetRender(bool b)
    {
        Images = GetComponentsInChildren<Image>();
        TextMeshes = GetComponentsInChildren<TMP_Text>();
        foreach (var item in Images)
        {
            item.enabled = b;
        }
        foreach (var item in TextMeshes)
        {
            item.enabled = b;
        }
    }

    public override void Start()
    {
        CurrentPage = 1f;
        RectTransform = GetComponent<RectTransform>();
    }

    public override void Update()
    {
        InfoFrame.GetComponent<RectTransform>().sizeDelta = new Vector2(DutyFrame.GetComponent<RectTransform>().sizeDelta.x + NameFrame.GetComponent<RectTransform>().sizeDelta.x, InfoFrame.GetComponent<RectTransform>().sizeDelta.y);
        float deltaPage = Mathf.Abs(CurrentPage - AblePage);
        ChangeAlpha(deltaPage);
    }
}
