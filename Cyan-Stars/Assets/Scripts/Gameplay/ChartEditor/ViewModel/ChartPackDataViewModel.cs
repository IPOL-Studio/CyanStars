#nullable enable

using System;
using System.Threading.Tasks;
using CatAsset.Runtime;
using CyanStars.Chart;
using CyanStars.Framework;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ChartPackDataViewModel : BaseViewModel
    {
        // TODO: 曲绘导入、裁剪，谱包导出还没做

        public readonly ReadOnlyReactiveProperty<bool> CanvasVisible;
        public readonly ReadOnlyReactiveProperty<string> ChartPackTitle;

        public readonly ReadOnlyReactiveProperty<string> PreviewStartBeatField1String;
        public readonly ReadOnlyReactiveProperty<string> PreviewStartBeatField2String;
        public readonly ReadOnlyReactiveProperty<string> PreviewStartBeatField3String;
        public readonly ReadOnlyReactiveProperty<string> PreviewEndBeatField1String;
        public readonly ReadOnlyReactiveProperty<string> PreviewEndBeatField2String;
        public readonly ReadOnlyReactiveProperty<string> PreviewEndBeatField3String;


        private AssetHandler<Sprite?>? coverAssetHandler;
        private readonly ReactiveProperty<Sprite?> loadedCoverSprite;

        public ReadOnlyReactiveProperty<Sprite?> LoadedCoverSprite => loadedCoverSprite;

        public readonly ReadOnlyReactiveProperty<string?> CoverFilePathString;

        public readonly ReadOnlyReactiveProperty<Vector2> CoverCropLeftTopHandlerPercentPos;
        public readonly ReadOnlyReactiveProperty<Vector2> CoverCropLeftBottomHandlerPercentPos;
        public readonly ReadOnlyReactiveProperty<Vector2> CoverCropRightTopHandlerPercentPos;
        public readonly ReadOnlyReactiveProperty<Vector2> CoverCropRightBottomHandlerPercentPos;


        public ChartPackDataViewModel(ChartEditorModel model, CommandManager commandManager)
            : base(model, commandManager)
        {
            CanvasVisible = Model.ChartPackDataCanvasVisibility
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            ChartPackTitle = Model.ChartPackData
                .Select(data => data.Title.AsObservable())
                .Switch()
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.Title.Value)
                .AddTo(base.Disposables);

            PreviewStartBeatField1String = Model.ChartPackData
                .Select(data => data.MusicPreviewStartBeat.AsObservable())
                .Switch()
                .Select(beat => beat.IntegerPart.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value.IntegerPart.ToString()
                )
                .AddTo(base.Disposables);
            PreviewStartBeatField2String = Model.ChartPackData
                .Select(data => data.MusicPreviewStartBeat.AsObservable())
                .Switch()
                .Select(beat => beat.Numerator.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value.Numerator.ToString()
                )
                .AddTo(base.Disposables);
            PreviewStartBeatField3String = Model.ChartPackData
                .Select(data => data.MusicPreviewStartBeat.AsObservable())
                .Switch()
                .Select(beat => beat.Denominator.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value.Denominator.ToString()
                )
                .AddTo(base.Disposables);
            PreviewEndBeatField1String = Model.ChartPackData
                .Select(data => data.MusicPreviewEndBeat.AsObservable())
                .Switch()
                .Select(beat => beat.IntegerPart.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value.IntegerPart.ToString()
                )
                .AddTo(base.Disposables);
            PreviewEndBeatField2String = Model.ChartPackData
                .Select(data => data.MusicPreviewEndBeat.AsObservable())
                .Switch()
                .Select(beat => beat.Numerator.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value.Numerator.ToString()
                )
                .AddTo(base.Disposables);
            PreviewEndBeatField3String = Model.ChartPackData
                .Select(data => data.MusicPreviewEndBeat.AsObservable())
                .Switch()
                .Select(beat => beat.Denominator.ToString())
                .ToReadOnlyReactiveProperty(
                    ForceUpdateEqualityComparer<string>.Instance,
                    Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value.Denominator.ToString()
                )
                .AddTo(base.Disposables);

            loadedCoverSprite = new ReactiveProperty<Sprite?>(null);
            Model.ChartPackData
                .Select(data => data.CoverFilePath.AsObservable())
                .Switch()
                .Subscribe(path => _ = LoadCoverSprite(path ?? ""))
                .AddTo(base.Disposables);
            CoverFilePathString = Model.ChartPackData
                .Select(data => data.CoverFilePath.AsObservable())
                .Switch()
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.CoverFilePath.Value ?? "")
                .AddTo(base.Disposables);
            var coverCropStartPercentPos = Model.ChartPackData
                .Select(data => data.CropStartPosition.AsObservable())
                .Switch()
                .Select(v => v ?? Vector2.zero)
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.CropStartPosition.Value ?? Vector2.zero)
                .AddTo(base.Disposables);
            var coverCropHeight = Model.ChartPackData
                .Select(data => data.CropHeight.AsObservable())
                .Switch()
                .Select(h => h ?? 0f)
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.CropHeight.Value ?? 0f)
                .AddTo(base.Disposables);
            var coverCropWidth = Model.ChartPackData
                .Select(data => data.CropWidth.AsObservable())
                .Switch()
                .Select(w => w ?? 0f)
                .ToReadOnlyReactiveProperty(Model.ChartPackData.CurrentValue.CropWidth.CurrentValue ?? 0f)
                .AddTo(base.Disposables);
            CoverCropLeftTopHandlerPercentPos = Observable.CombineLatest(
                    LoadedCoverSprite,
                    coverCropStartPercentPos,
                    coverCropHeight,
                    (sprite, startPos, height) =>
                    {
                        if (sprite is null)
                            return Vector2.zero;

                        float x = startPos.x / sprite.rect.width;
                        float y = (startPos.y + height) / sprite.rect.height;
                        return new Vector2(x, y);
                    }
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            CoverCropLeftBottomHandlerPercentPos = Observable.CombineLatest(
                    LoadedCoverSprite,
                    coverCropStartPercentPos,
                    (sprite, startPos) =>
                    {
                        if (sprite is null)
                            return Vector2.zero;

                        float x = startPos.x / sprite.rect.width;
                        float y = startPos.y / sprite.rect.height;
                        return new Vector2(x, y);
                    }
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            CoverCropRightTopHandlerPercentPos = Observable.CombineLatest(
                    LoadedCoverSprite,
                    coverCropStartPercentPos,
                    coverCropHeight,
                    coverCropWidth,
                    (sprite, startPos, height, width) =>
                    {
                        if (sprite is null)
                            return Vector2.zero;

                        float x = (startPos.x + width) / sprite.rect.width;
                        float y = (startPos.y + height) / sprite.rect.height;
                        return new Vector2(x, y);
                    }
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            CoverCropRightBottomHandlerPercentPos = Observable.CombineLatest(
                    LoadedCoverSprite,
                    coverCropStartPercentPos,
                    coverCropWidth,
                    (sprite, startPos, width) =>
                    {
                        if (sprite is null)
                            return Vector2.zero;

                        float x = (startPos.x + width) / sprite.rect.width;
                        float y = startPos.y / sprite.rect.height;
                        return new Vector2(x, y);
                    }
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
        }

        private async Task LoadCoverSprite(string filePath)
        {
            coverAssetHandler?.Unload();
            loadedCoverSprite.Value = null;
            coverAssetHandler = await GameRoot.Asset.LoadAssetAsync<Sprite?>(filePath);
            loadedCoverSprite.Value = coverAssetHandler.Asset;
        }


        public void CloseCanvas()
        {
            if (!Model.ChartPackDataCanvasVisibility.Value)
                return;

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.ChartPackDataCanvasVisibility.Value = false;
                }, () =>
                {
                    Model.ChartPackDataCanvasVisibility.Value = true;
                })
            );
        }

        public void SetChartPackTitle(string newTitle)
        {
            string oldTitle = Model.ChartPackData.CurrentValue.Title.Value;
            if (newTitle == oldTitle)
                return;

            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.ChartPackData.CurrentValue.Title.Value = newTitle;
                }, () =>
                {
                    Model.ChartPackData.CurrentValue.Title.Value = oldTitle;
                })
            );
        }

        public void SetPreviewStartBeat(string integerPartString, string numeratorString, string denominatorString)
        {
            if (!int.TryParse(integerPartString, out var integerPart) ||
                !int.TryParse(numeratorString, out var numerator) ||
                !int.TryParse(denominatorString, out var denominator))
            {
                Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.ForceNotify();
                return;
            }

            if (!Beat.TryCreateBeat(integerPart, numerator, denominator, out Beat newBeat))
            {
                Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.ForceNotify();
                return;
            }

            if (newBeat > Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value)
            {
                Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.ForceNotify();
                return;
            }

            var oldBeat = Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value;
            if (newBeat == oldBeat)
                return;
            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value = newBeat;
                }, () =>
                {
                    Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value = oldBeat;
                })
            );
        }

        public void SetPreviewEndBeat(string integerPartString, string numeratorString, string denominatorString)
        {
            if (!int.TryParse(integerPartString, out var integerPart) ||
                !int.TryParse(numeratorString, out var numerator) ||
                !int.TryParse(denominatorString, out var denominator))
            {
                Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.ForceNotify();
                return;
            }

            if (!Beat.TryCreateBeat(integerPart, numerator, denominator, out Beat newBeat))
            {
                Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.ForceNotify();
                return;
            }

            if (newBeat < Model.ChartPackData.CurrentValue.MusicPreviewStartBeat.Value)
            {
                Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.ForceNotify();
                return;
            }

            var oldBeat = Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value;
            if (newBeat == oldBeat)
                return;
            CommandManager.ExecuteCommand(new DelegateCommand(() =>
                {
                    Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value = newBeat;
                }, () =>
                {
                    Model.ChartPackData.CurrentValue.MusicPreviewEndBeat.Value = oldBeat;
                })
            );
        }

        /// <summary>
        /// 将传入的百分比坐标转换为裁剪开始的像素坐标和裁剪高度
        /// </summary>
        /// <param name="handlerType">handler 位置类型</param>
        /// <param name="percentPos">handler 百分比坐标</param>
        public void SetCoverCropHandlerPos(CoverCropHandlerType handlerType, Vector2 percentPos)
        {
            if (LoadedCoverSprite.CurrentValue == null)
                throw new InvalidOperationException("不允许在未加载曲绘时设置裁剪框位置");

            Vector2 coverPixel = new Vector2(
                LoadedCoverSprite.CurrentValue.rect.width,
                LoadedCoverSprite.CurrentValue.rect.height
            );

            // 根据 handler 类型计算裁剪开始位置和裁剪高度，CropWidth 自动变为 CropHeight 的 4 倍
            switch (handlerType)
            {
                case CoverCropHandlerType.LeftTop:
                    float height1 =
                        percentPos.y * coverPixel.y -
                        (Model.ChartPackData.CurrentValue.CropStartPosition.CurrentValue?.y ?? 0.0f);
                    height1 = Mathf.Min(Mathf.Max(0.0f, height1), coverPixel.y);
                    height1 = Mathf.Min(height1, coverPixel.x / 4.0f);
                    Model.ChartPackData.CurrentValue.CropHeight.Value = height1;

                    float startX1 = percentPos.x * coverPixel.x;
                    float startY1 = Model.ChartPackData.CurrentValue.CropStartPosition.CurrentValue?.y ?? 0.0f;

                    startX1 = Mathf.Min(Mathf.Max(0.0f, startX1), coverPixel.x);
                    startY1 = Mathf.Min(Mathf.Max(0.0f, startY1), coverPixel.y);

                    Model.ChartPackData.CurrentValue.CropStartPosition.Value = new Vector2(startX1, startY1);
                    break;
                case CoverCropHandlerType.LeftBottom:
                    float height2 =
                        (Model.ChartPackData.CurrentValue.CropStartPosition.Value?.y ?? 0.0f) +
                        (Model.ChartPackData.CurrentValue.CropHeight.Value ?? 0.0f) -
                        percentPos.y * coverPixel.y;
                    height2 = Mathf.Min(Mathf.Max(0.0f, height2), coverPixel.y);
                    height2 = Mathf.Min(height2, coverPixel.x / 4.0f);
                    Model.ChartPackData.CurrentValue.CropHeight.Value = height2;

                    float startX2 = percentPos.x * coverPixel.x;
                    float startY2 = percentPos.y * coverPixel.y;

                    startX2 = Mathf.Min(Mathf.Max(0.0f, startX2), coverPixel.x);
                    startY2 = Mathf.Min(Mathf.Max(0.0f, startY2), coverPixel.y);

                    Model.ChartPackData.CurrentValue.CropStartPosition.Value = new Vector2(startX2, startY2);
                    break;
                case CoverCropHandlerType.RightBottom:
                    float height3 =
                        (Model.ChartPackData.CurrentValue.CropStartPosition.Value?.y ?? 0.0f) +
                        (Model.ChartPackData.CurrentValue.CropHeight.Value ?? 0.0f) -
                        percentPos.y * coverPixel.y;
                    height3 = Mathf.Min(Mathf.Max(0.0f, height3), coverPixel.y);
                    height3 = Mathf.Min(height3, coverPixel.x / 4.0f);
                    Model.ChartPackData.CurrentValue.CropHeight.Value = height3;

                    float startX3 = Model.ChartPackData.CurrentValue.CropStartPosition.CurrentValue?.x ?? 0.0f;
                    float startY3 = percentPos.y * coverPixel.y;

                    startX3 = Mathf.Min(Mathf.Max(0.0f, startX3), coverPixel.x);
                    startY3 = Mathf.Min(Mathf.Max(0.0f, startY3), coverPixel.y);

                    Model.ChartPackData.CurrentValue.CropStartPosition.Value = new Vector2(startX3, startY3);
                    break;
                case CoverCropHandlerType.RightTop:
                    float height4 =
                        percentPos.y * coverPixel.y -
                        (Model.ChartPackData.CurrentValue.CropStartPosition.CurrentValue?.y ?? 0.0f);
                    height4 = Mathf.Min(Mathf.Max(0.0f, height4), coverPixel.y);
                    height4 = Mathf.Min(height4, coverPixel.x / 4.0f);
                    Model.ChartPackData.CurrentValue.CropHeight.Value = height4;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(handlerType), handlerType, null);
            }
        }


        public override void Dispose()
        {
            coverAssetHandler?.Unload();
            base.Dispose();
        }
    }

    public enum CoverCropHandlerType
    {
        LeftTop,
        LeftBottom,
        RightTop,
        RightBottom
    }
}
