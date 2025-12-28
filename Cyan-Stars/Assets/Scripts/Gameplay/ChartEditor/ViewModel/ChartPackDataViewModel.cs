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
        /// 将传入的百分比 x 坐标转换为裁剪开始的像素坐标和裁剪高度
        /// </summary>
        /// <param name="handlerType">handler 位置类型</param>
        /// <param name="percentPosY">handler y 坐标/Image UI 高度的比值</param>
        /// <remarks>由于裁剪框宽高比恒定为 4:1，因此自动推断 handler x 坐标位置；CropWidth 只读并自动更新为 4 倍高度，无需手动更新</remarks>
        public void SetCoverCropHandlerPos(CoverCropHandlerType handlerType, float percentPosY)
        {
            if (LoadedCoverSprite.CurrentValue == null)
                throw new InvalidOperationException("不允许在未加载曲绘时设置裁剪框位置");

            percentPosY = Mathf.Min(Mathf.Max(0.0f, percentPosY), 1.0f);

            Vector2 coverPixel = new Vector2( // 曲绘原图的像素尺寸
                LoadedCoverSprite.CurrentValue.rect.width,
                LoadedCoverSprite.CurrentValue.rect.height
            );

            float currentStartX = Model.ChartPackData.CurrentValue.CropStartPosition.CurrentValue?.x ?? 0.0f;
            float currentStartY = Model.ChartPackData.CurrentValue.CropStartPosition.CurrentValue?.y ?? 0.0f;
            float currentWidth = Model.ChartPackData.CurrentValue.CropWidth.CurrentValue ?? 0.0f;
            float currentHeight = Model.ChartPackData.CurrentValue.CropHeight.CurrentValue ?? 0.0f;

            // 自动计算 percentPosX，校验并调整裁剪区域，确保不会超出曲绘范围；然后更新 Model 中的裁剪数据
            switch (handlerType)
            {
                case CoverCropHandlerType.LeftTop:
                {
                    // 获取对角点（不动点）的像素坐标
                    Vector2 rightBottomPixelPos = new Vector2(
                        x: currentStartX + currentWidth,
                        y: currentStartY
                    );

                    // 由对角点反向计算最大容许的裁剪像素高度
                    float maxCropHeight = Mathf.Min(
                        coverPixel.y - rightBottomPixelPos.y,
                        rightBottomPixelPos.x / 4.0f
                    );

                    // 计算裁剪区域像素高度并修正
                    float newCropHeight = percentPosY * coverPixel.y - rightBottomPixelPos.y;
                    newCropHeight = Mathf.Min(Mathf.Max(0.0f, newCropHeight), maxCropHeight);

                    // 计算裁剪起始像素位置
                    Vector2 newCropStartPos = new Vector2(
                        x: rightBottomPixelPos.x - newCropHeight * 4.0f,
                        y: rightBottomPixelPos.y
                    );

                    // 更新 Model 中的裁剪数据
                    Model.ChartPackData.CurrentValue.CropStartPosition.Value = newCropStartPos;
                    Model.ChartPackData.CurrentValue.CropHeight.Value = newCropHeight;
                    break;
                }
                case CoverCropHandlerType.LeftBottom:
                {
                    // 获取对角点（不动点）的像素坐标
                    Vector2 rightTopPixelPos = new Vector2(
                        x: currentStartX + currentWidth,
                        y: currentStartY + currentHeight
                    );

                    // 由对角点反向计算最大容许的裁剪像素高度
                    float maxCropHeight = Mathf.Min(
                        rightTopPixelPos.y,
                        rightTopPixelPos.x / 4.0f
                    );

                    // 计算裁剪区域像素高度并修正
                    float newCropHeight = rightTopPixelPos.y - percentPosY * coverPixel.y;
                    newCropHeight = Mathf.Min(Mathf.Max(0.0f, newCropHeight), maxCropHeight);

                    // 计算裁剪起始像素位置
                    Vector2 newCropStartPos = new Vector2(
                        x: rightTopPixelPos.x - newCropHeight * 4.0f,
                        y: rightTopPixelPos.y - newCropHeight
                    );

                    Model.ChartPackData.CurrentValue.CropStartPosition.Value = newCropStartPos;
                    Model.ChartPackData.CurrentValue.CropHeight.Value = newCropHeight;
                    break;
                }
                case CoverCropHandlerType.RightTop:
                {
                    // 获取对角点（不动点）的像素坐标
                    Vector2 leftBottomPixelPos = new Vector2(
                        x: currentStartX,
                        y: currentStartY
                    );

                    // 由对角点反向计算最大容许的裁剪像素高度
                    float maxCropHeight = Mathf.Min(
                        coverPixel.y - leftBottomPixelPos.y,
                        (coverPixel.x - leftBottomPixelPos.x) / 4.0f
                    );

                    // 计算裁剪区域像素高度并修正
                    float newCropHeight = percentPosY * coverPixel.y - leftBottomPixelPos.y;
                    newCropHeight = Mathf.Min(Mathf.Max(0.0f, newCropHeight), maxCropHeight);

                    // 计算裁剪起始像素位置
                    Vector2 newCropStartPos = leftBottomPixelPos;

                    Model.ChartPackData.CurrentValue.CropStartPosition.Value = newCropStartPos;
                    Model.ChartPackData.CurrentValue.CropHeight.Value = newCropHeight;
                    break;
                }
                case CoverCropHandlerType.RightBottom:
                {
                    // 获取对角点（不动点）的像素坐标
                    Vector2 leftTopPixelPos = new Vector2(
                        x: currentStartX,
                        y: currentStartY + currentHeight
                    );

                    // 由对角点反向计算最大容许的裁剪像素高度
                    float maxCropHeight = Mathf.Min(
                        leftTopPixelPos.y,
                        (coverPixel.x - leftTopPixelPos.x) / 4.0f
                    );

                    // 计算裁剪区域像素高度并修正
                    float newCropHeight = leftTopPixelPos.y - percentPosY * coverPixel.y;
                    newCropHeight = Mathf.Min(Mathf.Max(0.0f, newCropHeight), maxCropHeight);

                    // 计算裁剪起始像素位置
                    Vector2 newCropStartPos = new Vector2(
                        x: leftTopPixelPos.x,
                        y: leftTopPixelPos.y - newCropHeight
                    );

                    Model.ChartPackData.CurrentValue.CropStartPosition.Value = newCropStartPos;
                    Model.ChartPackData.CurrentValue.CropHeight.Value = newCropHeight;
                    break;
                }
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
