#nullable enable

using CyanStars.Chart;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CyanStars.Gameplay.ChartEditor.View
{
    public class ChartPackDataView : BasePopupView<ChartPackDataViewModel>
    {
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
        private GameObject coverCropFrameObject = null!;

        [SerializeField]
        private Button exportChartPackButton = null!;


        private ReadOnlyReactiveProperty<bool> coverCropFrameVisibility = null!;


        public override void Bind(ChartPackDataViewModel targetViewModel)
        {
            base.Bind(targetViewModel);

            coverCropFrameVisibility = ViewModel.ChartPackData
                .Select(data => data.CoverFilePath.AsObservable())
                .Switch()
                .Select(path => path != null)
                .ToReadOnlyReactiveProperty()
                .AddTo(this);

            ViewModel.ChartPackTitle
                .Subscribe(title => chartPackTitleField.text = title)
                .AddTo(this);

            ViewModel.PreviewStartBeat
                .Subscribe(beat =>
                {
                    previewStartBeatField1.text = beat.IntegerPart.ToString();
                    previewStartBeatField2.text = beat.Numerator.ToString();
                    previewStartBeatField3.text = beat.Denominator.ToString();
                })
                .AddTo(this);
            ViewModel.PreviewEndBeat
                .Subscribe(beat =>
                {
                    previewEndBeatField1.text = beat.IntegerPart.ToString();
                    previewEndBeatField2.text = beat.Numerator.ToString();
                    previewEndBeatField3.text = beat.Denominator.ToString();
                })
                .AddTo(this);

            ViewModel.CoverFilePathString
                .Subscribe(text => coverPathText.text = text)
                .AddTo(this);
            coverCropFrameVisibility
                .Subscribe(isVisible => coverCropFrameObject.SetActive(isVisible))
                .AddTo(this);


            chartPackTitleField
                .OnEndEditAsObservable()
                .Subscribe(ViewModel.SetChartPackTitle)
                .AddTo(this);

            // TODO: 用此方法统一更新预览拍
            //
            // private void UpdateBeat(string s1, string s2, string s3)
            // {
            //     Debug.LogWarning("TODO: 更新预览拍");
            // }
            //
            // Observable.Merge(
            //         previewStartBeatField1.onEndEdit.AsObservable(),
            //         previewStartBeatField2.onEndEdit.AsObservable(),
            //         previewStartBeatField3.onEndEdit.AsObservable()
            //     )
            //     .Subscribe(_ =>
            //         {
            //             UpdateBeat(previewStartBeatField1.text, previewStartBeatField2.text, previewStartBeatField3.text);
            //         }
            //     )
            //     .AddTo(this);

            previewStartBeatField1
                .OnEndEditAsObservable()
                .Subscribe(_ =>
                {
                    bool isSuccessfullyCreated = TryCreateBeat(previewStartBeatField1.text,
                        previewStartBeatField2.text,
                        previewStartBeatField3.text,
                        out Beat startBeat);

                    if (!isSuccessfullyCreated)
                        previewStartBeatField1.text = ViewModel.PreviewStartBeat.CurrentValue.IntegerPart.ToString();
                    else
                        ViewModel.SetPreviewStartBeat(startBeat);
                })
                .AddTo(this);
            previewStartBeatField2
                .OnEndEditAsObservable()
                .Subscribe(_ =>
                {
                    bool isSuccessfullyCreated = TryCreateBeat(previewStartBeatField1.text,
                        previewStartBeatField2.text,
                        previewStartBeatField3.text,
                        out Beat startBeat);

                    if (!isSuccessfullyCreated)
                        previewStartBeatField2.text = ViewModel.PreviewStartBeat.CurrentValue.Numerator.ToString();
                    else
                        ViewModel.SetPreviewStartBeat(startBeat);
                })
                .AddTo(this);
            previewStartBeatField3
                .OnEndEditAsObservable()
                .Subscribe(_ =>
                {
                    bool isSuccessfullyCreated = TryCreateBeat(previewStartBeatField1.text,
                        previewStartBeatField2.text,
                        previewStartBeatField3.text,
                        out Beat startBeat);

                    if (!isSuccessfullyCreated)
                        previewStartBeatField3.text = ViewModel.PreviewStartBeat.CurrentValue.Denominator.ToString();
                    else
                        ViewModel.SetPreviewStartBeat(startBeat);
                })
                .AddTo(this);

            previewEndBeatField1
                .OnEndEditAsObservable()
                .Subscribe(_ =>
                {
                    bool isSuccessfullyCreated = TryCreateBeat(previewEndBeatField1.text,
                        previewEndBeatField2.text,
                        previewEndBeatField3.text,
                        out Beat endBeat);

                    if (!isSuccessfullyCreated)
                        previewEndBeatField1.text = ViewModel.PreviewEndBeat.CurrentValue.IntegerPart.ToString();
                    else
                        ViewModel.SetPreviewEndBeat(endBeat);
                })
                .AddTo(this);
            previewEndBeatField2
                .OnEndEditAsObservable()
                .Subscribe(_ =>
                {
                    bool isSuccessfullyCreated = TryCreateBeat(previewEndBeatField1.text,
                        previewEndBeatField2.text,
                        previewEndBeatField3.text,
                        out Beat endBeat);

                    if (!isSuccessfullyCreated)
                        previewEndBeatField2.text = ViewModel.PreviewEndBeat.CurrentValue.Numerator.ToString();
                    else
                        ViewModel.SetPreviewEndBeat(endBeat);
                })
                .AddTo(this);
            previewEndBeatField3
                .OnEndEditAsObservable()
                .Subscribe(_ =>
                {
                    bool isSuccessfullyCreated = TryCreateBeat(previewEndBeatField1.text,
                        previewEndBeatField2.text,
                        previewEndBeatField3.text,
                        out Beat endBeat);

                    if (!isSuccessfullyCreated)
                        previewEndBeatField3.text = ViewModel.PreviewEndBeat.CurrentValue.Denominator.ToString();
                    else
                        ViewModel.SetPreviewEndBeat(endBeat);
                })
                .AddTo(this);

            exportChartPackButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.ExportChartPack())
                .AddTo(this);
        }

        /// <summary>
        /// 尝试将 string 转为 beat
        /// </summary>
        /// <returns>
        /// 是否成功转换
        /// </returns>
        private static bool TryCreateBeat(string integerPartString, string numeratorString, string denominatorString, out Beat beat)
        {
            if (!int.TryParse(integerPartString, out var integerPart) ||
                !int.TryParse(numeratorString, out var numerator) ||
                !int.TryParse(denominatorString, out var denominator))
            {
                Beat.TryCreateBeat(0, 0, 1, out beat);
                return false;
            }

            Beat.TryCreateBeat(integerPart, numerator, denominator, out beat);
            return true;
        }
    }
}
