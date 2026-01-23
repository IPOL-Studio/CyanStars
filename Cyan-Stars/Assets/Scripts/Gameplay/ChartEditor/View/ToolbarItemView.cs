#nullable enable

using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using UnityEngine;
using UnityEngine.UI;
using R3;

namespace CyanStars.Gameplay.ChartEditor.View
{
    /// <summary>
    /// 每个工具按 Toggle 持有一个 V。
    /// </summary>
    public class ToolbarItemView : BaseView<ToolbarViewModel>
    {
        [SerializeField]
        private Toggle toggle = null!;

        [SerializeField]
        private EditToolType editToolType;

        public override void Bind(ToolbarViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ViewModel.SelectedEditTool
                .Subscribe(type =>
                {
                    toggle.SetIsOnWithoutNotify(type == editToolType);
                })
                .AddTo(this);

            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool isOn)
        {
            if (isOn)
                ViewModel.SelectedEditTool.Value = editToolType;
        }

        protected override void OnDestroy()
        {
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
    }
}
