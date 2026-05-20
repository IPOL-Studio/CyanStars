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
        [SerializeField]
        private Image imgProgress;

        [SerializeField]
        private TextMeshProUGUI txtCombo;

        [SerializeField]
        private TMP_Text txtScore;

        [SerializeField]
        private TMP_Text txtImpurityRate;

        public Image ImgFrame;

        [SerializeField]
        private Button btnStart;

        [SerializeField]
        private Button btnPause;


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
            txtScore.color = new Color(1, 0.841f, 0.078f, 0.87f);
            txtImpurityRate.text = "0.00ms";
            txtImpurityRate.color = new Color(1, 0.841f, 0.078f, 0.87f);
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
            decimal impurityRate = Math.Ceiling((decimal)playingDataModule.MusicGamePlayData.ImpurityRate * 100) / 100;
            txtImpurityRate.text = impurityRate.ToString("F2") + "ms";

            if (playingDataModule.MusicGamePlayData.MissNum > 0)
                txtScore.color = new Color(1, 1, 1, 0.87f);
            else if (playingDataModule.MusicGamePlayData is not { GreatNum: 0, RightNum: 0, OutNum: 0, BadNum: 0, MissNum: 0 })
                txtScore.color = new Color(0.4f, 0.667f, 1, 0.87f);
            else
                txtScore.color = new Color(1, 0.841f, 0.078f, 0.87f);

            txtImpurityRate.color = playingDataModule.MusicGamePlayData.ImpurityRate switch
            {
                > 50 => new Color(1, 1, 1, 0.87f),
                > 30 => new Color(0.4f, 0.667f, 1, 0.87f),
                _ => new Color(1, 0.841f, 0.078f, 0.87f)
            };
        }
    }
}
