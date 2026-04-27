#nullable enable

using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Framework.Event;
using CyanStars.Framework.UI;
using CyanStars.Gameplay.Base;
using CyanStars.GameSave;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame.UI.ScoreSettlement
{
    [UIData(UIGroupName = UIConst.UIGroupButtom,
        UIPrefabName = "Assets/BundleRes/Prefabs/ScoreSettlementUI/ScoreSettlementPanel.prefab")]
    public class ScoreSettlementPanel : BaseUIPanel
    {
        #region 组件引用

        [Header("缓动相关")]
        [SerializeField]
        private CanvasGroup mainCanvasGroup = null!;

        [SerializeField]
        private CanvasGroup blackCoverCanvasGroup = null!;

        [SerializeField]
        private float riseTimeS = 1.5f;


        [Header("曲目信息组件")]
        [SerializeField]
        private TMP_Text musicTitleText = null!;

        [SerializeField]
        private TMP_Text difficultyAndLevelText = null!;


        [Header("分数相关组件")]
        [SerializeField]
        private TMP_Text scoreText = null!;

        [SerializeField]
        private GameObject newBestGameObject = null!;


        [Header("杂率相关组件")]
        [SerializeField]
        private TMP_Text impurityRateText = null!;

        [SerializeField]
        private TMP_Text earlyCountText = null!;

        [SerializeField]
        private TMP_Text lateCountText = null!;

        [SerializeField]
        private TMP_Text averageOffsetText = null!;


        [Header("最大连击相关组件")]
        [SerializeField]
        private TMP_Text maxComboText = null!;

        [SerializeField]
        private TMP_Text exactCountText = null!;

        [SerializeField]
        private TMP_Text greatCountText = null!;

        [SerializeField]
        private TMP_Text rightCountText = null!;

        [SerializeField]
        private TMP_Text outCountText = null!;

        [SerializeField]
        private TMP_Text badCountText = null!;

        [SerializeField]
        private TMP_Text missCountText = null!;


        [Header("成绩等级相关组件")]
        [SerializeField]
        private GameObject ultraPureGameObject = null!;

        [SerializeField]
        private GameObject allExactPlusGameObject = null!;

        [SerializeField]
        private GameObject allExactGameObject = null!;

        [SerializeField]
        private GameObject fullComboPlusGameObject = null!;

        [SerializeField]
        private GameObject fullComboGameObject = null!;

        [SerializeField]
        private GameObject clearGameObject = null!;


        [Header("立绘")]
        [SerializeField]
        private RawImage image = null!;


        [Header("按钮相关组件")]
        [SerializeField]
        private Button screenshotButton = null!;

        [SerializeField]
        private Button continueButton = null!;

        #endregion

        #region 缓动目标值

        private int targetScoreNum;

        private float targetImpurityRateNum;
        private int targetEarlyCountNum;
        private int targetLateCountNum;
        private float targetAverageOffsetNum;

        private int targetMaxComboNum;
        private int targetExactCountNum;
        private int targetGreatCountNum;
        private int targetRightCountNum;
        private int targetOutCountNum;
        private int targetBadCountNum;
        private int targetMissCountNum;

        #endregion

        protected override void OnCreate()
        {
            continueButton.onClick.AddListener(() =>
                {
                    GameRoot.UI.CloseUIPanel(this);
                    GameRoot.Event.Dispatch(EventConst.MusicGameExitEvent, this, EmptyEventArgs.Create());
                }
            );
        }

        public override void OnOpen()
        {
            blackCoverCanvasGroup.alpha = 0f;
            mainCanvasGroup.alpha = 0f;
            PreprocessData();
            PlayTextFadeEffect();
        }


        /// <summary>
        /// 获取数据并计算目标值
        /// </summary>
        private void PreprocessData()
        {
            // 获取曲名、分数、杂率、最大连击数，各判定数
            var dataModule = GameRoot.GetDataModule<MusicGamePlayingDataModule>();
            var chartModule = GameRoot.GetDataModule<ChartModule>();

            MusicGamePlayData musicGamePlayData = dataModule.MusicGamePlayData;
            musicTitleText.text = chartModule.SelectedRuntimeChartPack.ChartPackData.Title;
            difficultyAndLevelText.text = $"{0} {1}"; // TODO: 对难度字段进行国际化

            targetScoreNum = musicGamePlayData.FullScore == 0
                ? 0 // 谱面没有 Note 时，展示得分为 0，见于调试谱面等情况
                : Mathf.RoundToInt(musicGamePlayData.Score /
                    musicGamePlayData.FullScore * 1000000);
            targetImpurityRateNum = musicGamePlayData.ImpurityRate;
            targetMaxComboNum = musicGamePlayData.MaxCombo;

            targetExactCountNum = musicGamePlayData.ExactNum;
            targetGreatCountNum = musicGamePlayData.GreatNum;
            targetRightCountNum = musicGamePlayData.RightNum;
            targetOutCountNum = musicGamePlayData.OutNum;
            targetBadCountNum = musicGamePlayData.BadNum;
            targetMissCountNum = musicGamePlayData.MissNum;

            // 计算 Early、Late、平均误差
            MusicGameSettingsModule musicGameSettingsModule = GameRoot.GetDataModule<MusicGameSettingsModule>();
            EvaluateRange evaluateRange = musicGameSettingsModule.EvaluateRange;
            float exactRange = evaluateRange.Exact;

            float sum = 0;
            targetEarlyCountNum = 0;
            targetLateCountNum = 0;

            foreach (float deviation in
                     musicGamePlayData.DeviationList) // Drag，尾判等不计算杂率的音符不计算 Early 和 Late
            {
                sum += deviation;
                if (Mathf.Abs(deviation) <= exactRange) continue; // Exact 范围内 Note 不计算 Early 和 Late
                targetEarlyCountNum += deviation > 0f ? 1 : 0;
                targetLateCountNum += deviation < 0f ? 1 : 0;
            }

            if (musicGamePlayData.DeviationList.Count == 0) // 防止玩家放置游玩或没有有效Note的极端情况
            {
                targetAverageOffsetNum = 0;
            }
            else
            {
                targetAverageOffsetNum = sum / musicGamePlayData.DeviationList.Count * 1000f;
            }

            // 获取评级并切换图片
            var grade = GradeHelper.GetGrade(musicGamePlayData);
            ultraPureGameObject.SetActive(grade == ChartGrade.UltraPure);
            allExactPlusGameObject.SetActive(grade == ChartGrade.AllExactPlus);
            allExactGameObject.SetActive(grade == ChartGrade.AllExact);
            fullComboPlusGameObject.SetActive(grade == ChartGrade.FullComboPlus);
            fullComboGameObject.SetActive(grade == ChartGrade.FullCombo);
            clearGameObject.SetActive(grade == ChartGrade.Clear);
        }

        /// <summary>
        /// 通过 DOTween 播放动画
        /// </summary>
        private void PlayTextFadeEffect()
        {
            // 切入灰色过场图
            DOTween.To(() => 0f,
                    x => blackCoverCanvasGroup.alpha = x,
                    1f,
                    riseTimeS / 2f)
                .SetEase(Ease.OutQuart)
                .OnComplete(() =>
                    {
                        mainCanvasGroup.DOFade(1f, 0.1f) // 此处 duration 参数用于在完全黑屏后延迟一段时间再减淡
                            .OnComplete(() =>
                                {
                                    // 淡去灰色图像，同时开始文本数值渐变
                                    DOTween.To(() => 1f,
                                            x => blackCoverCanvasGroup.alpha = x,
                                            0f,
                                            riseTimeS / 2f)
                                        .SetEase(Ease.InQuart);

                                    DOTween.To(() => 0f,
                                            x => scoreText.text = x.ToString("0000000"),
                                            targetScoreNum,
                                            riseTimeS)
                                        .SetEase(Ease.OutQuart);
                                    DOTween.To(() => 100.0f,
                                            x => impurityRateText.text = x.ToString("0.0"),
                                            targetImpurityRateNum,
                                            riseTimeS)
                                        .SetEase(Ease.OutQuart);
                                    DOTween.To(() => 0f,
                                            x => maxComboText.text = x.ToString("0"),
                                            targetMaxComboNum,
                                            riseTimeS)
                                        .SetEase(Ease.OutQuart);

                                    DOTween.To(() => 0f,
                                            x => earlyCountText.text = x.ToString("0"),
                                            targetEarlyCountNum,
                                            riseTimeS)
                                        .SetEase(Ease.OutQuart);
                                    DOTween.To(() => 0f,
                                            x => lateCountText.text = x.ToString("0"),
                                            targetLateCountNum,
                                            riseTimeS)
                                        .SetEase(Ease.OutQuart);
                                    DOTween.To(() => 0f,
                                            x => averageOffsetText.text = x.ToString("0.0"),
                                            targetAverageOffsetNum,
                                            riseTimeS)
                                        .SetEase(Ease.OutQuart);

                                    DOTween.To(() => 0f,
                                            x => exactCountText.text = x.ToString("0"),
                                            targetExactCountNum,
                                            riseTimeS)
                                        .SetEase(Ease.OutQuart);
                                    DOTween.To(() => 0f,
                                            x => greatCountText.text = x.ToString("0"),
                                            targetGreatCountNum,
                                            riseTimeS)
                                        .SetEase(Ease.OutQuart);
                                    DOTween.To(() => 0f,
                                            x => rightCountText.text = x.ToString("0"),
                                            targetRightCountNum,
                                            riseTimeS)
                                        .SetEase(Ease.OutQuart);
                                    DOTween.To(() => 0f,
                                            x => outCountText.text = x.ToString("0"),
                                            targetOutCountNum,
                                            riseTimeS)
                                        .SetEase(Ease.OutQuart);
                                    DOTween.To(() => 0f,
                                            x => badCountText.text = x.ToString("0"),
                                            targetBadCountNum,
                                            riseTimeS)
                                        .SetEase(Ease.OutQuart);
                                    DOTween.To(() => 0f,
                                            x => missCountText.text = x.ToString("0"),
                                            targetMissCountNum,
                                            riseTimeS)
                                        .SetEase(Ease.OutQuart);
                                }
                            );
                    }
                );
        }
    }
}
