using CyanStars.Framework;
using CyanStars.Framework.Event;
using CyanStars.Framework.UI;
using CyanStars.Gameplay.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CyanStars.Gameplay.MusicGame
{
    [UIData(UIGroupName = UIConst.UIGroupButtom,
        UIPrefabName = "Assets/BundleRes/Prefabs/ScoreSettlementUI/ScoreSettlementPanel.prefab")]
    public class ScoreSettlementPanel : BaseUIPanel
    {
        // 渐变时间
        public float UIRiseTime = 0.5f;
        public float ScoreNumRiseTime = 2.0f;
        public float ImpurityRateNumRiseTime = 1.0f;
        public float MaxComboNumRiseTime = 1.5f;
        public float ExactNumRiseTime = 1.5f;
        public float GreatNumRiseTime = 1.5f;
        public float RightNumRiseTime = 1.5f;
        public float OutNumRiseTime = 1.5f;
        public float BadAndMissNumRiseTime = 1.5f;
        public float EarlyNumRiseTime = 1.5f;
        public float LateNumRiseTime = 1.5f;
        public float AveNumRiseTime = 1.5f;

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

            // 获取曲名、分数、杂率、最大连击数，各判定数
            MusicGameModule musicGameModule = GameRoot.GetDataModule<MusicGameModule>();
            Title.text = musicGameModule.GetMap(musicGameModule.MapIndex).Name;
            targetScoreNum = musicGameModule.MusicGamePlayData.FullScore == 0
                ? 0 // 谱面没有 Note 时，展示得分为 0，见于调试谱面等情况
                : Mathf.RoundToInt(musicGameModule.MusicGamePlayData.Score /
                    musicGameModule.MusicGamePlayData.FullScore * 1000000);
            targetImpurityRateNum = musicGameModule.MusicGamePlayData.ImpurityRate;
            targetMaxComboNum = musicGameModule.MusicGamePlayData.MaxCombo;

            targetExactNum = musicGameModule.MusicGamePlayData.ExactNum;
            targetGreatNum = musicGameModule.MusicGamePlayData.GreatNum;
            targetRightNum = musicGameModule.MusicGamePlayData.RightNum;
            targetOutNum = musicGameModule.MusicGamePlayData.OutNum;
            targetBadAndMissNum = musicGameModule.MusicGamePlayData.BadNum + musicGameModule.MusicGamePlayData.MissNum;

            // 计算 Early、Late、平均误差
            MusicGameSettingsModule musicGameSettingsModule = GameRoot.GetDataModule<MusicGameSettingsModule>();
            EvaluateRange evaluateRange = musicGameSettingsModule.EvaluateRange;
            float exactRange = evaluateRange.Exact;

            float sum = 0;
            targetEarlyNum = 0;
            targetLateNum = 0;

            foreach (float deviation in
                     musicGameModule.MusicGamePlayData.DeviationList) // Drag，尾判等不计算杂率的音符不计算 Early 和 Late
            {
                sum += deviation;
                if (Mathf.Abs(deviation) <= exactRange) continue; // Exact 范围内 Note 不计算 Early 和 Late
                targetEarlyNum += deviation > 0f ? 1 : 0;
                targetLateNum += deviation < 0f ? 1 : 0;
            }

            if (musicGameModule.MusicGamePlayData.DeviationList.Count == 0) // 防止玩家放置游玩或没有有效Note的极端情况
            {
                targetAveNum = 0;
            }
            else
            {
                targetAveNum = sum / musicGameModule.MusicGamePlayData.DeviationList.Count * 1000f;
            }

            DOTween.To(() => 0f,
                    x => canvasGroup.alpha = x,
                    1f,
                    UIRiseTime)
                .SetEase(Ease.OutQuart);

            DOTween.To(() => 0f,
                    x => TextScoreNum.text = x.ToString("0000000"),
                    targetScoreNum,
                    ScoreNumRiseTime)
                .SetEase(Ease.OutQuart);

            DOTween.To(() => 100.0f,
                    x => TextImpurityRateNum.text = x.ToString("0.0"),
                    targetImpurityRateNum,
                    ImpurityRateNumRiseTime)
                .SetEase(Ease.OutQuart);

            DOTween.To(() => 0f,
                    x => TextMaxComboNum.text = x.ToString("0"),
                    targetMaxComboNum,
                    MaxComboNumRiseTime)
                .SetEase(Ease.OutQuart);

            DOTween.To(() => 0f,
                    x => TextExactNum.text = x.ToString("0"),
                    targetExactNum,
                    ExactNumRiseTime)
                .SetEase(Ease.OutQuart);

            DOTween.To(() => 0f,
                    x => TextGreatNum.text = x.ToString("0"),
                    targetGreatNum,
                    GreatNumRiseTime)
                .SetEase(Ease.OutQuart);

            DOTween.To(() => 0f,
                    x => TextRightNum.text = x.ToString("0"),
                    targetRightNum,
                    RightNumRiseTime)
                .SetEase(Ease.OutQuart);

            DOTween.To(() => 0f,
                    x => TextOutNum.text = x.ToString("0"),
                    targetOutNum,
                    OutNumRiseTime)
                .SetEase(Ease.OutQuart);

            DOTween.To(() => 0f,
                    x => TextBadAndMissNum.text = x.ToString("0"),
                    targetBadAndMissNum,
                    BadAndMissNumRiseTime)
                .SetEase(Ease.OutQuart);

            DOTween.To(() => 0f,
                    x => TextEarlyNum.text = x.ToString("0"),
                    targetEarlyNum,
                    EarlyNumRiseTime)
                .SetEase(Ease.OutQuart);

            DOTween.To(() => 0f,
                    x => TextLateNum.text = x.ToString("0"),
                    targetLateNum,
                    LateNumRiseTime)
                .SetEase(Ease.OutQuart);

            DOTween.To(() => 0f,
                    x => TextAveNum.text = x.ToString("0.0"),
                    targetAveNum,
                    AveNumRiseTime)
                .SetEase(Ease.OutQuart);
        }
    }
}
