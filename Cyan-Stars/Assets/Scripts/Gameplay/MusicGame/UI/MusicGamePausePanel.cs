using System;
using CyanStars.Framework;
using CyanStars.Framework.Event;
using CyanStars.Framework.UI;
using CyanStars.Gameplay.Base;
using UnityEngine.UI;

namespace CyanStars.Gameplay.MusicGame
{
    /// <summary>
    /// 音游暂停界面
    /// </summary>
    [UIData(UIGroupName = UIConst.UIGroupButtom,
        UIPrefabName = "Assets/BundleRes/Prefabs/MusicGameUI/MusicGamePausePanel.prefab")]
    public class MusicGamePausePanel : BaseUIPanel
    {
        public Button BtnResume;
        public Button BtnExit;

        protected override void OnCreate()
        {
            BtnResume.onClick.AddListener(() =>
            {
                GameRoot.UI.CloseUIPanel(this);
                GameRoot.Event.Dispatch(EventConst.MusicGameResumeEvent,this,EmptyEventArgs.Create());
            });

            BtnExit.onClick.AddListener(() =>
            {
                GameRoot.UI.CloseUIPanel(this);
                GameRoot.Event.Dispatch(EventConst.MusicGameExitEvent,this,EmptyEventArgs.Create());
            });
        }

        public override void OnOpen()
        {
            GameRoot.Event.Dispatch(EventConst.MusicGamePauseEvent,this,EmptyEventArgs.Create());
        }
    }
}
