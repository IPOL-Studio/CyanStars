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
    List<Vector4> staffLabelPosList = new List<Vector4>();    // 用于记录启用的StaffLabel的2D位置以避免重叠，x(page2时) y 宽 高

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
    /// 为特定Star配置和显示StaffLabel
    /// </summary>
    public void GenerateStaffLabels(string rawStaffText)
    {
        // 先禁用所有已显示的 Label，并清除分组
        foreach (Star star in canShowStaffList)
        {
            StaffLabel staffLabel = star.GetComponentInChildren<StaffLabel>();
            staffLabel.SetRender(false);
            staffLabel.Group = 0;
        }

        string[] staffTexts = rawStaffText.Split('\n');
        if (canShowStaffList.Count < staffTexts.Length)
        {
            // 目前的可用星星大约是生成量的10%
            Debug.LogError("可用星星数量不足以展示所有Staff，这会导致Staff显示不全");
            // 至于怎么解决……还没想过
        }


        // 为每一行 Staff 分配一个可用的星星，并顺序分组
        foreach (string staffText in staffTexts)
        {
            DistributionStar(staffText);
        }

        // 这里的明天再改，写个管理组件按时间在Group之间轮播
        foreach (Star star in canShowStaffList)
        {
            if (star.StaffLabelObj.GetComponent<StaffLabel>().Group != 0)
            {
                star.StaffLabelObj.GetComponent<StaffLabel>().SetRender(true);
            }
        }
    }

    /// <summary>
    /// 找到一个可用且组内不重叠的星星，传入参数
    /// </summary>
    void DistributionStar(string staffText)
    {
        bool flag = false;
        int group = 1;
        int tryTime = 0;
        while (flag == false && tryTime < canShowStaffList.Count)
        {
            tryTime++;
            staffLabelPosList.Clear();
            foreach (Star star in canShowStaffList)
            {
                StaffLabel staffLabel = star.GetComponentInChildren<StaffLabel>();

                // 当前星星已被分配
                if (staffLabel.Group != 0)
                { continue; }

                // 尝试写入Staff信息并判断是否碰撞
                string duty = staffText.Split(' ')[0];
                staffLabel.DutyTextObj.GetComponent<TMP_Text>().text = duty;
                string name = staffText.Split(' ')[1];
                staffLabel.NameTextObj.GetComponent<TMP_Text>().text = name;
                staffLabel.RefreshLength();

                // 如果碰撞则不继续，这颗星星不展示，之后可以被重新写入
                if (CheckOverlapInList(GetStaffLabelVector4FromStar(star)))
                { continue; }

                // 否则分配展示组
                staffLabel.Group = group;
                flag = true;
                break;
            }
            group++;
        }
    }

    /// <summary>
    /// 获取当前Star下StaffLabel的xywh
    /// </summary>
    Vector4 GetStaffLabelVector4FromStar(Star star)
    {
        RectTransform staffLabelRectTransform = star.StaffLabelObj.GetComponent<RectTransform>();
        float x = (star.PosRatio.x + star.PosParallax.x) * Screen.width;
        float y = (star.PosRatio.y + star.PosParallax.y) * Screen.height;
        float w = staffLabelRectTransform.rect.width;
        float h = staffLabelRectTransform.rect.height;
        return new Vector4(x, y, w, h);
    }

    /// <summary>
    /// 检查传入的坐标是否与列表中已有的坐标碰撞
    /// </summary>
    bool CheckOverlapInList(Vector4 v)
    {
        if (staffLabelPosList.Count == 0)
        {
            return false;
        }
        float rect1Right = v.x + v.z;
        float rect1Bottom = v.y + v.w;
        foreach (var item in staffLabelPosList)
        {
            float rect2Right = item.x + item.z;
            float rect2Bottom = item.y + item.w;
            bool xOverlap = item.x < rect1Right && rect2Right > v.x;
            bool yOverlap = item.y < rect1Bottom && rect2Bottom > v.y;
            if (xOverlap && yOverlap)
            {
                return true;
            }
        }
        return false;
    }

}
