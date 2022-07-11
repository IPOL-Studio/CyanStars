using CyanStars.Framework;
using CyanStars.Framework.Asset;
using CyanStars.Framework.FSM;
using UnityEngine;

namespace CyanStars.Gameplay.Procedure
{
    /// <summary>
    /// 主界面流程
    /// </summary>
    [ProcedureState]
    public class MainHomeProcedure : BaseState
    {
        private bool flag;

        public override async void OnEnter()
        {
            if (!flag)
            {
                flag = true;
                //加载内置谱面清单
                await GameRoot.GetDataModule<MusicGameModule>().LoadInternalMaps();

                //切换到音游流程
                GameRoot.ChangeProcedure<MusicGameProcedure>();
            }
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        public override void OnExit()
        {
        }
    }
}
