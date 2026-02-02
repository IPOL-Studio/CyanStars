#nullable enable

using System;
using CyanStars.Framework;
using CyanStars.Framework.FSM;
using CyanStars.Gameplay.ChartEditor;
using UnityEngine.SceneManagement;

namespace CyanStars.Gameplay.ChartEditor.Procedure
{
    [ProcedureState]
    public class ChartEditorProcedure : BaseState
    {
        private const string ScenePath = "Assets/BundleRes/Scenes/ChartEditor.unity";
        private const string SceneRootName = "SceneRoot";


        public override async void OnEnter()
        {
            // 打开场景并检查制谱器 SceneRoot 状态
            Scene scene = (await GameRoot.Asset.LoadSceneAsync(ScenePath)).Scene;

            ChartEditorSceneRoot? sceneRoot = null;
            int foundCount = 0;
            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                if (rootGameObject.name != SceneRootName)
                {
                    continue;
                }

                sceneRoot = rootGameObject.GetComponent<ChartEditorSceneRoot>();
                if (sceneRoot == null)
                {
                    throw new ArgumentNullException(nameof(sceneRoot), "在制谱器中找到了 SceneRoot，但未挂载 ChartEditorSceneRoot 类，请检查！");
                }

                foundCount++;
            }

            if (foundCount != 1)
            {
                throw new Exception("未找到制谱器 SceneRoot 或找到了多个！");
            }

            // 更新制谱器 DataModule 相关数据
            ChartEditorDataModule chartEditorDataModule = GameRoot.GetDataModule<ChartEditorDataModule>();
            chartEditorDataModule.OnEnterChartEditorProcedure(ChartEditorSceneRoot.CommandStack);

            // 初始化场景
            sceneRoot?.InitSceneRoot();
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        public override void OnExit()
        {
            ChartEditorDataModule chartEditorDataModule = GameRoot.GetDataModule<ChartEditorDataModule>();
            chartEditorDataModule.OnExitChartEditorProcedure();
        }
    }
}
