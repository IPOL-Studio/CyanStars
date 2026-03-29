using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class StarController : MonoBehaviour
    {
        [SerializeField]
        private GameObject starPrefab;

        [SerializeField]
        private int minStarCount;

        [SerializeField]
        private int maxStarCount;

        [SerializeField]
        private float minStarAlpha;

        [SerializeField]
        private float maxStarAlpha;

        [SerializeField]
        private float minStarSize;

        [SerializeField]
        private float maxStarSize;

        [SerializeField]
        private float minStarParallax;

        [SerializeField]
        private float maxStarParallax;

        [SerializeField]
        private float staffLabelKeepShowTime;

        [SerializeField]
        private int maxStaffLabelCountInGroup;

        [SerializeField]
        private RectTransform panelRectTransform;


        /// <summary>
        /// StaffLabel 的默认渐变时间（s）
        /// </summary>
        private const float DefaultFadeTime = 0.5f;

        private const float MinStarLabelX = 0.2f;
        private const float MaxStarLabelX = 0.7f;

        private int groupCount;
        private int currentShowingGroupId;

        private Star[] stars;
        private List<Star> canShowStaffStars = new List<Star>();
        private HashSet<Star> staffShowingStars = new HashSet<Star>();

        private Dictionary<int, int> staffLabelCountInGroupDict = new Dictionary<int, int>();

        private float baseRatio = 1f;

        public void GenerateStars()
        {
            if (stars != null)
            {
                Debug.LogWarning("星星已经生成过了");
                return;
            }

            // 随机生成一定数量的星星
            int starCount = Random.Range(minStarCount, maxStarCount + 1);
            stars = new Star[starCount];
            for (int i = 0; i < starCount; i++)
            {
                var starObj = Instantiate(starPrefab, transform);
                var star = starObj.GetComponent<Star>();

                star.PosRatio = new Vector3(Random.Range(0, maxStarParallax + 1), Random.value, 1f);
                star.Alpha = Random.Range(minStarAlpha, maxStarAlpha);
                star.PosParallax = -Random.Range(minStarParallax, maxStarParallax) * Vector3.right;

                var size = Random.Range(minStarSize, maxStarSize);
                star.SetStarScale(size);

                var starWithLabelPos = star.CalculatePosFormScreenRatio(panelRectTransform, baseRatio + 1);
                var panelWidth = panelRectTransform.rect.width;
                if (0.6f <= star.Alpha &&
                    0.02f <= size &&
                    panelWidth * MinStarLabelX <= starWithLabelPos.x &&
                    starWithLabelPos.x <= panelWidth * MaxStarLabelX &&
                    0.2f <= star.PosRatio.y &&
                    star.PosRatio.y <= 0.9f)
                {
                    // 这个星星足够大、足够亮，而且位于屏幕中心附近，可以显示 StaffLabel
                    canShowStaffStars.Add(star);
                    star.SetStaffLabelActive(true);
                    starObj.transform.SetSiblingIndex(0);
                }

                star.UpdateFromScreenRatio(panelRectTransform, 1);

                stars[i] = star;
            }

            Debug.Log($"随机生成了{starCount}颗星星，其中{canShowStaffStars.Count}颗可以显示Staff");
        }

        public void DestroyAllStars()
        {
            if (stars == null)
                return;

            foreach (var star in this.stars)
            {
                Destroy(star.gameObject);
            }

            stars = null;
        }

        /// <summary>
        /// 重新分组 staff
        /// </summary>
        public void ResetAllStaffGroup(Dictionary<string, List<string>> staffs)
        {
            if (staffs.Count > canShowStaffStars.Count)
            {
                // TODO: 先按照已加载曲包中最大 Staff 数量生成合规的星星，再随机生成装饰性星星；以修复星星数量不足导致无法完整展示 Staff 的问题
                Debug.LogError(
                    $"Staff数量过多，最多{canShowStaffStars.Count}个，目前{staffs.Count}个。请尝试设置更多的星星生成数量来临时解决这个问题");
                return;
            }

            HideAllStaffLabel(false);
            staffShowingStars.Clear();
            staffLabelCountInGroupDict.Clear();

            if (staffs.Count == 0)
            {
                groupCount = 0;
                currentShowingGroupId = 0;
                return;
            }

            groupCount = 1;
            foreach (var item in staffs)
            {
                var sb = new StringBuilder();
                foreach (var str in item.Value)
                {
                    sb.Append(str);
                }

                string combined = sb.ToString();

                while (true)
                {
                    if (SetGroup(combined, item.Key, groupCount))
                        break;

                    groupCount++;
                }
            }

            currentShowingGroupId = 0;
            Debug.Log($"Staff分组完成，共{groupCount}组");
        }

        /// <summary>
        /// 显示下一组 staff
        /// </summary>
        public void ShowNextStaffGroup()
        {
            if (groupCount == 0)
            {
                Debug.LogError("请先调用 ResetAllStaffGroup 方法");
                return;
            }

            if (groupCount == 1)
            {
                if (currentShowingGroupId == 0)
                {
                    currentShowingGroupId = staffShowingStars.First().GroupId;
                    RefreshStaffLabelRender(1, true);
                }

                return;
            }
            else
            {
                int next = currentShowingGroupId + 1;
                currentShowingGroupId = next > groupCount ? 1 : next;

                RefreshStaffLabelRender(1, true);
            }
        }

        /// <summary>
        /// 从第一组开始重新显示 staff
        /// </summary>
        public void ResetShowingGroup()
        {
            currentShowingGroupId = 0;
            RefreshStaffLabelRender(0, true);
        }

        public void OnUpdate(float ratio)
        {
            foreach (var star in stars)
            {
                star.UpdateFromScreenRatio(panelRectTransform, ratio);
            }
        }

        private bool SetGroup(string duty, string name, int groupId)
        {
            foreach (var star in canShowStaffStars)
            {
                if (star.GroupId != 0)
                {
                    continue;
                }

                star.SetStaffLabelText(duty, name);

                int staffLabelCount = staffLabelCountInGroupDict.GetValueOrDefault(groupId);

                if (staffLabelCount >= maxStaffLabelCountInGroup || IsCollisionInGroup(star, groupId))
                {
                    continue;
                }

                star.GroupId = groupId;
                staffLabelCountInGroupDict[groupId] = staffLabelCount + 1;
                staffShowingStars.Add(star);
                return true;
            }

            return false;
        }

        private void RefreshStaffLabelRender(float targetAlpha, bool isFade)
        {
            float gradientTime = isFade ? DefaultFadeTime : 0f;
            foreach (var star in staffShowingStars)
            {
                if (star.GroupId == currentShowingGroupId)
                {
                    star.SetRender(targetAlpha, gradientTime);
                }
                else
                {
                    star.SetRender(0, gradientTime);
                }
            }
        }

        private void HideAllStaffLabel(bool isFade = true)
        {
            float gradientTime = isFade ? DefaultFadeTime : 0f;
            foreach (var star in canShowStaffStars)
            {
                star.GroupId = 0;
                star.SetRender(0, gradientTime);
            }
        }

        /// <summary>
        /// 获取 StaffLabel 的碰撞区域的矩形
        /// </summary>
        private Rect GetStaffLabelRect(Star star)
        {
            var pos = star.CalculatePosFormScreenRatio(panelRectTransform, 2);
            var size = star.GetStaffLabelSize();

            return new Rect(pos, size);
        }

        private bool IsCollisionInGroup(Star thisStar, int groupId)
        {
            foreach (var otherStar in staffShowingStars)
            {
                if (thisStar == otherStar || groupId != otherStar.GroupId)
                {
                    continue;
                }

                if (GetStaffLabelRect(thisStar).Overlaps(GetStaffLabelRect(otherStar)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
