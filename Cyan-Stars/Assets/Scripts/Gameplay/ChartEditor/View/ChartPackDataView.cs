#nullable enable

using CyanStars.Gameplay.ChartEditor.ViewModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class ChartPackDataView : BaseView<ChartPackDataViewModel>
    {
        [SerializeField]
        private Canvas canvas = null!;

        [SerializeField]
        private TMP_InputField chartPackTitleField = null!;

        [SerializeField]
        private TMP_InputField previewStartBeatField1 = null!;

        [SerializeField]
        private TMP_InputField previewStartBeatField2 = null!;

        [SerializeField]
        private TMP_InputField previewStartBeatField3 = null!;

        [SerializeField]
        private TMP_InputField previewEndBeatField1 = null!;

        [SerializeField]
        private TMP_InputField previewEndBeatField2 = null!;

        [SerializeField]
        private TMP_InputField previewEndBeatField3 = null!;

        [SerializeField]
        private TMP_Text coverPathText = null!;

        [SerializeField]
        private Button importCoverButton = null!;

        [SerializeField]
        private Button exportChartPackButton = null!;


        protected override void OnDestroy()
        {
        }
    }
}
