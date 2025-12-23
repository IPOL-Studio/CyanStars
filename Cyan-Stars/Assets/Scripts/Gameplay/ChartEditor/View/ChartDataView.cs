#nullable enable

using System;
using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class ChartDataView : BaseView<ChartDataViewModel>
    {
        [SerializeField]
        private Canvas chartDataCanvas = null!;

        [SerializeField]
        private Button closeCanvasButton = null!;

        [SerializeField]
        private Toggle kuiXingToggle = null!;

        [SerializeField]
        private Toggle qiMingToggle = null!;

        [SerializeField]
        private Toggle tianShuToggle = null!;

        [SerializeField]
        private Toggle wuYinToggle = null!;

        [SerializeField]
        private Toggle undefinedToggle = null!;

        [SerializeField]
        private TMP_InputField levelField = null!;

        [SerializeField]
        private TMP_InputField readyBeatField = null!;


        public override void Bind(ChartDataViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            chartDataCanvas.enabled = ViewModel.ChartDataCanvasVisibility.Value;

            ViewModel.ChartDataCanvasVisibility.OnValueChanged += SetCanvasVisibility;
            ViewModel.ChartDifficulty.OnValueChanged += SetDifficulty;
            ViewModel.ChartLevelString.OnValueChanged += SetLevel;
            ViewModel.ReadyBeatCountString.OnValueChanged += SetReadyBeatCount;

            closeCanvasButton.onClick.AddListener(ViewModel.CloseCanvas);
            kuiXingToggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    ViewModel.SetChartDifficulty(ChartDifficulty.KuiXing);
                }
            });
            qiMingToggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    ViewModel.SetChartDifficulty(ChartDifficulty.QiMing);
                }
            });
            tianShuToggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    ViewModel.SetChartDifficulty(ChartDifficulty.TianShu);
                }
            });
            wuYinToggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    ViewModel.SetChartDifficulty(ChartDifficulty.WuYin);
                }
            });
            undefinedToggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                {
                    ViewModel.SetChartDifficulty(null);
                }
            });
            levelField.onValueChanged.AddListener(ViewModel.SetChartLevelString);
            readyBeatField.onValueChanged.AddListener(ViewModel.SetReadyBeatCount);
        }

        private void SetCanvasVisibility(bool visible)
        {
            chartDataCanvas.enabled = visible;
        }

        private void SetDifficulty(ChartDifficulty? difficulty)
        {
            kuiXingToggle.isOn = difficulty == ChartDifficulty.KuiXing;
            qiMingToggle.isOn = difficulty == ChartDifficulty.QiMing;
            tianShuToggle.isOn = difficulty == ChartDifficulty.TianShu;
            wuYinToggle.isOn = difficulty == ChartDifficulty.WuYin;
            undefinedToggle.isOn = difficulty == null;
            Debug.LogWarning("TODO: 切换图标样式"); // TODO: 切换图标样式
        }

        private void SetLevel(string levelString)
        {
            levelField.text = levelString;
        }

        private void SetReadyBeatCount(string readyBeatCountString)
        {
            readyBeatField.text = readyBeatCountString;
        }

        protected override void OnDestroy()
        {
            ViewModel.ChartDataCanvasVisibility.OnValueChanged -= SetCanvasVisibility;
            ViewModel.ChartDifficulty.OnValueChanged -= SetDifficulty;
            ViewModel.ChartLevelString.OnValueChanged -= SetLevel;
            ViewModel.ReadyBeatCountString.OnValueChanged -= SetReadyBeatCount;

            closeCanvasButton.onClick.RemoveAllListeners();
            kuiXingToggle.onValueChanged.RemoveAllListeners();
            qiMingToggle.onValueChanged.RemoveAllListeners();
            tianShuToggle.onValueChanged.RemoveAllListeners();
            wuYinToggle.onValueChanged.RemoveAllListeners();
            undefinedToggle.onValueChanged.RemoveAllListeners();
            levelField.onValueChanged.RemoveAllListeners();
            readyBeatField.onValueChanged.RemoveAllListeners();
        }
    }
}
