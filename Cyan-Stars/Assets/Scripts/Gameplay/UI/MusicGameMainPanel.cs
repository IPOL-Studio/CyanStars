using System;
using System.Collections;
using System.Collections.Generic;
using CyanStars.Framework;
using CyanStars.Framework.UI;
using CyanStars.Gameplay.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.UI
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

        private MusicGameModule dataModule;
        
        protected override void OnCreate()
        {
            dataModule = GameRoot.GetDataModule<MusicGameModule>();
            
            BtnStart.onClick.AddListener(() =>
            {
                GameRoot.Event.Dispatch(EventConst.MusicGameStartEvent,this,System.EventArgs.Empty);
            });
        }

        public override void OnOpen()
        {
            GameRoot.Event.AddListener(EventConst.MusicGameDataRefreshEvent,OnMusicGameDataRefresh);
        }

        public override void OnClose()
        {
            GameRoot.Event.RemoveListener(EventConst.MusicGameDataRefreshEvent,OnMusicGameDataRefresh);
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
        /// 音游数据刷新回调
        /// </summary>
        private void OnMusicGameDataRefresh(object sender, EventArgs args)
        {
            TxtCombo.text = dataModule.Combo.ToString();
            TxtScore.text = "SCORE(DEBUG):" + dataModule.Score;
        }
    }
}

