using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 用于控制选曲面板背景星星和 Staff 信息的生成、移动。
/// </summary>
public class StarsGenerator : MonoBehaviour
{
    #region 策划参数
    [Header("图像配置 Image settings")]
    [Tooltip("星星预制体")]
    public GameObject StarPrefab;

    [Header("数值与UI配置 Value & UI settings")]
    [Tooltip("至少生成几个星星")]
    public int MinStarNum;
    [Tooltip("至多生成几个星星")]
    public int MaxStarNum;
    [Tooltip("星星的透明度至少为")]
    public float MinStarAlpha;
    [Tooltip("星星的透明度至多为")]
    public float MaxStarAlpha;
    [Tooltip("星星的大小至少为")]
    public float MinStarSize;
    [Tooltip("星星的大小至多为")]
    public float MaxStarSize;
    [Tooltip("由视差引起的位移至少为（以1倍屏幕宽度为单位）")]
    public float MinStarParallax;
    [Tooltip("由视差引起的位移至多为（以1倍屏幕宽度为单位）")]
    public float MaxStarParallax;
    #endregion

    List<Star> canShowStaffList = new List<Star>();

    /// <summary>
    /// 按照在 Unity 中配置的参数开始生成星星预制体
    /// 并将适合展示 Staff 的星星添加到列表内
    /// </summary>
    public void GenerateStars()
    {
        int starNum = Random.Range(MinStarNum, MaxStarNum);

        for (int i = 0; i < starNum; i++)
        {
            // 随机生成位置、透明度、大小、视差灵敏度，并传递给每一个 Star 预制体
            GameObject newStarObject = Instantiate(StarPrefab, transform);
            Star newStar = newStarObject.GetComponent<Star>();
            float xPos = Random.Range(0f, MaxStarParallax + 1);
            float yPos = Random.Range(0f, 1f);
            newStar.PosRatio = new Vector3(xPos, yPos, 1f);
            float alpha = Random.Range(MinStarAlpha, MaxStarAlpha);
            newStar.Alpha = alpha;
            float size = Random.Range(MinStarSize, MaxStarSize);
            newStar.Size = new Vector3(size, size, 1f);
            float parallax = -Random.Range(MinStarParallax, MaxStarParallax);
            newStar.PosParallax = new Vector3(parallax, 0f, 0f);

            if ((alpha >= 0.6f) && (size >= 0.02f) && (0.2f <= xPos + parallax) && (xPos + parallax <= 0.7f) && (0.2f <= yPos) && (yPos <= 0.9f))
            {
                // 这个星星够大，够亮，位置也正好
                // 可以在这颗星星上生成 Staff 信息
                newStar.CanShowStaff = true;
                canShowStaffList.Add(newStar);
                newStar.StaffLabelObj.SetActive(true);
            }
        }
        Debug.Log($"随机生成了{starNum}颗星星，其中{canShowStaffList.Count}颗可以显示Staff");
    }

    /// <summary>
    /// 配置和显示StaffLabel
    /// </summary>
    public void GenerateStaffLabels(string rawStaffText)
    {
        // 先禁止所有的StaffLabel渲染
        DisableAllStaffLabels();

        // 将原始Staff信息拆成每一行
        string[] staffTexts = rawStaffText.Split('\n');

        // 传入参数，尝试生成，如果在组内没有碰撞，则正式生成，并分配组号
        foreach (string staffText in staffTexts)
        {
            AssignGroupAndCheckOverlap(staffText);
        }

        // 展示Staff
        // ToDo：改为根据时间轮播每一组
        ShowAssignedStaffLabels();
    }

    /// <summary>
    /// 禁用所有StaffLabel
    /// </summary>
    private void DisableAllStaffLabels()
    {
        foreach (Star star in canShowStaffList)
        {
            StaffLabel staffLabel = star.GetComponentInChildren<StaffLabel>();
            staffLabel.SetRender(false);
            staffLabel.Group = 0;
        }
    }

    /// <summary>
    /// 为Staff分配组并检查碰撞
    /// </summary>
    private void AssignGroupAndCheckOverlap(string staffText)
    {
        int group = 1;
        foreach (Star star in canShowStaffList)
        {
            StaffLabel staffLabel = star.GetComponentInChildren<StaffLabel>();

            if (staffLabel.Group != 0) // 如果星星已被分配，则跳过
                continue;

            // 分配StaffLabel
            string duty = staffText.Split(' ')[0];
            staffLabel.DutyTextObj.GetComponent<TMP_Text>().text = duty;
            string name = staffText.Split(' ')[1];
            staffLabel.NameTextObj.GetComponent<TMP_Text>().text = name;
            staffLabel.RefreshLength();

            // 检查碰撞
            if (!CheckOverlapInGroup(star, group))
            {
                staffLabel.Group = group;
                Debug.Log(group);
                break;
            }
            group++;
        }
    }

    /// <summary>
    /// 检查分组内是否有碰撞
    /// </summary>
    private bool CheckOverlapInGroup(Star star, int group)
    {
        foreach (Star s in canShowStaffList)
        {
            if (s.gameObject == star.gameObject || s.GetComponentInChildren<StaffLabel>().Group != group)
                continue;

            Rect rect1 = GetStaffLabelRect(star);
            Rect rect2 = GetStaffLabelRect(s);

            if (rect1.Overlaps(rect2))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 获取StaffLabel的矩形区域
    /// </summary>
    private Rect GetStaffLabelRect(Star star)
    {
        RectTransform staffLabelRectTransform = star.StaffLabelObj.GetComponent<RectTransform>();
        RectTransform panelRectTransform = star.GetComponentInParent<PageController>().Panel.GetComponent<RectTransform>();

        float x = (star.PosRatio.x + star.PosParallax.x) * panelRectTransform.rect.width;
        float y = (star.PosRatio.y + star.PosParallax.y) * panelRectTransform.rect.height;
        float w = staffLabelRectTransform.rect.width;
        float h = staffLabelRectTransform.rect.height;

        return new Rect(x, y, w, h);
    }

    /// <summary>
    /// 显示已分配的StaffLabel
    /// </summary>
    private void ShowAssignedStaffLabels()
    {
        foreach (Star star in canShowStaffList)
        {
            StaffLabel staffLabel = star.GetComponentInChildren<StaffLabel>();

            if (staffLabel.Group != 0)
                staffLabel.SetRender(true);
        }
    }
}
