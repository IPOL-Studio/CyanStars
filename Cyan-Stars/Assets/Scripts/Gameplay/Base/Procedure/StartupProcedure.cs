using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Framework.FSM;

namespace CyanStars.Gameplay.Base
{
    /// <summary>
    /// 启动流程
    /// </summary>
    [ProcedureState(true)]
    public class StartupProcedure : BaseState
    {
        public override async void OnEnter()
        {
#if UNITY_EDITOR
            if (GameRoot.Asset.IsEditorMode)
            {
                //编辑器下并且开启了编辑器资源模式 直接切换到主界面流程
                await GameRoot.GetDataModule<ChartModule>().ReloadAllChartPacksAsync();
                GameRoot.ChangeProcedure<MainHomeProcedure>();
                return;
            }
#endif
            //否则需要先检查资源清单
            GameRoot.Asset.CheckVersion(async result =>
            {
                if (result.Success)
                {
                    await GameRoot.GetDataModule<ChartModule>().ReloadAllChartPacksAsync();
                    GameRoot.ChangeProcedure<MainHomeProcedure>();
                }
            });
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        public override void OnExit()
        {
        }
    }
}
