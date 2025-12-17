#nullable enable

using System.Collections.Generic;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Gameplay.ChartEditor.View;
using CyanStars.Gameplay.ChartEditor.ViewModel;
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


        /// <summary>
        /// 创建 Model 、ViewModel 并启动绑定
        /// </summary>
        /// <remarks>注意：由于引用关系，制谱器会修改传入的谱包和谱面实例内的数据。请先深拷贝一个谱包和谱面，再调用制谱器初始化</remarks>
        public void StartBind(string workspacePath,
                              ChartMetadata chartMetadata,
                              ChartPackData chartPackData,
                              ChartData chartData,
                              CommandManager commandManager)
        {
            ChartEditorModel model = new ChartEditorModel(workspacePath, chartMetadata, chartPackData, chartData);

            var toolbarViewModel = new ToolbarViewModel(model, commandManager);
            foreach (var item in toolbarItemViews)
            {
                item.Bind(toolbarViewModel);
            }

            var menuButtonsViewModel = new MenuButtonsViewModel(model, commandManager);
            menuButtonsView.Bind(menuButtonsViewModel);

            var editorAttributeViewModel = new EditorAttributeViewModel(model, commandManager);
            editorAttributeView.Bind(editorAttributeViewModel);
        }
    }
}
