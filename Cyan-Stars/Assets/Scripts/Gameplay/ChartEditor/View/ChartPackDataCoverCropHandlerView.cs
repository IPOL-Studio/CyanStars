#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class ChartPackDataCoverCropHandlerView : BaseView<ChartPackDataViewModel>
    {
        [SerializeField]
        private CoverCropHandlerType type;

        [SerializeField]
        private RectTransform transform;


        public override void Bind(ChartPackDataViewModel targetViewModel)
        {
            base.Bind(targetViewModel);


        }

        protected override void OnDestroy()
        {
        }
    }
}
