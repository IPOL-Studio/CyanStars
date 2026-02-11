#nullable enable

using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Gameplay.ChartEditor.View;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor
{
    public class MvvmBindManager : MonoBehaviour
    {
        private readonly CompositeDisposable Disposables = new CompositeDisposable();


        [SerializeField]
        private ToolbarView toolbarView = null!;

        [SerializeField]
        private MenuButtonsView menuButtonsView = null!;

        [SerializeField]
        private EditorAttributeView editorAttributeView = null!;

        [SerializeField]
        private NoteAttributeView noteAttributeView = null!;

        [SerializeField]
        private ChartPackDataView chartPackDataView = null!;

        [SerializeField]
        private ChartPackDataCoverView chartPackDataCoverView = null!;

        [SerializeField]
        private ChartDataView chartDataView = null!;

        [SerializeField]
        private ChartPackDataCoverCropFrameView cropFrameView = null!;

        [SerializeField]
        private List<ChartPackDataCoverCropHandlerView> cropHandlerViews = null!;

        [SerializeField]
        private MusicVersionView musicVersionView = null!;

        [SerializeField]
        private BpmGroupView bpmGroupView = null!;

        [SerializeField]
        private EditAreaView editAreaView = null!;

        [SerializeField]
        private PopupView popupView = null!;


        /// <summary>
        /// 创建 Model 、ViewModel 并启动绑定
        /// </summary>
        /// <remarks>注意：由于引用关系，制谱器会修改传入的谱包和谱面实例内的数据。请先深拷贝一个谱包和谱面，再调用制谱器初始化</remarks>
        public void StartBind(string workspacePath,
                              int chartMetadataIndex,
                              ChartPackData chartPackData,
                              ChartData chartData,
                              ChartEditorMusicManager musicManager)
        {
            // TODO: 为 Model 实现 IDispose，以进一步管理生命周期
            ChartEditorModel model =
                new ChartEditorModel(workspacePath, chartMetadataIndex, chartPackData, chartData);

            musicManager.Init(model);

            var toolbarViewModel = new ToolbarViewModel(model).AddTo(Disposables);
            toolbarView.Bind(toolbarViewModel);

            var menuButtonsViewModel = new MenuButtonsViewModel(model).AddTo(Disposables);
            menuButtonsView.Bind(menuButtonsViewModel);

            var editorAttributeViewModel = new EditorAttributeViewModel(model).AddTo(Disposables);
            editorAttributeView.Bind(editorAttributeViewModel);

            var noteAttributeViewModel = new NoteAttributeViewModel(model).AddTo(Disposables);
            noteAttributeView.Bind(noteAttributeViewModel);

            var chartPackDataViewModel = new ChartPackDataViewModel(model).AddTo(Disposables);
            chartPackDataView.Bind(chartPackDataViewModel);

            var chartPackDataCoverViewModel = new ChartPackDataCoverViewModel(model).AddTo(Disposables);
            chartPackDataCoverView.Bind(chartPackDataCoverViewModel);
            cropFrameView.Bind(chartPackDataCoverViewModel);
            foreach (var item in cropHandlerViews)
                item.Bind(chartPackDataCoverViewModel);

            var chartDataViewModel = new ChartDataViewModel(model).AddTo(Disposables);
            chartDataView.Bind(chartDataViewModel);

            var musicVersionViewModel = new MusicVersionViewModel(model).AddTo(Disposables);
            musicVersionView.Bind(musicVersionViewModel);

            var bpmGroupViewModel = new BpmGroupViewModel(model).AddTo(Disposables);
            bpmGroupView.Bind(bpmGroupViewModel);

            var editAreaViewModel = new EditAreaViewModel(model).AddTo(Disposables);
            editAreaView.Bind(editAreaViewModel);
        }

        /// <summary>
        /// 退出制谱器时解除所有绑定，以释放内存
        /// </summary>
        /// <remarks>VM 通过 CatAsset 加载的资源也应该在此时由 VM 管理释放</remarks>
        public void UnbindAll()
        {
            Disposables.Dispose();
        }

        private void OnDestroy()
        {
            UnbindAll();
        }
    }
}
