#nullable enable

using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Gameplay.ChartEditor.View;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor
{
    public class MvvmBindManager : MonoBehaviour
    {
        [SerializeField]
        private List<ToolbarItemView> toolbarItemViews = null!;

        [SerializeField]
        private MenuButtonsView menuButtonsView = null!;

        [SerializeField]
        private EditorAttributeView editorAttributeView = null!;

        [SerializeField]
        private ChartPackDataView chartPackDataView = null!;

        [SerializeField]
        private List<ChartPackDataCoverCropHandlerView> cropHandlerViews = null!;

        private readonly CompositeDisposable disposables = new CompositeDisposable();


        /// <summary>
        /// 创建 Model 、ViewModel 并启动绑定
        /// </summary>
        /// <remarks>注意：由于引用关系，制谱器会修改传入的谱包和谱面实例内的数据。请先深拷贝一个谱包和谱面，再调用制谱器初始化</remarks>
        public void StartBind(string workspacePath,
                              int chartMetadataIndex,
                              ChartPackData chartPackData,
                              ChartData chartData,
                              CommandManager commandManager)
        {
            ChartEditorModel model =
                new ChartEditorModel(workspacePath, chartMetadataIndex, chartPackData, chartData);

            var toolbarViewModel = new ToolbarViewModel(model, commandManager).AddTo(disposables);
            foreach (var item in toolbarItemViews)
                item.Bind(toolbarViewModel);

            var menuButtonsViewModel = new MenuButtonsViewModel(model, commandManager).AddTo(disposables);
            menuButtonsView.Bind(menuButtonsViewModel);

            var editorAttributeViewModel = new EditorAttributeViewModel(model, commandManager).AddTo(disposables);
            editorAttributeView.Bind(editorAttributeViewModel);

            var chartPackDataViewModel = new ChartPackDataViewModel(model, commandManager).AddTo(disposables);
            chartPackDataView.Bind(chartPackDataViewModel);
            foreach (var item in cropHandlerViews)
                item.Bind(chartPackDataViewModel);
        }

        /// <summary>
        /// 退出制谱器时解除所有绑定，以释放内存
        /// </summary>
        /// <remarks>VM 通过 CatAsset 加载的资源也应该在此释放</remarks>
        public void UnbindAll()
        {
            disposables.Dispose();
        }
    }
}
