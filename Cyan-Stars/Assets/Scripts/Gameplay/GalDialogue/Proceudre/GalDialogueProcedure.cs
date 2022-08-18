using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.Asset;
using CyanStars.Framework.FSM;
using UnityEngine;

public class GalDialogueProcedure : BaseState
{
    public override async void OnEnter()
    {
        bool success = await GameRoot.Asset.AwaitLoadScene("Assets/BundleRes/Scenes/Dialogue.unity");

        if (success)
        {

        }
    }

    public override void OnUpdate(float deltaTime)
    {

    }

    public override void OnExit()
    {

    }
}
