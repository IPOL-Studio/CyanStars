using CyanStars.Framework;
using CyanStars.Framework.FSM;

namespace CyanStars.Gameplay.Procedure
{
    /// <summary>
    /// 主界面流程
    /// </summary>
    [ProcedureState]
    public class MainHomeProcedure : BaseState
    {
        private bool flag;
        
        public override void OnEnter()
        {
            if (!flag)
            {
                flag = true;
                
                //直接切换到音游流程
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