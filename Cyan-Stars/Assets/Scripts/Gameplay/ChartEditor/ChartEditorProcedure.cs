#nullable enable

using System;
using System.Collections.Generic;
using CatAsset.Runtime;
using CyanStars.Framework;
using CyanStars.Framework.FSM;
using Gameplay.ChartEditor;
using UnityEngine.SceneManagement;

namespace CyanStars.Gameplay.ChartEditor.Procedure
{
    [ProcedureState]
    public class ChartEditorProcedure : BaseState
    {
        private const string ScenePath = "Assets/BundleRes/Scenes/ChartEditor.unity";
        private const string SceneRootName = "SceneRoot";

        private SceneHandler chartEditorSceneHandler;


        public override async void OnEnter()
        {
            // 打开场景并检查制谱器 SceneRoot 状态
            chartEditorSceneHandler = await GameRoot.Asset.LoadSceneAsync(ScenePath);
            Scene chartEditorScene = chartEditorSceneHandler.Scene;

            ChartEditorSceneRoot? sceneRoot = null;
            int foundCount = 0;
            foreach (var rootGameObject in chartEditorScene.GetRootGameObjects())
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

            // 预热资源
            sceneRoot!.gameObject.SetActive(false);
            List<string> assetsToInit = ChartEditorAssetHelper.AllPaths;
            await GameRoot.Asset.BatchLoadAssetAsync(assetsToInit).BindTo(sceneRoot.gameObject);
            sceneRoot.gameObject.SetActive(true);

            // 初始化场景
            sceneRoot.InitSceneRoot();
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        public override void OnExit()
        {
            ChartEditorDataModule chartEditorDataModule = GameRoot.GetDataModule<ChartEditorDataModule>();
            chartEditorDataModule.OnExitChartEditorProcedure();
            GameRoot.Asset.UnloadScene(chartEditorSceneHandler);
        }
    }
}
