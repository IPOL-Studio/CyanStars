#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
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

            ViewModel.ChartDataCanvasVisibility
                .Subscribe(isVisibility =>
                    {
                        chartDataCanvas.enabled = isVisibility;
                    }
                )
                .AddTo(this);
            ViewModel.ChartDifficulty
                .Subscribe(difficulty =>
                    {
                        kuiXingToggle.isOn = difficulty == ChartDifficulty.KuiXing;
                        qiMingToggle.isOn = difficulty == ChartDifficulty.QiMing;
                        tianShuToggle.isOn = difficulty == ChartDifficulty.TianShu;
                        wuYinToggle.isOn = difficulty == ChartDifficulty.WuYin;
                        undefinedToggle.isOn = difficulty == null;
                        Debug.LogWarning("TODO: 切换图标样式"); // TODO: 切换图标样式
                    }
                )
                .AddTo(this);
            ViewModel.ChartLevelString
                .Subscribe(text => levelField.text = text)
                .AddTo(this);
            ViewModel.ReadyBeatCountString
                .Subscribe(text => readyBeatField.text = text)
                .AddTo(this);

            closeCanvasButton.onClick.AddListener(ViewModel.CloseCanvas);
            kuiXingToggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                        ViewModel.SetChartDifficulty(ChartDifficulty.KuiXing);
                }
            );
            qiMingToggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                        ViewModel.SetChartDifficulty(ChartDifficulty.QiMing);
                }
            );
            tianShuToggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                        ViewModel.SetChartDifficulty(ChartDifficulty.TianShu);
                }
            );
            wuYinToggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                        ViewModel.SetChartDifficulty(ChartDifficulty.WuYin);
                }
            );
            undefinedToggle.onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                        ViewModel.SetChartDifficulty(null);
                }
            );
            levelField.onEndEdit.AddListener(ViewModel.SetChartLevelString);
            readyBeatField.onEndEdit.AddListener(ViewModel.SetReadyBeatCount);
        }

        protected override void OnDestroy()
        {
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
