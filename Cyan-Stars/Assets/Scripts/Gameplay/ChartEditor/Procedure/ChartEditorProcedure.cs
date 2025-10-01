using CyanStars.Framework;
using CyanStars.Framework.FSM;
using UnityEngine.SceneManagement;

namespace CyanStars.GamePlay.ChartEditor.Procedure
{
    [ProcedureState]
    public class ChartEditorProcedure : BaseState
    {
        private const string ScenePath = "Assets/BundleRes/Scenes/ChartEditor.unity";

        private Scene scene;

        public override async void OnEnter()
        {
            GameRoot.MainCamera.gameObject.SetActive(false);
            scene = await GameRoot.Asset.LoadSceneAsync(ScenePath);
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        public override void OnExit()
        {
            GameRoot.MainCamera.gameObject.SetActive(true);
        }
    }
}
