using System;
using TMPro;
using UnityEngine.UI;
using CyanStars.Framework;
using CyanStars.Framework.Event;
using CyanStars.Framework.UI;
using CyanStars.Gameplay.Base;
using UnityEngine;


namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音游主界面
    /// </summary>
    [UIData(UIGroupName = UIConst.UIGroupButtom,
        UIPrefabName = "Assets/BundleRes/Prefabs/MusicGameUI/MusicGameMainPanel.prefab")]
    public class MusicGameMainPanel : BaseUIPanel
    {
        [Header("颜色配置")]
        [SerializeField]
        private Color apColor = new(1, 0.841f, 0.078f, 0.87f);

        [SerializeField]
        private Color fcColor = new(0.4f, 0.667f, 1, 0.87f);

        [SerializeField]
        private Color normalColor = new(1, 1, 1, 0.87f);

        [Header("物体引用")]
        [SerializeField]
        private Image imgProgress;

        [SerializeField]
        private TextMeshProUGUI txtCombo;

        [SerializeField]
        private TMP_Text txtScore;

        [SerializeField]
        private TMP_Text txtImpurityRate;

        [SerializeField]
        private Button btnStart;

        [SerializeField]
        private Button btnPause;

        public Image ImgFrame; // TODO: 边框闪烁暂时弃用，待重构


        private MusicGamePlayingDataModule playingDataModule;


        protected override void OnCreate()
        {
            playingDataModule = GameRoot.GetDataModule<MusicGamePlayingDataModule>();

            btnStart.onClick.AddListener(() =>
            {
                GameRoot.Event.Dispatch(EventConst.MusicGameStartEvent, this, EmptyEventArgs.Create());
                btnStart.gameObject.SetActive(false);
            });

            btnPause.onClick.AddListener(() =>
            {
                GameRoot.UI.OpenUIPanelAsync<MusicGamePausePanel>();
            });
        }

        public override void OnOpen()
        {
            imgProgress.fillAmount = 0;
            txtCombo.text = "0";
            txtScore.text = "0000000";
            txtScore.color = apColor;
            txtImpurityRate.text = "0.00ms";
            txtImpurityRate.color = apColor;
            btnStart.gameObject.SetActive(true);
            Color color = ImgFrame.color;
            color.a = 0;
            ImgFrame.color = color;

            GameRoot.Event.AddListener(EventConst.MusicGameDataRefreshEvent, OnMusicGameDataRefresh);
            GameRoot.Timer.UpdateTimer.Add(OnUpdate);
        }

        public override void OnClose()
        {
            GameRoot.Event.RemoveListener(EventConst.MusicGameDataRefreshEvent, OnMusicGameDataRefresh);
            GameRoot.Timer.UpdateTimer.Remove(OnUpdate);
        }

        private void OnUpdate(double deltaTime, object userdata)
        {
            if (playingDataModule.RunningTimeline != null)
            {
                imgProgress.fillAmount = (float)playingDataModule.RunningTimeline.Context.CurrentTime / playingDataModule.RunningTimeline.Context.Length;
            }
        }

        /// <summary>
        /// 音游数据刷新监听
        /// </summary>
        private void OnMusicGameDataRefresh(object sender, EventArgs args)
        {
            txtCombo.text = playingDataModule.MusicGamePlayData.Combo < 2
                ? string.Empty
                : playingDataModule.MusicGamePlayData.Combo.ToString();
            int score = (int)Math.Floor(playingDataModule.MusicGamePlayData.Score / playingDataModule.MusicGamePlayData.FullScore * 1000000);
            txtScore.text = score.ToString("D7");
            double impurityRate = Math.Ceiling(playingDataModule.MusicGamePlayData.ImpurityRate * 100) / 100;
            txtImpurityRate.text = impurityRate.ToString("F2") + "ms";

            var playData = playingDataModule.MusicGamePlayData;
            bool isAllExact = playData is { GreatNum: 0, RightNum: 0, OutNum: 0, BadNum: 0, MissNum: 0 };
            bool hasMiss = playData.MissNum > 0;

            if (hasMiss)
                txtScore.color = normalColor;
            else if (isAllExact)
                txtScore.color = apColor;
            else
                txtScore.color = fcColor;

            txtImpurityRate.color = playData.ImpurityRate switch
            {
                > 50 => normalColor,
                > 30 => fcColor,
                _ => apColor
            };
        }
    }
}
