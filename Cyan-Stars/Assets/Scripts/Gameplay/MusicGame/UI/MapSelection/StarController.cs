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


        private int groupCount;
        private int currentShowingGroupId;

        private Star[] stars;
        private List<Star> canShowStaffStars = new List<Star>();
        private HashSet<Star> staffShowingStars = new HashSet<Star>();

        private Dictionary<int, int> staffLabelCountInGroupDict = new Dictionary<int, int>();

        private float baseRatio = 1f;

        public void GenerateStars()
        {
            if (this.stars != null)
            {
                Debug.LogWarning("星星已经生成过了");
                return;
            }

            int starCount = Random.Range(minStarCount, maxStarCount + 1);
            var stars = new Star[starCount];
            var trans = transform;

            var panelWidth = panelRectTransform.rect.width;
            float minStarLabelX = panelWidth * 0.2f;
            float maxStarLabelX = panelWidth * 0.7f;

            for (int i = 0; i < starCount; i++)
            {
                var starObj = Instantiate(starPrefab, trans);
                var star = starObj.GetComponent<Star>();

                star.PosRatio = new Vector3(Random.Range(0, maxStarParallax + 1), Random.value, 1f);
                star.EnabledAlpha = Random.Range(minStarAlpha, maxStarAlpha);
                star.PosParallax = -Random.Range(minStarParallax, maxStarParallax) * Vector3.right;

                var size = Random.Range(minStarSize, maxStarSize);
                star.EnabledSize = new Vector3(size, size, 1f);

                var starPos2 = star.CalculatePosFormScreenRatio(panelRectTransform, baseRatio + 1);

                if (star.EnabledAlpha >= 0.6f && size >= 0.02f &&
                    starPos2.x >= minStarLabelX &&
                    starPos2.x <= maxStarLabelX &&
                    star.PosRatio.y >= 0.2f && star.PosRatio.y <= 0.9f)
                {
                    star.CanShowStaff = true;
                    canShowStaffStars.Add(star);
                    star.SetStaffLabelActive(true);
                    starObj.transform.SetSiblingIndex(0);
                }

                star.UpdateFromScreenRatio(panelRectTransform, 1);

                stars[i] = star;
            }

            this.stars = stars;

            Debug.Log($"随机生成了{starCount}颗星星，其中{canShowStaffStars.Count}颗可以显示Staff");
        }

        public void DestroyAllStars()
        {
            if (this.stars == null)
                return;

            foreach (var star in this.stars)
            {
                Destroy(star.gameObject);
            }

            this.stars = null;
        }

        public void ResetAllStaffGroup(Dictionary<string, List<string>> staffs)
        {
            if (staffs.Count > canShowStaffStars.Count)
            {
                Debug.LogError(
                    $"Staff数量过多，最多{canShowStaffStars.Count}个，目前{staffs.Count}个。请尝试设置更多的星星生成数量来临时解决这个问题"); // TODO: 修复星星数量不足导致无法完整展示 Staff 的问题
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
                Debug.Log(item);

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
                    RefreshStaffLabelRender(true, true);
                }

                return;
            }
            else
            {
                int next = currentShowingGroupId + 1;
                currentShowingGroupId = next > groupCount ? 1 : next;

                RefreshStaffLabelRender(true, true);
            }
        }

        public void ResetShowingGroup()
        {
            currentShowingGroupId = 0;
            RefreshStaffLabelRender(false, true);
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

                star.StaffLabel.DutyText = duty;
                star.StaffLabel.NameText = name;
                star.StaffLabel.RefreshLength();

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

        private void RefreshStaffLabelRender(bool isAble, bool isFade)
        {
            foreach (var star in staffShowingStars)
            {
                if (star.GroupId == currentShowingGroupId)
                {
                    star.StaffLabel.SetRender(isAble, isFade);
                }
                else
                {
                    star.StaffLabel.SetRender(false, isFade);
                }
            }
        }

        private void HideAllStaffLabel(bool isFade = true)
        {
            foreach (var star in canShowStaffStars)
            {
                star.GroupId = 0;
                star.StaffLabel.SetRender(false, isFade);
            }
        }

        /// <summary>
        /// 获取 StaffLabel 的碰撞区域的矩形
        /// </summary>
        private Rect GetStaffLabelRect(Star star)
        {
            var pos = star.CalculatePosFormScreenRatio(panelRectTransform, 2);
            var size = star.StaffLabel.CollisionArea.rect.size;

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
