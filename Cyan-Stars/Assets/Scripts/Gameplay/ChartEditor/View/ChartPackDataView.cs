#nullable enable

using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.ViewModel;
using R3;
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
        private Button closeCanvasButton = null!;

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
        private Button exportChartPackButton = null!; //TODO


        private readonly ReactiveProperty<bool> canvasVisibility = new ReactiveProperty<bool>(false);
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


            canvasVisibility
                .Subscribe(visible => canvas.enabled = visible)
                .AddTo(this);
            ViewModel.ChartPackTitle
                .Subscribe(title => chartPackTitleField.text = title)
                .AddTo(this);

            ViewModel.PreviewStartBeatField1String
                .Subscribe(text => previewStartBeatField1.text = text)
                .AddTo(this);
            ViewModel.PreviewStartBeatField2String
                .Subscribe(text => previewStartBeatField2.text = text)
                .AddTo(this);
            ViewModel.PreviewStartBeatField3String
                .Subscribe(text => previewStartBeatField3.text = text)
                .AddTo(this);
            ViewModel.PreviewEndBeatField1String
                .Subscribe(text => previewEndBeatField1.text = text)
                .AddTo(this);
            ViewModel.PreviewEndBeatField2String
                .Subscribe(text => previewEndBeatField2.text = text)
                .AddTo(this);
            ViewModel.PreviewEndBeatField3String
                .Subscribe(text => previewEndBeatField3.text = text)
                .AddTo(this);

            ViewModel.CoverFilePathString
                .Subscribe(text => coverPathText.text = text)
                .AddTo(this);
            coverCropFrameVisibility
                .Subscribe(isVisible => coverCropFrameObject.SetActive(isVisible))
                .AddTo(this);


            closeCanvasButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                    {
                        if (!canvasVisibility.CurrentValue)
                            return;

                        GameRoot.GetDataModule<ChartEditorDataModule>().CommandStack.ExecuteCommand(
                            new DelegateCommand(
                                () => canvasVisibility.Value = false,
                                () => canvasVisibility.Value = true
                            )
                        );
                    }
                )
                .AddTo(this);
            chartPackTitleField
                .OnEndEditAsObservable()
                .Subscribe(ViewModel.SetChartPackTitle)
                .AddTo(this);

            // TODO: 用此方法统一更新预览拍
            //
            // private void UpdateBeat(string s1, string s2, string s3)
            // {
            //     Debug.LogWarning("TODO: 更新预览拍");// TODO: 更新预览拍
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
                    ViewModel.SetPreviewStartBeat(
                        previewStartBeatField1.text,
                        previewStartBeatField2.text,
                        previewStartBeatField3.text
                    )
                )
                .AddTo(this);
            previewStartBeatField2
                .OnEndEditAsObservable()
                .Subscribe(_ =>
                    ViewModel.SetPreviewStartBeat(
                        previewStartBeatField1.text,
                        previewStartBeatField2.text,
                        previewStartBeatField3.text
                    )
                )
                .AddTo(this);
            previewStartBeatField3
                .OnEndEditAsObservable()
                .Subscribe(_ =>
                    ViewModel.SetPreviewStartBeat(
                        previewStartBeatField1.text,
                        previewStartBeatField2.text,
                        previewStartBeatField3.text
                    )
                )
                .AddTo(this);
            previewEndBeatField1
                .OnEndEditAsObservable()
                .Subscribe(_ =>
                    ViewModel.SetPreviewEndBeat(
                        previewEndBeatField1.text,
                        previewEndBeatField2.text,
                        previewEndBeatField3.text
                    )
                )
                .AddTo(this);
            previewEndBeatField2
                .OnEndEditAsObservable()
                .Subscribe(_ =>
                    ViewModel.SetPreviewEndBeat(
                        previewEndBeatField1.text,
                        previewEndBeatField2.text,
                        previewEndBeatField3.text
                    )
                )
                .AddTo(this);
            previewEndBeatField3
                .OnEndEditAsObservable()
                .Subscribe(_ =>
                    ViewModel.SetPreviewEndBeat(
                        previewEndBeatField1.text,
                        previewEndBeatField2.text,
                        previewEndBeatField3.text
                    )
                )
                .AddTo(this);

            exportChartPackButton
                .OnClickAsObservable()
                .Subscribe(_ => ViewModel.ExportChartPack())
                .AddTo(this);
        }


        public void OpenCanvas()
        {
            if (canvasVisibility.CurrentValue)
                return;

            GameRoot.GetDataModule<ChartEditorDataModule>().CommandStack.ExecuteCommand(
                new DelegateCommand(
                    () => canvasVisibility.Value = true,
                    () => canvasVisibility.Value = false
                )
            );
        }


        protected override void OnDestroy()
        {
        }
    }
}
