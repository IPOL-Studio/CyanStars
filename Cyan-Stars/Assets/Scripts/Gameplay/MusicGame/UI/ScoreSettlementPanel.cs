using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Framework.Event;
using CyanStars.Framework.UI;
using CyanStars.Gameplay.Base;
using CyanStars.GameSave;
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
        [Header("渐变时间配置")]
        public float UIRiseTime = 0.8f;

        public float GrayImageKeepTime = 0.5f;
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

        [Header("Unity组件")]
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
        public Image ImageGrade;
        public Image ImageSun;
        public Button ContinueButton;
        public CanvasGroup MainUICanvasGroup;
        public CanvasGroup GrayImageCanvasGroup;

        [Header("多媒体文件引用")]
        public Sprite GradeClear;

        public Sprite GradeFullCombo;
        public Sprite GradeFullComboPlus;
        public Sprite GradeAllExact;
        public Sprite GradeAllExactPlus;
        public Sprite GradeUltraPure;
        public Sprite SunClear;
        public Sprite SunFc;
        public Sprite SunAeAndUp;

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

        private ChartGrade grade;


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
            // 将 UI 恢复到初始状态
            MainUICanvasGroup.alpha = 0;
            GrayImageCanvasGroup.alpha = 0;

            PreprocessData();
            PlayTextFadeEffect();
        }

        /// <summary>
        /// 获取数据并计算目标值
        /// </summary>
        private void PreprocessData()
        {
            // 获取曲名、分数、杂率、最大连击数，各判定数
            var musicGamePlayingDataModule = GameRoot.GetDataModule<MusicGamePlayingDataModule>();
            var chartModule = GameRoot.GetDataModule<ChartModule>();
            MusicGamePlayData musicGamePlayData = musicGamePlayingDataModule.MusicGamePlayData;
            Title.text = chartModule.SelectedRuntimeChartPack.ChartPackData.Title;
            targetScoreNum = musicGamePlayData.FullScore == 0
                ? 0 // 谱面没有 Note 时，展示得分为 0，见于调试谱面等情况
                : Mathf.RoundToInt(musicGamePlayData.Score /
                    musicGamePlayData.FullScore * 1000000);
            targetImpurityRateNum = musicGamePlayData.ImpurityRate;
            targetMaxComboNum = musicGamePlayData.MaxCombo;

            targetExactNum = musicGamePlayData.ExactNum;
            targetGreatNum = musicGamePlayData.GreatNum;
            targetRightNum = musicGamePlayData.RightNum;
            targetOutNum = musicGamePlayData.OutNum;
            targetBadAndMissNum = musicGamePlayData.BadNum + musicGamePlayData.MissNum;

            // 计算 Early、Late、平均误差
            MusicGameSettingsModule musicGameSettingsModule = GameRoot.GetDataModule<MusicGameSettingsModule>();
            EvaluateRange evaluateRange = musicGameSettingsModule.EvaluateRange;
            float exactRange = evaluateRange.Exact;

            float sum = 0;
            targetEarlyNum = 0;
            targetLateNum = 0;

            foreach (float deviation in
                     musicGamePlayData.DeviationList) // Drag，尾判等不计算杂率的音符不计算 Early 和 Late
            {
                sum += deviation;
                if (Mathf.Abs(deviation) <= exactRange) continue; // Exact 范围内 Note 不计算 Early 和 Late
                targetEarlyNum += deviation > 0f ? 1 : 0;
                targetLateNum += deviation < 0f ? 1 : 0;
            }

            if (musicGamePlayData.DeviationList.Count == 0) // 防止玩家放置游玩或没有有效Note的极端情况
            {
                targetAveNum = 0;
            }
            else
            {
                targetAveNum = sum / musicGamePlayData.DeviationList.Count * 1000f;
            }

            // 获取评级并切换图片
            grade = GradeHelper.GetGrade(musicGamePlayData);
            switch (grade)
            {
                case ChartGrade.Clear:
                    ImageGrade.sprite = GradeClear;
                    ImageSun.sprite = SunClear;
                    break;
                case ChartGrade.FullCombo:
                    ImageGrade.sprite = GradeFullCombo;
                    ImageSun.sprite = SunFc;
                    break;
                case ChartGrade.FullComboPlus:
                    ImageGrade.sprite = GradeFullComboPlus;
                    ImageSun.sprite = SunFc;
                    break;
                case ChartGrade.AllExact:
                    ImageGrade.sprite = GradeAllExact;
                    ImageSun.sprite = SunAeAndUp;
                    break;
                case ChartGrade.AllExactPlus:
                    ImageGrade.sprite = GradeAllExactPlus;
                    ImageSun.sprite = SunAeAndUp;
                    break;
                case ChartGrade.UltraPure:
                    ImageGrade.sprite = GradeUltraPure;
                    ImageSun.sprite = SunAeAndUp;
                    break;
                default:
                    // But how?
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 通过 DOTween 播放动画
        /// </summary>
        private void PlayTextFadeEffect()
        {
            // 切入灰色过场图
            DOTween.To(() => 0f,
                    x => GrayImageCanvasGroup.alpha = x,
                    1f,
                    UIRiseTime / 2)
                .SetEase(Ease.OutQuart)
                .OnComplete(() =>
                {
                    MainUICanvasGroup.DOFade(1f, GrayImageKeepTime) // GrayImageKeepTime 在这里起到了延迟写一个动画的作用
                        .OnComplete(() =>
                        {
                            // 淡去灰色图像，同时开始文本数值渐变
                            DOTween.To(() => 1f,
                                    x => GrayImageCanvasGroup.alpha = x,
                                    0f,
                                    UIRiseTime / 2)
                                .SetEase(Ease.InQuart);


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
                        });
                });
        }
    }
}
