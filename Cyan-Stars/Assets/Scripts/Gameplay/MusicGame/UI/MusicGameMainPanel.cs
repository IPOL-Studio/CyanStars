using System;
using TMPro;
using UnityEngine.UI;
using CyanStars.Framework;
using CyanStars.Framework.Event;
using CyanStars.Framework.UI;
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

        private MusicGameModule dataModule;

        protected override void OnCreate()
        {
            dataModule = GameRoot.GetDataModule<MusicGameModule>();

            BtnStart.onClick.AddListener(() =>
            {
                GameRoot.Event.Dispatch(EventConst.MusicGameStartEvent, this, EmptyEventArgs.Create());
                BtnStart.gameObject.SetActive(false);
            });

            BtnPause.onClick.AddListener(() =>
            {
                GameRoot.UI.OpenUIPanel<MusicGamePausePanel>(null);
            });
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
        }

        public override void OnClose()
        {
            GameRoot.Event.RemoveListener(EventConst.MusicGameDataRefreshEvent, OnMusicGameDataRefresh);
        }

        private void Update()
        {
            //先暂时这样，后面有了timerManager后再改
            if (dataModule.RunningTimeline != null)
            {
                ImgProgress.fillAmount = dataModule.RunningTimeline.CurrentTime / dataModule.RunningTimeline.Length;
            }
        }

        /// <summary>
        /// 音游数据刷新监听
        /// </summary>
        private void OnMusicGameDataRefresh(object sender, EventArgs args)
        {
            TxtCombo.text = dataModule.Combo.ToString();
            TxtScore.text = "SCORE(DEBUG):" + dataModule.Score;
        }




    }
}
