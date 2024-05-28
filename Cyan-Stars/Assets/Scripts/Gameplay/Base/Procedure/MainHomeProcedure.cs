using CyanStars.Framework;
using CyanStars.Framework.FSM;
using CyanStars.Framework.Timer;
using CyanStars.Framework.UI;
using CyanStars.Gameplay.MusicGame;
using UnityEngine;

namespace CyanStars.Gameplay.Base
{
    /// <summary>
    /// 主界面流程
    /// </summary>
    [ProcedureState]
    public class MainHomeProcedure : BaseState
    {


        public override async void OnEnter()
        {
            //打开谱面选择界面
            await GameRoot.UI.AwaitOpenUIPanel<MapSelectionPanelR>();
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        public override void OnExit()
        {
            GameRoot.UI.CloseUIPanel<MapSelectionPanelR>();
        }
    }
}
