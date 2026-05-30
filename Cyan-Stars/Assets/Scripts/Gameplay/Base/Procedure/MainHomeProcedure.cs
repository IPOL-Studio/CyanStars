using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Framework.FSM;
using CyanStars.Gameplay.MusicGame;

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
            await GameRoot.GetDataModule<ChartModule>().ReloadAllChartPacksAsync();
            await GameRoot.UI.OpenUIPanelAsync<MapSelectionPanel>(); //打开谱面选择界面
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        public override void OnExit()
        {
            GameRoot.UI.CloseUIPanel<MapSelectionPanel>();
        }
    }
}
