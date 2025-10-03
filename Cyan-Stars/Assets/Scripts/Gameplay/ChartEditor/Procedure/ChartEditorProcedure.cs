using System;
using System.IO;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Framework.FSM;
using CyanStars.GamePlay.ChartEditor.Model;
using CyanStars.GamePlay.ChartEditor.View;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CyanStars.GamePlay.ChartEditor.Procedure
{
    [ProcedureState]
    public class ChartEditorProcedure : BaseState
    {
        private const string ScenePath = "Assets/BundleRes/Scenes/ChartEditor.unity";
        private const string SceneMainCanvaGameObjectName = "MainCanvas";

        private Scene scene;
        private Canvas canvas;


        public override async void OnEnter()
        {
            GameRoot.MainCamera.gameObject.SetActive(false);
            scene = await GameRoot.Asset.LoadSceneAsync(ScenePath);

            int foundCount = 0;
            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                if (rootGameObject.name != SceneMainCanvaGameObjectName)
                {
                    foundCount++;
                    canvas = rootGameObject.GetComponent<Canvas>();
                }
            }

            if (foundCount != 1)
            {
                throw new Exception("未找到编辑器主 Canvas 或找到了多个");
            }

            // 创建制谱器 Model，并为所有 View 添加绑定
            ChartModule module = GameRoot.GetDataModule<ChartModule>();
            string chartPackFilePath =
                Path.Combine(module.SelectedRuntimeChartPack.WorkspacePath,
                    ChartModule.ChartPackFileName);

            // TODO: 由 UI 传入选择的谱包和谱面
            // 通过序列化获取深拷贝的谱包数据
            ChartPackData chartPackData = await GameRoot.Asset.LoadAssetAsync<ChartPackData>(chartPackFilePath);
            // 这个方法会直接序列化获取深拷贝
            ChartData chartData =
                await module.GetChartDataFromDisk(module.SelectedRuntimeChartPack, module.SelectedChartIndex);
            EditorModel editorModel = new EditorModel(chartPackData, chartData);

            foreach (var baseView in canvas.gameObject.GetComponentsInChildren<BaseView>())
            {
                // 为已有的静态 view 进行绑定，动态 view 需要在生成时由母 view 重新绑定
                baseView.Bind(editorModel);
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
