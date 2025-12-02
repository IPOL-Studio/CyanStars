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
        public Image ImgProgress;
        public TextMeshProUGUI TxtCombo;
        public TextMeshProUGUI TxtScore;
        public Image ImgFrame;
        public Button BtnStart;
        public TextMeshProUGUI TxtLrc;
        public Button BtnPause;

        private MusicGamePlayingDataModule playingDataModule;

        protected override void OnCreate()
        {
            playingDataModule = GameRoot.GetDataModule<MusicGamePlayingDataModule>();

            BtnStart.onClick.AddListener(() =>
            {
                GameRoot.Event.Dispatch(EventConst.MusicGameStartEvent, this, EmptyEventArgs.Create());
                BtnStart.gameObject.SetActive(false);
            });

            BtnPause.onClick.AddListener(() => { GameRoot.UI.OpenUIPanelAsync<MusicGamePausePanel>(); });
        }

        public override void OnOpen()
        {
            ImgProgress.fillAmount = 0;
            TxtCombo.text = "0";
            TxtScore.text = "SCORE(DEBUG):0";
            BtnStart.gameObject.SetActive(true);
            TxtLrc.text = null;
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

        private void OnUpdate(float deltaTime, object userdata)
        {
            if (playingDataModule.RunningTimeline != null)
            {
                ImgProgress.fillAmount = playingDataModule.RunningTimeline.CurrentTime / playingDataModule.RunningTimeline.Length;
            }
        }

        /// <summary>
        /// 音游数据刷新监听
        /// </summary>
        private void OnMusicGameDataRefresh(object sender, EventArgs args)
        {
            TxtCombo.text = playingDataModule.MusicGamePlayData.Combo < 2
                ? string.Empty
                : playingDataModule.MusicGamePlayData.Combo.ToString();
            TxtScore.text = "SCORE(DEBUG):" + playingDataModule.MusicGamePlayData.Score;
        }
    }
}
