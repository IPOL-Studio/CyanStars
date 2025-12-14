#nullable enable

using System;
using System.IO;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Framework.FSM;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Gameplay.ChartEditor.View;
using UnityEngine;
using UnityEngine.SceneManagement;
using CyanStars.Utils;

namespace CyanStars.Gameplay.ChartEditor.Procedure
{
    [ProcedureState]
    public class ChartEditorProcedure : BaseState
    {
        private const string ScenePath = "Assets/BundleRes/Scenes/ChartEditor.unity";
        private const string SceneMainCanvaGameObjectName = "MainCanva";

        private Scene scene;
        private Canvas canvas;


        public override async void OnEnter()
        {
            // GameRoot.MainCamera.gameObject.SetActive(false);
            scene = (await GameRoot.Asset.LoadSceneAsync(ScenePath)).Scene;

            int foundCount = 0;
            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                if (rootGameObject.name == SceneMainCanvaGameObjectName)
                {
                    canvas = rootGameObject.GetComponent<Canvas>();
                    foundCount++;
                }
            }

            if (foundCount != 1)
            {
                throw new Exception("未找到制谱器主 Canvas 或找到了多个");
            }

            // 创建制谱器 Model，并为所有 View 添加绑定
            ChartModule module = GameRoot.GetDataModule<ChartModule>();
// Debug
            Debug.LogWarning("作为调试生成了默认谱包，正式发布时请修改");
            if (Directory.Exists(PathUtil.Combine(Application.persistentDataPath, "ChartPacks")))
                Directory.Delete(PathUtil.Combine(Application.persistentDataPath, "ChartPacks"), true);
            module.CreateAndSelectNewChartPack("Test", out _); // TODO: 改用从 UI 获取
// EndDebug
            string workspacePath = module.SelectedRuntimeChartPack.WorkspacePath;
            string chartPackFilePath = PathUtil.Combine(workspacePath, ChartModule.ChartPackFileName);

            // 通过序列化获取深拷贝的谱包数据
            ChartPackData chartPackData;
            chartPackData = (await GameRoot.Asset.LoadAssetAsync<ChartPackData>(chartPackFilePath)).Asset;
            ChartMetadata metadata =
                module.SelectedRuntimeChartPack.ChartPackData.ChartMetaDatas[(int)module.SelectedChartIndex];
            string chartFilePath = PathUtil.Combine(workspacePath, metadata.FilePath);
            await module.GetChartDataFromDisk(module.SelectedRuntimeChartPack, (int)module.SelectedChartIndex);
            ChartData chartData = module.ChartData;
            ChartEditorModel chartEditorModel =
                await ChartEditorModel.CreateEditorModel(workspacePath, chartFilePath, chartPackData, chartData);

            foreach (var baseView in canvas.gameObject.GetComponentsInChildren<BaseView>())
            {
                // 为已有的静态 view 进行绑定，动态 view 需要在生成时由母 view 重新绑定
                try
                {
                    baseView.Bind(chartEditorModel);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        public override void OnUpdate(float deltaTime)
        {
        }

        public override void OnExit()
        {
            // GameRoot.MainCamera.gameObject.SetActive(true);
        }
    }
}
