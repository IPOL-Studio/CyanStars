#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class SpeedTemplateListItemView : BaseView<SpeedTemplateListItemViewModel>
    {
        [SerializeField]
        private Button itemButton = null!;

        [SerializeField]
        private Image ledImage = null!;

        [SerializeField]
        private TMP_Text countText = null!;

        [SerializeField]
        private TMP_Text remarkText = null!;


        public override void Bind(SpeedTemplateListItemViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            ViewModel.IsSelected
                .Subscribe(isSelected => ledImage.enabled = isSelected)
                .AddTo(this);
            ViewModel.ItemIndex
                .Subscribe(index => countText.text = $"#{index + 1}")
                .AddTo(this);
            ViewModel.PropertyUpdatedSubject
                .Subscribe(data => remarkText.text = data.Remark)
                .AddTo(this);

            itemButton.OnClickAsObservable()
                .Subscribe(_ => ViewModel.OnClick())
                .AddTo(this);
        }
    }
}
