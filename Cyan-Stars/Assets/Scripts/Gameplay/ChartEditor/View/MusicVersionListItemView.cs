#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class MusicVersionListItemView : BaseView<MusicVersionListItemViewModel>
    {
        [SerializeField]
        private Toggle itemToggle = null!;

        [SerializeField]
        private Image ledImage = null!;

        [SerializeField]
        private TMP_Text titleText = null!;


        public override void Bind(MusicVersionListItemViewModel targetViewModel)
        {
            base.Bind(targetViewModel);
        }

        protected override void OnDestroy()
        {
        }
    }
}
