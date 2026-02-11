#nullable enable

using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor.Command;
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
        private Sprite selectedToggleSprite = null!;

        [SerializeField]
        private Sprite unselectedToggleSprite = null!;


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


        private readonly ReactiveProperty<bool> CanvasVisibility = new ReactiveProperty<bool>(false);


        public override void Bind(ChartDataViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            CanvasVisibility
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

                        kuiXingToggle.image.sprite =
                            difficulty == ChartDifficulty.KuiXing
                                ? selectedToggleSprite
                                : unselectedToggleSprite;
                        qiMingToggle.image.sprite =
                            difficulty == ChartDifficulty.QiMing
                                ? selectedToggleSprite
                                : unselectedToggleSprite;
                        tianShuToggle.image.sprite =
                            difficulty == ChartDifficulty.TianShu
                                ? selectedToggleSprite
                                : unselectedToggleSprite;
                        wuYinToggle.image.sprite =
                            difficulty == ChartDifficulty.WuYin
                                ? selectedToggleSprite
                                : unselectedToggleSprite;
                        undefinedToggle.image.sprite =
                            difficulty == null
                                ? selectedToggleSprite
                                : unselectedToggleSprite;
                    }
                )
                .AddTo(this);
            ViewModel.ChartLevelString
                .Subscribe(text => levelField.text = text)
                .AddTo(this);
            ViewModel.ReadyBeatCountString
                .Subscribe(text => readyBeatField.text = text)
                .AddTo(this);

            closeCanvasButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                    {
                        if (!CanvasVisibility.CurrentValue)
                            return;

                        GameRoot.GetDataModule<ChartEditorDataModule>().CommandStack.ExecuteCommand(
                            () => CanvasVisibility.Value = false,
                            () => CanvasVisibility.Value = true
                        );
                    }
                )
                .AddTo(this);
            kuiXingToggle
                .OnValueChangedAsObservable()
                .Subscribe(isOn =>
                    {
                        if (isOn)
                            ViewModel.SetChartDifficulty(ChartDifficulty.KuiXing);
                    }
                )
                .AddTo(this);
            qiMingToggle
                .OnValueChangedAsObservable()
                .Subscribe(isOn =>
                    {
                        if (isOn)
                            ViewModel.SetChartDifficulty(ChartDifficulty.QiMing);
                    }
                )
                .AddTo(this);
            tianShuToggle.OnValueChangedAsObservable()
                .Subscribe(isOn =>
                    {
                        if (isOn)
                            ViewModel.SetChartDifficulty(ChartDifficulty.TianShu);
                    }
                )
                .AddTo(this);
            wuYinToggle
                .OnValueChangedAsObservable()
                .Subscribe(isOn =>
                    {
                        if (isOn)
                            ViewModel.SetChartDifficulty(ChartDifficulty.WuYin);
                    }
                )
                .AddTo(this);
            undefinedToggle.OnValueChangedAsObservable()
                .Subscribe(isOn =>
                    {
                        if (isOn)
                            ViewModel.SetChartDifficulty(null);
                    }
                )
                .AddTo(this);
            levelField
                .OnEndEditAsObservable()
                .Subscribe(ViewModel.SetChartLevelString)
                .AddTo(this);
            readyBeatField
                .OnEndEditAsObservable()
                .Subscribe(ViewModel.SetReadyBeatCount)
                .AddTo(this);
        }

        public void OpenCanvas()
        {
            if (CanvasVisibility.CurrentValue)
                return;

            GameRoot.GetDataModule<ChartEditorDataModule>().CommandStack.ExecuteCommand(
                new DelegateCommand(
                    () => CanvasVisibility.Value = true,
                    () => CanvasVisibility.Value = false
                )
            );
        }

        protected override void OnDestroy()
        {
        }
    }
}
