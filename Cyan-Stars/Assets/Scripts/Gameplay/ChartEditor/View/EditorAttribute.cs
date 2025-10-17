using System.Globalization;
using CyanStars.GamePlay.ChartEditor.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.GamePlay.ChartEditor.View
{
    public class EditorAttribute : BaseView
    {
        private const int BeatAccuracyStep = 1;
        private const float BeatZoomStep = 0.2f;

        [SerializeField]
        private TMP_InputField posAccuracyField;

        [SerializeField]
        private Toggle posMagnet;

        [SerializeField]
        private TMP_InputField beatAccuracyField;

        [SerializeField]
        private Button beatAccuracySub;

        [SerializeField]
        private Button beatAccuracyAdd;

        [SerializeField]
        private TMP_InputField beatZoomField;

        [SerializeField]
        private Button beatZoomOut;

        [SerializeField]
        private Button beatZoomIn;

        public override void Bind(ChartEditorModel chartEditorModel)
        {
            base.Bind(chartEditorModel);

            // 初始化字段
            posAccuracyField.text = Model.PosAccuracy.ToString();
            posMagnet.isOn = Model.PosMagnetState;
            beatAccuracyField.text = Model.BeatAccuracy.ToString();
            beatZoomField.text = Model.BeatZoom.ToString(CultureInfo.InvariantCulture);

            // 绑定 UI 事件响应
            posAccuracyField.onEndEdit.AddListener((text) => { Model.SetPosAccuracy(text); });
            posMagnet.onValueChanged.AddListener((isOn) => { Model.SetPosMagnetState(isOn); });
            beatAccuracyField.onEndEdit.AddListener((text) => { Model.SetBeatAccuracy(text); });
            beatAccuracySub.onClick.AddListener(() =>
            {
                Model.SetBeatAccuracy(
                    (Model.BeatAccuracy - BeatAccuracyStep).ToString(CultureInfo.InvariantCulture));
            });
            beatAccuracyAdd.onClick.AddListener(() =>
            {
                Model.SetBeatAccuracy(
                    (Model.BeatAccuracy + BeatAccuracyStep).ToString(CultureInfo.InvariantCulture));
            });
            beatZoomField.onEndEdit.AddListener((text) => { Model.SetBeatZoom(text); });
            beatZoomOut.onClick.AddListener(() =>
            {
                Model.SetBeatZoom((Model.BeatZoom - BeatZoomStep).ToString(CultureInfo.InvariantCulture));
            });
            beatZoomIn.onClick.AddListener(() =>
            {
                Model.SetBeatZoom((Model.BeatZoom + BeatZoomStep).ToString(CultureInfo.InvariantCulture));
            });

            // 绑定 M 层事件响应
            Model.OnEditorAttributeChanged += EditorAttributeChanged;
            Model.OnSelectedNotesChanged += SelectedNotesChanged;
        }

        private void SelectedNotesChanged()
        {
            // 只有未选中音符，才展示编辑器属性（否则展示 Note 属性）
            foreach (Transform child in this.transform)
            {
                child.gameObject.SetActive(Model.SelectedNotes.Count == 0);
            }
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
            if (Model != null)
            {
                Model.OnEditorAttributeChanged -= EditorAttributeChanged;
            }
        }
    }
}
