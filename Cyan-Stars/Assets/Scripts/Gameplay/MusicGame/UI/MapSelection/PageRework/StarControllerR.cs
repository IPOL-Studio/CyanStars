using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Gameplay.MusicGame
{
    public class StarControllerR : MonoBehaviour
    {
        [SerializeField] private GameObject starPrefab;

        [SerializeField] private int minStarCount;
        [SerializeField] private int maxStarCount;

        [SerializeField] private float minStarAlpha;
        [SerializeField] private float maxStarAlpha;

        [SerializeField] private float minStarSize;
        [SerializeField] private float maxStarSize;

        [SerializeField] private float minStarParallax;
        [SerializeField] private float maxStarParallax;

        [SerializeField] private float staffLabelKeepShowTime;
        [SerializeField] private int maxStaffLabelCountInGroup;

        [SerializeField] private RectTransform panelRectTransform;


        private int groupCount;
        private int currentShowingGroupId;

        private StarR[] stars;
        private List<StarR> canShowStaffStars = new List<StarR>();
        private HashSet<StarR> staffShowingStars = new HashSet<StarR>();

        private Dictionary<int, int> staffLabelCountInGroupDict = new Dictionary<int, int>();

        public void GenerateStars()
        {
            if (this.stars != null)
            {
                Debug.LogWarning("星星已经生成过了");
                return;
            }

            int starCount = Random.Range(minStarCount, maxStarCount + 1);
            var stars = new StarR[starCount];
            var trans = transform;

            for (int i = 0; i < starCount; i++)
            {
                var starObj = Instantiate(starPrefab, trans);
                var star = starObj.GetComponent<StarR>();

                star.PosRatio = new Vector3(Random.Range(0, maxStarParallax + 1), Random.value, 1f);
                star.EnabledAlpha = Random.Range(minStarAlpha, maxStarAlpha);
                star.PosParallax = -Random.Range(minStarParallax, maxStarParallax) * Vector3.right;

                var size = Random.Range(minStarSize, maxStarSize);
                star.EnabledSize = new Vector3(size, size, 1f);

                if (star.EnabledAlpha >= 0.6f && size >= 0.02f &&
                    star.PosRatio.x + star.PosParallax.x >= 0.2f && star.PosRatio.x + star.PosParallax.x <= 0.7f &&
                    star.PosRatio.y >= 0.2f && star.PosRatio.y <= 0.9f)
                {
                    star.CanShowStaff = true;
                    canShowStaffStars.Add(star);
                    star.SetStaffLabelActive(true);
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

        public void ResetAllStaffGroup(string rawStaffText)
        {
            var staffs = rawStaffText.Split('\n');

            if (staffs.Length > canShowStaffStars.Count)
            {
                Debug.LogError($"Staff数量过多，最多{canShowStaffStars.Count}个，目前{staffs.Length}个。请尝试设置更多的星星生成数量来临时解决这个问题");    // ToFix
                return;
            }

            HideAllStaffLabel(false);
            staffShowingStars.Clear();

            if (staffs is null || staffs.Length == 0)
            {
                groupCount = 0;
                currentShowingGroupId = 0;
                return;
            }

            groupCount = 1;
            foreach (var item in staffs)
            {
                var arr = item.Split(' ');
                Debug.Log(item);

                while (true)
                {
                    if (SetGroup(arr[0], arr[1], groupCount))
                        break;

                    groupCount++;
                }
            }

            currentShowingGroupId = 1;
        }

        public void ShowNextStaffGroup()
        {
            if (groupCount == 0)
            {
                Debug.LogError("请先调用 ResetAllStaffGroup 方法");
                return;
            }

            int next = currentShowingGroupId + 1;
            currentShowingGroupId = next > groupCount ? 1 : next;

            RefreshStaffLabelRender(true, true);
        }

        public void ResetShowingGroup() => currentShowingGroupId = groupCount == 0 ? 0 : 1;

        public void OnUpdate()
        {

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
        private Rect GetStaffLabelRect(StarR star)
        {
            var pos = (star.PosRatio + star.PosParallax) * panelRectTransform.rect.size;
            var size = star.StaffLabel.CollisionArea.rect.size;

            return new Rect(pos, size);
        }

        private bool IsCollisionInGroup(StarR thisStar, int groupId)
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
