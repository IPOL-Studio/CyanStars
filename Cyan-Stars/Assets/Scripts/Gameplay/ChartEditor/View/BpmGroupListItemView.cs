#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class BpmGroupListItemView : BaseView<BpmGroupListItemViewModel>
    {
        [SerializeField]
        private Toggle itemToggle;

        [SerializeField]
        private RawImage ledRawImage;

        [SerializeField]
        private TMP_Text countText;

        [SerializeField]
        private TMP_Text beatAndTimeText;


        public override void Bind(BpmGroupListItemViewModel targetViewModel)
        {
            base.Bind(targetViewModel);
        }

        protected override void OnDestroy()
        {
        }
    }
}
