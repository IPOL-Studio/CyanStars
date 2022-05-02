using System.Collections;
using System.Collections.Generic;
using CyanStars.Framework;
using CyanStars.Framework.Asset;
using CyanStars.Framework.FSM;
using UnityEngine;

namespace CyanStars.Gameplay.Procedure
{
    /// <summary>
    /// 音游流程
    /// </summary>
    [ProcedureState]
    public class MusicGameProcedure : BaseState
    {
        public override async void OnEnter()
        { 
            await GameRoot.Asset.AwaitLoadScene("Assets/BundleRes/Scenes/Dark.unity");
        }

        public override void OnUpdate(float deltaTime)
        {
            
        }

        public override void OnExit()
        {
            GameRoot.Asset.UnloadScene("Assets/BundleRes/Scenes/Dark.unity");
        }
    }
}

