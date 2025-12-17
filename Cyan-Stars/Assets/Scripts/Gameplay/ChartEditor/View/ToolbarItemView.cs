#nullable enable

using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class ToolbarItemView : BaseView<ToolbarViewModel>
    {
        [SerializeField]
        private Toggle toggle = null!;

        [SerializeField]
        private EditToolType editToolType;


        public override void Bind(ToolbarViewModel viewModel)
        {
            base.Bind(viewModel);

            toggle.isOn = ViewModel.SelectedEditTool.Value == editToolType;

            ViewModel.SelectedEditTool.OnValueChanged += RefreshUI;
            toggle.onValueChanged.AddListener((isOn) =>
            {
                if (!isOn)
                {
                    return;
                }

                ViewModel.SelectTool(this.editToolType);
            });
        }

        private void RefreshUI(EditToolType type)
        {
            if (this.editToolType == type && !toggle.isOn)
            {
                toggle.isOn = true;
            }
        }

        protected override void OnDestroy()
        {
            ViewModel.SelectedEditTool.OnValueChanged += RefreshUI;
            toggle.onValueChanged.RemoveAllListeners();
        }
    }
}
