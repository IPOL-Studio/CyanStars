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

            ViewModel.OnEditToolChanged += RefreshUI;
            toggle.onValueChanged.AddListener((isOn) =>
            {
                if (!isOn)
                {
                    return;
                }

                ViewModel.SelectTool(this.editToolType);
            });
        }

        private void RefreshUI(EditToolType editToolType)
        {
            if (this.editToolType == editToolType && !toggle.isOn)
            {
                toggle.isOn = true;
            }
        }

        protected override void OnDestroy()
        {
            ViewModel.OnEditToolChanged -= RefreshUI;
            toggle.onValueChanged.RemoveAllListeners();
        }
    }
}
