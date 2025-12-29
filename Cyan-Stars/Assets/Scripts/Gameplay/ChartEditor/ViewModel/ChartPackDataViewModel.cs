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

        // 裁剪框整体的 Anchor Min/Max，用于控制 Frame 本身的位置和大小
        public readonly ReadOnlyReactiveProperty<Vector2> CoverCropAnchorMin;
        public readonly ReadOnlyReactiveProperty<Vector2> CoverCropAnchorMax;


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

            CoverCropAnchorMin = Observable.CombineLatest(
                    LoadedCoverSprite, coverCropStartPercentPos,
                    (sprite, startPos) =>
                    {
                        if (sprite is null || sprite.rect.width <= 0 || sprite.rect.height <= 0) return Vector2.zero;
                        return new Vector2(startPos.x / sprite.rect.width, startPos.y / sprite.rect.height);
                    })
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            CoverCropAnchorMax = Observable.CombineLatest(
                    LoadedCoverSprite, coverCropStartPercentPos, coverCropWidth, coverCropHeight,
                    (sprite, startPos, width, height) =>
                    {
                        if (sprite is null || sprite.rect.width <= 0 || sprite.rect.height <= 0) return Vector2.zero;
                        return new Vector2((startPos.x + width) / sprite.rect.width, (startPos.y + height) / sprite.rect.height);
                    })
                .ToReadOnlyReactiveProperty()
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


        public void OpenCoverBrowser()
        {
            GameRoot.File.OpenLoadFilePathBrowser((newFilePath) =>
                {
                    var data = Model.ChartPackData.CurrentValue;

                    string? oldFilePath = data.CoverFilePath.Value;
                    Vector2? oldStartPos = data.CropStartPosition.Value;
                    float? oldHeight = data.CropHeight.Value;

                    if (oldFilePath == newFilePath)
                        return;

                    CommandManager.ExecuteCommand(
                        new DelegateCommand(
                            () =>
                            {
                                // 重置裁剪数据，以便 LoadCoverSprite 检测到后自动执行 AutoSetCoverCrop
                                data.CropStartPosition.Value = null;
                                data.CropHeight.Value = null;
                                data.CoverFilePath.Value = newFilePath;
                            },
                            () =>
                            {
                                data.CoverFilePath.Value = oldFilePath;
                                // 撤销时恢复旧的裁剪数据
                                data.CropStartPosition.Value = oldStartPos;
                                data.CropHeight.Value = oldHeight;
                            }
                        )
                    );
                }
            );
        }

        private async Task LoadCoverSprite(string filePath)
        {
            coverAssetHandler?.Unload();
            loadedCoverSprite.Value = null;

            if (string.IsNullOrEmpty(filePath))
                return;

            coverAssetHandler = await GameRoot.Asset.LoadAssetAsync<Sprite?>(filePath);
            loadedCoverSprite.Value = coverAssetHandler.Asset;

            // 图片加载完成后，如果裁剪高度未设置（例如刚导入新图片），则自动计算居中裁剪
            if (loadedCoverSprite.Value != null)
            {
                var currentHeight = Model.ChartPackData.CurrentValue.CropHeight.Value;
                if (currentHeight == null || currentHeight <= 0f)
                {
                    AutoSetCoverCrop(loadedCoverSprite.Value);
                }
            }
        }

        /// <summary>
        /// 自动设置裁剪区域：保持 4:1 比例，在不超过原图范围的前提下最大化并居中
        /// </summary>
        private void AutoSetCoverCrop(Sprite sprite)
        {
            float imgW = sprite.rect.width;
            float imgH = sprite.rect.height;

            if (imgW <= 0 || imgH <= 0) return;

            // 目标比例 4:1 => W = 4 * H
            // 尝试以宽度为基准计算高度：H = W / 4
            // 尝试以高度为基准计算高度：H = H_img

            // 最终高度取两者中能被图片尺寸容纳的最大值
            // 即 min(图片高度, 图片宽度 / 4)
            float cropH = Mathf.Min(imgH, imgW / 4.0f);
            float cropW = cropH * 4.0f;

            // 居中计算起始坐标
            float startX = (imgW - cropW) / 2.0f;
            float startY = (imgH - cropH) / 2.0f;

            Model.ChartPackData.CurrentValue.CropStartPosition.Value = new Vector2(startX, startY);
            Model.ChartPackData.CurrentValue.CropHeight.Value = cropH;
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

                    // 更新 Model 中的裁剪数据
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

                    // 更新 Model 中的裁剪数据
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

                    // 更新 Model 中的裁剪数据
                    Model.ChartPackData.CurrentValue.CropStartPosition.Value = newCropStartPos;
                    Model.ChartPackData.CurrentValue.CropHeight.Value = newCropHeight;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(handlerType), handlerType, null);
            }
        }

        /// <summary>
        /// 移动整个裁剪框
        /// </summary>
        /// <param name="deltaRatio">X/Y 轴的移动量（相对于图片尺寸的百分比）</param>
        public void MoveCoverCrop(Vector2 deltaRatio)
        {
            if (LoadedCoverSprite.CurrentValue == null) return;

            var sprite = LoadedCoverSprite.CurrentValue;
            float imgW = sprite.rect.width;
            float imgH = sprite.rect.height;

            // 获取当前 Model 数据
            Vector2 currentStartPos = Model.ChartPackData.CurrentValue.CropStartPosition.CurrentValue ?? Vector2.zero;
            float currentWidth = Model.ChartPackData.CurrentValue.CropWidth.CurrentValue ?? 0f;
            float currentHeight = Model.ChartPackData.CurrentValue.CropHeight.CurrentValue ?? 0f;

            // 计算像素偏移
            Vector2 deltaPixel = new Vector2(deltaRatio.x * imgW, deltaRatio.y * imgH);
            Vector2 targetPos = currentStartPos + deltaPixel;

            // 限制范围，确保裁剪框不超出图片边界
            float minX = 0f;
            float maxX = imgW - currentWidth;
            float minY = 0f;
            float maxY = imgH - currentHeight;

            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

            if (targetPos != currentStartPos)
            {
                Model.ChartPackData.CurrentValue.CropStartPosition.Value = targetPos;
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
