using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
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

        [Header("Staff 相关配置 Staff related settings")]
        [Tooltip("每轮 Staff 标签的展示时间（秒）")]
        public float KeepShowTime;
        [Tooltip("每组最多展示的 Staff 标签数量")]
        public int MaxStaffLabelNumInGroup;
        #endregion

        /// <summary>
        /// 计时器
        /// </summary>
        float t = 0;

        /// <summary>
        /// 用于分组时的组数
        /// </summary>
        int Group { get; set; }

        /// <summary>
        /// 当前展示的组数
        /// </summary>
        int ShowGroup { get; set; }

        /// <summary>
        /// 当前组内已生成的 Staff 数量
        /// </summary>
        int StaffLabelNumInGroup { get; set; }

        /// <summary>
        /// 最大组数
        /// </summary>
        int MaxGroup { get; set; }

        /// <summary>
        /// 适合挂载 StaffLabel 的 Star 的列表
        /// </summary>
        List<Star> canShowStaffs = new List<Star>();

        /// <summary>
        /// 已挂载 StaffLabel 的 Star 列表
        /// </summary>
        List<Star> haveSetStaffs = new List<Star>();

        /// <summary>
        /// 传入的目标页面（详见 PageController 类）
        /// </summary>
        float targetPage;
        public float TargetPage
        {
            get { return targetPage; }
            set
            {
                targetPage = value;
                ShowGroup = 1;
                t = 0;
                RefreshStaffLabelRender(false);
            }
        }

        // --------------------

        /// <summary>
        /// 按照在 Unity 中配置的参数开始生成星星预制体
        /// 并将适合展示 Staff 的星星记录到 canShowStaffList 列表内
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
                    canShowStaffs.Add(newStar);
                    newStar.SetStaffLabelActive(true);
                }
            }
            Debug.Log($"随机生成了{starNum}颗星星，其中{canShowStaffs.Count}颗可以显示Staff");
        }

        /// <summary>
        /// 切换了曲目，为每一个 Staff 重新分组
        /// </summary>
        public void ResetAllStaffGroup(string rawStaffText)
        {
            StaffLabelNumInGroup = 0;
            string[] staffTexts = rawStaffText.Split('\n');

            if (staffTexts.Length > canShowStaffs.Count)
            {
                Debug.LogError($"Staff数量过多，最多{canShowStaffs.Count}个，目前{staffTexts.Length}个。请尝试设置更多的星星生成数量来临时解决这个问题");    // ToFix
                return;     // 如果继续执行下去好像会导致死循环
            }

            HideAllStaffLabel();
            haveSetStaffs.Clear();
            Group = 1;
            MaxGroup = 1;
            ShowGroup = 1;

            foreach (var item in staffTexts)
            {
                string duty = item.Split(' ')[0];
                string name = item.Split(' ')[1];
                SetGroup(duty, name);
            }
        }

        /// <summary>
        /// 为单个 StaffLabel 找到一颗星星并分组
        /// </summary>
        void SetGroup(string duty, string name)
        {
            while (true)
            {
                // 查找一颗空的星星
                foreach (Star thisStar in canShowStaffs)
                {
                    // 跳过已被占用的星星
                    if (thisStar.Group != 0)
                    {
                        continue;    // 换一颗星星吧
                    }

                    // 找到空的星星，传入文本并刷新物体长度
                    StaffLabel staffLabel = thisStar.GetComponentInChildren<StaffLabel>();
                    staffLabel.DutyText.text = duty;
                    staffLabel.NameText.text = name;
                    staffLabel.RefreshLength();

                    // 检查找到的星星是否与其他同组星星碰撞或达到上限
                    if (CheckCollision(thisStar, Group) || StaffLabelNumInGroup >= MaxStaffLabelNumInGroup)
                    {
                        continue;    // 换一颗星星吧
                    }
                    else
                    {
                        // 成功，这颗星星没有在组内碰撞
                        MaxGroup = Mathf.Max(MaxGroup, Group);
                        haveSetStaffs.Add(thisStar);
                        thisStar.Group = Group;
                        StaffLabelNumInGroup++;
                        return;
                    }
                }
                // 在这一组内没有合适的位置生成星星或已达到数量上限
                Group++;
                StaffLabelNumInGroup = 0;
            }
        }

        /// <summary>
        /// 检查是否在指定组内发生碰撞
        /// </summary>
        bool CheckCollision(Star thisStar, int group)
        {
            foreach (Star otherStar in haveSetStaffs)
            {
                if (thisStar == otherStar || group != otherStar.Group)
                {
                    continue;
                }

                Rect thisRect = GetStaffLabelRect(thisStar);
                Rect otherRect = GetStaffLabelRect(otherStar);
                if (thisRect.Overlaps(otherRect))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 隐藏所有的 StaffLabel
        /// </summary>
        /// <param name="force">不播放渐隐动画，瞬间消失</param>
        void HideAllStaffLabel(bool force = false)
        {
            foreach (Star star in canShowStaffs)
            {
                star.Group = 0;
                StaffLabel staffLabel = star.GetComponentInChildren<StaffLabel>();
                staffLabel.SetRender(false, false);
            }
        }

        /// <summary>
        /// 获取 StaffLabel 的碰撞区域的矩形
        /// </summary>
        Rect GetStaffLabelRect(Star star)
        {
            RectTransform staffLabelRectTransform = star.StaffLabel.CollisionArea;
            RectTransform panelRectTransform = star.GetComponentInParent<PageController>().PanelRectTransform;

            float x = (star.PosRatio.x + star.PosParallax.x) * panelRectTransform.rect.width;
            float y = (star.PosRatio.y + star.PosParallax.y) * panelRectTransform.rect.height;
            float w = staffLabelRectTransform.rect.width;
            float h = staffLabelRectTransform.rect.height;

            return new Rect(x, y, w, h);
        }

        /// <summary>
        /// 刷新 StaffLabel 渲染状态
        /// </summary>
        /// <param name="force">不进行透明度的缓动效果</param>
        void RefreshStaffLabelRender(bool force = false)
        {
            foreach (Star star in haveSetStaffs)
            {
                if (star.Group == ShowGroup && targetPage == 2)
                {
                    star.GetComponentInChildren<StaffLabel>().SetRender(true, force);
                }
                else
                {
                    star.GetComponentInChildren<StaffLabel>().SetRender(false, force);
                }
            }
        }

        void Update()
        {
            t += Time.deltaTime;
            if (t >= KeepShowTime)
            {
                t -= KeepShowTime;
                if (ShowGroup >= MaxGroup)
                {
                    ShowGroup = 1;
                }
                else
                {
                    ShowGroup++;
                }
                RefreshStaffLabelRender(false);
            }
        }
    }
}
