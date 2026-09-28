#nullable enable

using System;
using System.Collections.Generic;
using CatAsset.Runtime;
using CyanStars.Chart;
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

        private SceneHandler chartEditorSceneHandler = null!;


        public override async void OnEnter()
        {
            // 关闭启动场景的主相机，避免其与制谱器场景的相机重复渲染
            GameRoot.MainCamera?.gameObject.SetActive(false);

            try
            {
                // 打开场景并检查制谱器 SceneRoot 状态
                chartEditorSceneHandler = await GameRoot.Asset.LoadSceneAsync(ScenePath);
                if (!chartEditorSceneHandler.IsValid || !chartEditorSceneHandler.IsSuccess)
                {
                    throw new Exception($"制谱器场景加载失败：{chartEditorSceneHandler.Error}");
                }

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

                var chartModule = GameRoot.GetDataModule<ChartModule>();
                if (chartModule.SelectedChartPackIndex != null && chartModule.SelectedChartIndex != null)
                    await chartModule.LoadChartDataAsync();

                List<string> assetsToInit = ChartEditorAssetHelper.AllPaths;
                await GameRoot.Asset.BatchLoadAssetAsync(assetsToInit).BindTo(sceneRoot.gameObject);

                sceneRoot.gameObject.SetActive(true);

                // 初始化场景
                sceneRoot.InitSceneRoot();
            }
            catch
            {
                // 初始化中断时主相机仍处于关闭状态，若直接抛出异常，流程不会走到 OnExit，玩家无法恢复画面
                // 因此先恢复主相机再重新抛出
                GameRoot.MainCamera?.gameObject.SetActive(true);
                throw;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        public override void OnExit()
        {
            // 恢复主相机与背景模糊状态
            GameRoot.MainCamera?.gameObject.SetActive(true);
            ChartEditorPopupBlur.Reset();

            ChartEditorDataModule chartEditorDataModule = GameRoot.GetDataModule<ChartEditorDataModule>();
            chartEditorDataModule.OnExitChartEditorProcedure();
            GameRoot.Asset.UnloadScene(chartEditorSceneHandler);
        }
    }
}
