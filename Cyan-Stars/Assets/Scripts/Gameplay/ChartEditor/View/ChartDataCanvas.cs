using System;
using CyanStars.Chart;
using CyanStars.GamePlay.ChartEditor.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.GamePlay.ChartEditor.View
{
    public class ChartDataCanvas : BaseView
    {
        [SerializeField]
        private Button closeCanvaButton;

        [SerializeField]
        private Canvas canvas;


        [SerializeField]
        private Toggle kuiXingToggle;

        [SerializeField]
        private Toggle qiMingToggle;

        [SerializeField]
        private Toggle tianShuToggle;

        [SerializeField]
        private Toggle wuYinToggle;

        [SerializeField]
        private Toggle undefinedToggle;

        [SerializeField]
        private TMP_InputField chartLevelField;

        [SerializeField]
        private TMP_InputField readyBeatField;


        public override void Bind(EditorModel editorModel)
        {
            base.Bind(editorModel);

            Model.OnChartDataChanged += RefreshUI;
            Model.OnChartDataCanvasVisiblenessChanged += RefreshUI;

            closeCanvaButton.onClick.AddListener(() => { Model.SetChartDataCanvasVisibleness(false); });

            kuiXingToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    Model.UpdateDifficulty(ChartDifficulty.KuiXing);
                }
            });
            qiMingToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    Model.UpdateDifficulty(ChartDifficulty.QiMing);
                }
            });
            tianShuToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    Model.UpdateDifficulty(ChartDifficulty.TianShu);
                }
            });
            wuYinToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    Model.UpdateDifficulty(ChartDifficulty.WuYin);
                }
            });
            undefinedToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    Model.UpdateDifficulty(null);
                }
            });

            chartLevelField.onEndEdit.AddListener((text) => { Model.UpdateLevel(text); });
            readyBeatField.onEndEdit.AddListener((text) => { Model.UpdateReadyBeat(text); });
        }

        private void RefreshUI()
        {
            // TODO
            throw new NotSupportedException();
        }

        private void OnDestroy()
        {
            Model.OnChartDataChanged -= RefreshUI;
            Model.OnChartDataCanvasVisiblenessChanged -= RefreshUI;
        }
    }
}
