using System;
using System.Globalization;
using CyanStars.ChartEditor.Model;
using CyanStars.ChartEditor.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CuanStars.ChartEditor.View
{
    public class EditorAttribute : BaseView
    {
        [SerializeField]
        private TMP_InputField posAccuracyField;

        [SerializeField]
        private Toggle posMagnet;

        [SerializeField]
        private TMP_InputField beatAccuracyField;

        [SerializeField]
        private Button beatAccutacySub;

        [SerializeField]
        private Button beatAccutacyAdd;

        [SerializeField]
        private TMP_InputField beatZoomField;

        [SerializeField]
        private Button beatZoomOut;

        [SerializeField]
        private Button beatZoomIn;

        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);

            posAccuracyField.text = Model.PosAccuracy.ToString();
            posMagnet.isOn = Model.PosMagnetState;
            beatAccuracyField.text = Model.BeatAccuracy.ToString();
            beatZoomField.text = Model.BeatZoom.ToString(CultureInfo.InvariantCulture);

            posAccuracyField.onEndEdit.AddListener((text) => { Model.SetPosAccuracy(text); });
            posMagnet.onValueChanged.AddListener((isOn) => { Model.SetPosMagnetState(isOn); });
            beatAccuracyField.onEndEdit.AddListener((text) => { Model.SetBeatAccuracy(text); });
            beatAccutacySub.onClick.AddListener(() => { Model.SetBeatAccuracy((Model.BeatAccuracy - 1).ToString()); });
            beatAccutacyAdd.onClick.AddListener(() => { Model.SetBeatAccuracy((Model.BeatAccuracy + 1).ToString()); });
            beatZoomField.onEndEdit.AddListener((text) => { Model.SetBeatZoom(text); });
            beatZoomOut.onClick.AddListener(() =>
            {
                Model.SetBeatZoom((Model.BeatZoom - 0.2f).ToString(CultureInfo.InvariantCulture));
            });
            beatZoomIn.onClick.AddListener(() =>
            {
                Model.SetBeatZoom((Model.BeatZoom + 0.2f).ToString(CultureInfo.InvariantCulture));
            });

            Model.OnEditorAttributeChanged += EditorAttributeChanged;
        }

        private void EditorAttributeChanged()
        {
            if (posAccuracyField.text != Model.PosAccuracy.ToString())
            {
                posAccuracyField.text = Model.PosAccuracy.ToString();
            }

            if (posMagnet.isOn != Model.PosMagnetState)
            {
                posMagnet.isOn = Model.PosMagnetState;
            }

            if (beatAccuracyField.text != Model.BeatAccuracy.ToString())
            {
                beatAccuracyField.text = Model.BeatAccuracy.ToString();
            }

            if (beatZoomField.text != Model.BeatZoom.ToString(CultureInfo.InvariantCulture))
            {
                beatZoomField.text = Model.BeatZoom.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void OnDestroy()
        {
            Model.OnEditorAttributeChanged -= EditorAttributeChanged;
        }
    }
}
