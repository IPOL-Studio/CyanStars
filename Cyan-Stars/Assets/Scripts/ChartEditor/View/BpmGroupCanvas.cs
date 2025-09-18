using System.Globalization;
using CyanStars.Chart;
using CyanStars.ChartEditor.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.ChartEditor.View
{
    public class BpmGroupCanvas : BaseView
    {
        [SerializeField]
        private Button closeCanvasButton;

        [SerializeField]
        private Canvas canvas;


        [SerializeField]
        private GameObject itemList;

        [SerializeField]
        private Button addItemButton;


        [SerializeField]
        private TMP_InputField bpmValueField;

        [SerializeField]
        private Button measureBpmButton;


        [SerializeField]
        private TMP_InputField startBeatField1;

        [SerializeField]
        private TMP_InputField startBeatField2;

        [SerializeField]
        private TMP_InputField startBeatField3;


        [SerializeField]
        private Button deleteItemButton;

        [SerializeField]
        private Toggle alarmToggle;

        [SerializeField]
        private Button testPlayButton;

        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);

            Model.OnBpmGroupCanvasVisiblenessChanged += RefreshUI;
            Model.OnBpmGroupChanged += RefreshUI;

            addItemButton.onClick.AddListener(() => { Model.AddBpmGroupItem(); });
            deleteItemButton.onClick.AddListener(() => { Model.DeleteBpmGroupItem(); });
            startBeatField1.onEndEdit.AddListener((_) =>
            {
                Model.UpdateBpmGroupItemBeat(startBeatField1.text, startBeatField2.text, startBeatField3.text);
            });
            startBeatField2.onEndEdit.AddListener((_) =>
            {
                Model.UpdateBpmGroupItemBeat(startBeatField1.text, startBeatField2.text, startBeatField3.text);
            });
            startBeatField3.onEndEdit.AddListener((_) =>
            {
                Model.UpdateBpmGroupItemBeat(startBeatField1.text, startBeatField2.text, startBeatField3.text);
            });
        }

        private void RefreshUI()
        {
            canvas.enabled = Model.BpmGroupCanvasVisibleness;

            if (Model.IsSimplification && Model.BpmGroupDatas.Count <= 1)
            {
                itemList.SetActive(false);
            }
            else
            {
                itemList.SetActive(true);
            }

            BpmGroupItem bpmGroupItem = Model.BpmGroupDatas[Model.SelectedBpmGroupIndex];

            bpmValueField.text = bpmGroupItem.Bpm.ToString(CultureInfo.InvariantCulture);
            startBeatField1.text = bpmGroupItem.StartBeat.IntegerPart.ToString();
            startBeatField2.text = bpmGroupItem.StartBeat.Numerator.ToString();
            startBeatField3.text = bpmGroupItem.StartBeat.Denominator.ToString();
        }

        private void OnDestroy()
        {
            Model.OnBpmGroupCanvasVisiblenessChanged -= RefreshUI;
            Model.OnBpmGroupChanged -= RefreshUI;
        }
    }
}
