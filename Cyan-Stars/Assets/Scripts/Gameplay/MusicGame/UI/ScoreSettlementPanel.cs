using CyanStars.Framework;
using CyanStars.Framework.Event;
using CyanStars.Framework.UI;
using CyanStars.Gameplay.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
    [UIData(UIGroupName = UIConst.UIGroupButtom,
        UIPrefabName = "Assets/BundleRes/Prefabs/ScoreSettlementUI/ScoreSettlementPanel.prefab")]
    public class ScoreSettlementPanel : BaseUIPanel
    {
        // 硬编码渐变时间配置
        private const float UIRiseTime = 0.5f;
        private const float ScoreNumRiseTime = 2.0f;
        private const float ImpurityRateNumRiseTime = 1.0f;
        private const float MaxComboNumRiseTime = 1.5f;
        private const float ExactNumRiseTime = 1.5f;
        private const float GreatNumRiseTime = 1.5f;
        private const float RightNumRiseTime = 1.5f;
        private const float OutNumRiseTime = 1.5f;
        private const float BadAndMissNumRiseTime = 1.5f;
        private const float EarlyNumRiseTime = 1.5f;
        private const float LateNumRiseTime = 1.5f;
        private const float AveNumRiseTime = 1.5f;

        // Unity组件
        public TextMeshProUGUI Title;
        public TextMeshProUGUI TextScoreNum;
        public TextMeshProUGUI TextImpurityRateNum;
        public TextMeshProUGUI TextMaxComboNum;
        public TextMeshProUGUI TextExactNum;
        public TextMeshProUGUI TextGreatNum;
        public TextMeshProUGUI TextRightNum;
        public TextMeshProUGUI TextOutNum;
        public TextMeshProUGUI TextBadAndMissNum;
        public TextMeshProUGUI TextEarlyNum;
        public TextMeshProUGUI TextLateNum;
        public TextMeshProUGUI TextAveNum;
        public Button ContinueButton;

        // 从音游程序获取目标数据
        private int targetScoreNum;
        private float targetImpurityRateNum;
        private int targetMaxComboNum;
        private int targetExactNum;
        private int targetGreatNum;
        private int targetRightNum;
        private int targetOutNum;
        private int targetBadAndMissNum;
        private int targetEarlyNum;
        private int targetLateNum;
        private float targetAveNum;

        // 内部变量
        private CanvasGroup canvasGroup;
        private float startTime;

        public void Start()
        {
            ContinueButton.onClick.AddListener(() =>
                {
                    GameRoot.UI.CloseUIPanel(this);
                    GameRoot.Event.Dispatch(EventConst.MusicGameExitEvent, this, EmptyEventArgs.Create());
                }
            );
        }

        public override void OnOpen()
        {
            canvasGroup = this.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            startTime = Time.time;

            // 获取曲名、分数、杂率、最大连击数，各判定数
            MusicGameModule musicGameModule = GameRoot.GetDataModule<MusicGameModule>();
            Title.text = musicGameModule.GetMap(musicGameModule.MapIndex).Name;
            targetScoreNum = musicGameModule.FullScore == 0
                ? 0 // 谱面没有 Note 时，展示得分为 0，见于调试谱面等情况
                : Mathf.RoundToInt(musicGameModule.Score / musicGameModule.FullScore * 1000000);
            targetImpurityRateNum = musicGameModule.ImpurityRate;
            targetMaxComboNum = musicGameModule.MaxCombo;

            targetExactNum = musicGameModule.ExactNum;
            targetGreatNum = musicGameModule.GreatNum;
            targetRightNum = musicGameModule.RightNum;
            targetOutNum = musicGameModule.OutNum;
            targetBadAndMissNum = musicGameModule.BadNum + musicGameModule.MissNum;

            // 计算 Early、Late、平均误差
            MusicGameSettingsModule musicGameSettingsModule = GameRoot.GetDataModule<MusicGameSettingsModule>();
            EvaluateRange evaluateRange = musicGameSettingsModule.EvaluateRange;
            float exactRange = evaluateRange.Exact;

            float sum = 0;
            targetEarlyNum = 0;
            targetLateNum = 0;

            foreach (float deviation in musicGameModule.DeviationList) // Drag，尾判等不计算杂率的音符不计算 Early 和 Late
            {
                sum += deviation;
                if (Mathf.Abs(deviation) <= exactRange) continue; // Exact 范围内 Note 不计算 Early 和 Late
                targetEarlyNum += deviation > 0f ? 1 : 0;
                targetLateNum += deviation < 0f ? 1 : 0;
            }

            if (musicGameModule.DeviationList.Count == 0) // 防止玩家放置游玩或没有有效Note的极端情况
            {
                targetLateNum = 0;
            }
            else
            {
                targetAveNum = sum / musicGameModule.DeviationList.Count * 1000f;
            }
        }

        public void Update()
        {
            if (Time.time <= (startTime + UIRiseTime))
            {
                canvasGroup.alpha = UpdateUIAlpha();
            }

            if (Time.time <= (startTime + ScoreNumRiseTime))
            {
                TextScoreNum.text = UpdateScoreNumDisplay();
            }

            if (Time.time <= (startTime + ImpurityRateNumRiseTime))
            {
                TextImpurityRateNum.text = UpdateImpurityRateNumDisplay();
            }

            if (Time.time <= (startTime + MaxComboNumRiseTime))
            {
                TextMaxComboNum.text = UpdateMaxComboNumDisplay();
            }

            if (Time.time <= (startTime + ExactNumRiseTime))
            {
                TextExactNum.text = UpdateExactNumDisplay();
            }

            if (Time.time <= (startTime + GreatNumRiseTime))
            {
                TextGreatNum.text = UpdateGreatNumDisplay();
            }

            if (Time.time <= (startTime + RightNumRiseTime))
            {
                TextRightNum.text = UpdateRightNumDisplay();
            }

            if (Time.time <= (startTime + OutNumRiseTime))
            {
                TextOutNum.text = UpdateOutNumDisplay();
            }

            if (Time.time <= (startTime + BadAndMissNumRiseTime))
            {
                TextBadAndMissNum.text = UpdateBadAndMissNumDisplay();
            }

            if (Time.time <= (startTime + EarlyNumRiseTime))
            {
                TextEarlyNum.text = UpdateEarlyNumDisplay();
            }

            if (Time.time <= (startTime + LateNumRiseTime))
            {
                TextLateNum.text = UpdateLateNumDisplay();
            }

            if (Time.time <= (startTime + AveNumRiseTime))
            {
                TextAveNum.text = UpdateAveNumDisplay();
            }
        }


        /// <summary>
        /// 获取实时UI不透明度
        /// </summary>
        /// <returns>实时UI不透明度</returns>
        private float UpdateUIAlpha()
        {
            return EasingFunction.EaseOutQuart(
                0f,
                1f,
                Mathf.Min(Time.time - startTime, UIRiseTime),
                UIRiseTime
            );
        }

        /// <summary>
        /// 获取实时分数文本
        /// </summary>
        /// <returns>实时分数</returns>
        private string UpdateScoreNumDisplay()
        {
            return Mathf.RoundToInt(
                EasingFunction.EaseOutQuart(
                    0f,
                    targetScoreNum,
                    Mathf.Min(Time.time - startTime, ScoreNumRiseTime),
                    ScoreNumRiseTime
                )
            ).ToString("D7"); // 格式化为7位数字，不足的位数用"0"填充
        }

        /// <summary>
        /// 获取实时杂率文本
        /// </summary>
        /// <returns>实时杂率</returns>
        private string UpdateImpurityRateNumDisplay()
        {
            return (Mathf.RoundToInt(
                        EasingFunction.EaseOutQuart(
                            100.0f,
                            targetImpurityRateNum,
                            Mathf.Min(Time.time - startTime, ImpurityRateNumRiseTime),
                            ImpurityRateNumRiseTime) * 10) / 10f
                ).ToString("0.0");
        }

        /// <summary>
        /// 获取实时MaxCombo文本
        /// </summary>
        /// <returns>实时MaxCombo</returns>
        private string UpdateMaxComboNumDisplay()
        {
            return Mathf.RoundToInt(
                EasingFunction.EaseOutQuart(
                    0f,
                    targetMaxComboNum,
                    Mathf.Min(Time.time - startTime, MaxComboNumRiseTime),
                    MaxComboNumRiseTime
                )
            ).ToString(); // 格式化为7位数字，不足的位数用"0"填充
        }

        private string UpdateExactNumDisplay()
        {
            return Mathf.RoundToInt(
                EasingFunction.EaseOutQuart(
                    0f,
                    targetExactNum,
                    Mathf.Min(Time.time - startTime, ExactNumRiseTime),
                    ExactNumRiseTime
                )
            ).ToString();
        }

        private string UpdateGreatNumDisplay()
        {
            return Mathf.RoundToInt(
                EasingFunction.EaseOutQuart(
                    0f,
                    targetGreatNum,
                    Mathf.Min(Time.time - startTime, GreatNumRiseTime),
                    GreatNumRiseTime
                )
            ).ToString();
        }

        private string UpdateRightNumDisplay()
        {
            return Mathf.RoundToInt(
                EasingFunction.EaseOutQuart(
                    0f,
                    targetRightNum,
                    Mathf.Min(Time.time - startTime, RightNumRiseTime),
                    RightNumRiseTime
                )
            ).ToString();
        }

        private string UpdateOutNumDisplay()
        {
            return Mathf.RoundToInt(
                EasingFunction.EaseOutQuart(
                    0f,
                    targetOutNum,
                    Mathf.Min(Time.time - startTime, OutNumRiseTime),
                    OutNumRiseTime
                )
            ).ToString();
        }

        private string UpdateBadAndMissNumDisplay()
        {
            return Mathf.RoundToInt(
                EasingFunction.EaseOutQuart(
                    0f,
                    targetBadAndMissNum,
                    Mathf.Min(Time.time - startTime, BadAndMissNumRiseTime),
                    BadAndMissNumRiseTime
                )
            ).ToString();
        }

        private string UpdateEarlyNumDisplay()
        {
            return Mathf.RoundToInt(
                EasingFunction.EaseOutQuart(
                    0f,
                    targetEarlyNum,
                    Mathf.Min(Time.time - startTime, EarlyNumRiseTime),
                    EarlyNumRiseTime
                )
            ).ToString();
        }

        private string UpdateLateNumDisplay()
        {
            return Mathf.RoundToInt(
                EasingFunction.EaseOutQuart(
                    0f,
                    targetLateNum,
                    Mathf.Min(Time.time - startTime, LateNumRiseTime),
                    LateNumRiseTime
                )
            ).ToString();
        }

        private string UpdateAveNumDisplay()
        {
            return Mathf.RoundToInt(
                EasingFunction.EaseOutQuart(
                    0f,
                    targetAveNum,
                    Mathf.Min(Time.time - startTime, AveNumRiseTime),
                    AveNumRiseTime
                )
            ).ToString("0.0");
        }
    }
}
