#nullable enable

using System;
using CyanStars.Framework;
using CyanStars.Framework.FSM;
using UnityEngine.SceneManagement;

namespace CyanStars.Gameplay.ChartEditor.Procedure
{
    [ProcedureState]
    public class ChartEditorProcedure : BaseState
    {
        private const string ScenePath = "Assets/BundleRes/Scenes/ChartEditor.unity";
        private const string SceneRootName = "SceneRoot";

        private Scene scene;
        private ChartEditorSceneRoot sceneRoot;


        public override async void OnEnter()
        {
            GameRoot.MainCamera.gameObject.SetActive(false);
            scene = (await GameRoot.Asset.LoadSceneAsync(ScenePath)).Scene;

            int foundCount = 0;
            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                if (rootGameObject.name != SceneRootName)
                {
                    continue;
                }

                sceneRoot = rootGameObject.GetComponent<ChartEditorSceneRoot>();
                if (sceneRoot is null)
                {
                    throw new ArgumentNullException(nameof(sceneRoot), "在制谱器中找到了 SceneRoot，但未挂载 ChartEditorSceneRoot 类，请检查！");
                }

                foundCount++;
            }

            if (foundCount != 1)
            {
                throw new Exception("未找到制谱器 SceneRoot 或找到了多个！");
            }
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
