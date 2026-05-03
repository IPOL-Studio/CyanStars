#nullable enable

using System;
using System.Threading.Tasks;
using CatAsset.Runtime;
using CyanStars.Framework;
using CyanStars.Framework.File;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Management;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Utils;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ChartPackDataCoverViewModel : BaseViewModel
    {
        private const string CoverFileName = "Cover.png";


        private Vector2? recordedCropStartPosPercent;
        private float? recordedCropHeightPercent;


        private readonly ReactiveProperty<AssetHandler<Sprite?>?> CoverSpriteHandler;
        public readonly ReadOnlyReactiveProperty<Sprite?> CoverSprite;


        public readonly ReadOnlyReactiveProperty<float> ImageFrameAspectRatio; // 小方框内底图宽高比

        public readonly ReadOnlyReactiveProperty<Vector2> CropLeftBottomPercentPos;
        public readonly ReadOnlyReactiveProperty<Vector2> CropRightTopPercentPos;


        public ChartPackDataCoverViewModel(ChartEditorModel model)
            : base(model)
        {
            // API 触发 --> FilePath 更新 & CoverSprite 卸载和异步加载 --> CoverFrame 可见性更新 --> ImageFrame 宽高比更新 --> 计算裁剪区域位置和大小 --> CoverRectTransform 位置更新 --> RectMask 边距更新

            CoverSpriteHandler = new ReactiveProperty<AssetHandler<Sprite?>?>();

            // 绑定曲绘路径、裁剪等 Model 属性
            // 由于可以撤销重做，故不足以根据路径变化事件来判断是否需要重置裁剪位置和高度。考虑到异步加载的问题，在导入新图时由 API 一并刷新图像、裁剪位置和高度。
            // 此处在初始化时仅一次性加载图像，不做绑定。
            ReadOnlyReactiveProperty<string?> filePath = Model.ChartPackData
                .Select(data => data.CoverFilePath.AsObservable())
                .Switch()
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            if (filePath.CurrentValue != null)
                _ = LoadCoverSpriteAsync();

            // 绑定图像、显示比例等 UI 显示相关属性
            CoverSprite = CoverSpriteHandler
                .Select(handler => handler?.Asset)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            ImageFrameAspectRatio = CoverSprite
                .Select(sprite =>
                    sprite != null && sprite.texture.height != 0
                        ? (float)sprite.texture.width / sprite.texture.height
                        : 1.0f)
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);

            // 绑定裁剪框 UI 相关属性
            CropLeftBottomPercentPos = Observable
                .CombineLatest(
                    CoverSprite,
                    Model.ChartPackData.CurrentValue.CropStartPositionPercent,
                    (sprite, startPercent) =>
                    {
                        if (sprite == null || startPercent == null)
                            return Vector2.zero;

                        return new Vector2(
                            Mathf.Clamp01(startPercent.Value.x),
                            Mathf.Clamp01(startPercent.Value.y)
                        );
                    }
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(Disposables);
            CropRightTopPercentPos = Observable
                .CombineLatest(
                    CoverSprite,
                    Model.ChartPackData.CurrentValue.CropStartPositionPercent,
                    Model.ChartPackData.CurrentValue.CropHeightPercent,
                    (sprite, startPercent, heightPercent) =>
                    {
                        if (sprite == null || startPercent == null || heightPercent == null)
                            return Vector2.zero;

                        return new Vector2(
                            Mathf.Clamp01(startPercent.Value.x + heightPercent.Value * sprite.texture.height * 4 / sprite.texture.width),
                            Mathf.Clamp01(startPercent.Value.y + heightPercent.Value)
                        );
                    }
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(Disposables);
        }

        private async Task LoadCoverSpriteAsync()
        {
            // 卸载旧的封面图
            if (CoverSpriteHandler.CurrentValue != null)
            {
                CoverSpriteHandler.CurrentValue.Unload();
                CoverSpriteHandler.Value = null;
            }

            // 如果能根据 targetPath 找到暂存文件句柄，则优先加载句柄指向的缓存文件，否则加载谱包资源文件夹内的文件。
            if (!string.IsNullOrEmpty(Model.ChartPackData.CurrentValue.CoverFilePath.CurrentValue))
            {
                string coverFilePath = PathUtil.Combine(Model.WorkspacePath, Model.ChartPackData.CurrentValue.CoverFilePath.CurrentValue);
                var handler = ChartEditorFileManager.GetHandlerByTargetPath(coverFilePath);
                if (handler != null)
                {
                    coverFilePath = handler.TempFilePath;
                }

                CoverSpriteHandler.Value =
                    await GameRoot.Asset.LoadAssetAsync<Sprite?>(coverFilePath);
            }
        }

        private void GetDefaultCoverCropData(Sprite sprite, out Vector2 startPosPercent, out float heightPercent)
        {
            float aspectRatio = (float)sprite.texture.width / sprite.texture.height;
            if (aspectRatio >= 4.0f)
            {
                // 宽图，左右裁剪
                heightPercent = 1;
                startPosPercent = new Vector2((sprite.texture.width - sprite.texture.height * 4) / 2.0f / sprite.texture.width, 0.0f);
            }
            else
            {
                // 高图，上下裁剪
                heightPercent = sprite.texture.width / 4.0f / sprite.texture.height;
                startPosPercent = new Vector2(0.0f, (sprite.texture.height - sprite.texture.width / 4f) / 2.0f / sprite.texture.height);
            }
        }


        public void OpenCoverBrowser()
        {
            GameRoot.File.OpenLoadFilePathBrowser(SetCoverFilePath, title: "打开曲绘", filters: new[] { GameRoot.File.SpriteFilter });
        }

        private void SetCoverFilePath(string newOriginFilePath)
        {
            // 1. 如有旧缓存文件，记录其裁剪信息，然后将旧缓存文件的目标路径清空
            // 2. 导入并暂存新曲绘，指向目标地址，曲绘文件名固定为 "Covet.png"，如有旧文件，在保存时覆盖之

            var newTargetRelativePath = PathUtil.Combine(ChartEditorFileManager.ChartPackAssetsFolderName, CoverFileName); // Assets/Cover.png

            var oldTargetRelativePath = Model.ChartPackData.CurrentValue.CoverFilePath.CurrentValue;
            if (string.IsNullOrEmpty(oldTargetRelativePath))
                oldTargetRelativePath = "";

            var oldTargetAbsolutePath = oldTargetRelativePath != ""
                ? PathUtil.Combine(Model.WorkspacePath, oldTargetRelativePath)
                : "";

            var newTargetAbsolutePath = PathUtil.Combine(Model.WorkspacePath, newTargetRelativePath);


            // 记录旧曲绘信息
            IReadonlyTempFileHandler? oldHandler = ChartEditorFileManager.GetHandlerByTargetPath(oldTargetAbsolutePath);
            Vector2? oldCropStartPosPercent = Model.ChartPackData.CurrentValue.CropStartPositionPercent.Value;
            float? oldCropHeightPercent = Model.ChartPackData.CurrentValue.CropHeightPercent.Value;


            // 仅复制文件到缓存区，暂不声明目标路径以防止自动顶替旧句柄目标路径。
            IReadonlyTempFileHandler newHandler = ChartEditorFileManager.TempFile(newOriginFilePath, null);

            CommandStack.ExecuteCommand(
                async () =>
                {
                    // 更新句柄以在保存时正确复制文件
                    if (oldHandler != null)
                    {
                        ChartEditorFileManager.UpdateTargetFilePath(oldHandler as TempFileHandler, null);
                    }

                    ChartEditorFileManager.UpdateTargetFilePath(newHandler as TempFileHandler, newTargetAbsolutePath);

                    // 加载图片、更新谱包引用地址、更新裁剪信息
                    Model.ChartPackData.CurrentValue.CoverFilePath.Value = newTargetRelativePath;
                    await LoadCoverSpriteAsync();
                    if (CoverSpriteHandler.Value?.Asset == null)
                    {
                        // 加载图片失败？
                        Model.ChartPackData.CurrentValue.CropStartPositionPercent.Value = null;
                        Model.ChartPackData.CurrentValue.CropHeightPercent.Value = null;
                    }
                    else
                    {
                        GetDefaultCoverCropData(CoverSpriteHandler.Value.Asset, out Vector2 newCropStartPos, out float newCropHeight);
                        Model.ChartPackData.CurrentValue.CropStartPositionPercent.Value = newCropStartPos;
                        Model.ChartPackData.CurrentValue.CropHeightPercent.Value = newCropHeight;
                    }
                },
                async () =>
                {
                    // 更新句柄以在保存时正确复制文件
                    ChartEditorFileManager.UpdateTargetFilePath(newHandler as TempFileHandler, null);

                    if (oldHandler != null)
                    {
                        ChartEditorFileManager.UpdateTargetFilePath(oldHandler as TempFileHandler, oldTargetAbsolutePath);
                    }

                    // 加载图片、更新谱包引用地址、更新裁剪信息
                    Model.ChartPackData.CurrentValue.CoverFilePath.Value = oldTargetRelativePath;
                    await LoadCoverSpriteAsync();
                    Model.ChartPackData.CurrentValue.CropStartPositionPercent.Value = oldCropStartPosPercent;
                    Model.ChartPackData.CurrentValue.CropHeightPercent.Value = oldCropHeightPercent;
                }
            );
        }

        public void RecordCropData()
        {
            recordedCropStartPosPercent = Model.ChartPackData.CurrentValue.CropStartPositionPercent.Value;
            recordedCropHeightPercent = Model.ChartPackData.CurrentValue.CropHeightPercent.Value;
        }

        public void CommitCropData()
        {
            Vector2? newCropStartPos = Model.ChartPackData.CurrentValue.CropStartPositionPercent.Value;
            float? newCropHeight = Model.ChartPackData.CurrentValue.CropHeightPercent.Value;

            if (newCropStartPos == recordedCropStartPosPercent && newCropHeight == recordedCropHeightPercent)
                return;

            CommandStack.ExecuteCommand(
                () =>
                {
                    Model.ChartPackData.CurrentValue.CropStartPositionPercent.Value = newCropStartPos;
                    Model.ChartPackData.CurrentValue.CropHeightPercent.Value = newCropHeight;
                },
                () =>
                {
                    Model.ChartPackData.CurrentValue.CropStartPositionPercent.Value = recordedCropStartPosPercent;
                    Model.ChartPackData.CurrentValue.CropHeightPercent.Value = recordedCropHeightPercent;
                }
            );
        }

        public void OnHandlerDragging(CoverCropHandlerType handlerType, Vector2 percentPos)
        {
            if (CoverSprite.CurrentValue == null)
                throw new InvalidOperationException("不允许在未加载曲绘时设置裁剪框位置");

            // 限制坐标在 0~1 之间
            percentPos.x = Mathf.Clamp01(percentPos.x);
            percentPos.y = Mathf.Clamp01(percentPos.y);

            Vector2 coverPixelSize = new Vector2(
                CoverSprite.CurrentValue.rect.width,
                CoverSprite.CurrentValue.rect.height
            );

            // 计算鼠标在原图上的像素坐标
            Vector2 mousePixelPos = new Vector2(percentPos.x * coverPixelSize.x, percentPos.y * coverPixelSize.y);

            // 将百分比坐标转为像素坐标
            var cropData = Model.ChartPackData.CurrentValue;
            Vector2 currentStartPercent = cropData.CropStartPositionPercent.CurrentValue ?? Vector2.zero;
            float currentHeightPercent = cropData.CropHeightPercent.CurrentValue ?? 0.0f;

            // 获取当前的状态作为基础（主要用于确定不动点）
            float currentStartX = currentStartPercent.x * coverPixelSize.x;
            float currentStartY = currentStartPercent.y * coverPixelSize.y;
            float currentHeight = currentHeightPercent * coverPixelSize.y;
            float currentWidth = currentHeight * 4.0f;

            Vector2 newCropStartPixel;
            float newCropHeightPixel;


            // 自动计算 percentPosX，校验并调整裁剪区域，确保不会超出曲绘范围；然后更新 Model 中的裁剪数据
            // 1. 确定对角点（不动点）
            // 2. 计算鼠标到对角点的距离
            // 3. 根据 4:1 比例，计算需要的 Height
            //    为了让裁剪框跟随鼠标，取 Max(dy, dx / 4)。即：如果鼠标拉得很宽，就由宽度决定高度；如果拉得很高，就由高度决定
            // 4. 计算边界限制，防止超出图片
            switch (handlerType)
            {
                case CoverCropHandlerType.LeftTop:
                {
                    Vector2 pivot = new Vector2(currentStartX + currentWidth, currentStartY);

                    float dx = Mathf.Max(0, pivot.x - mousePixelPos.x);
                    float dy = Mathf.Max(0, mousePixelPos.y - pivot.y);

                    float maxH = Mathf.Min(pivot.x / 4.0f, coverPixelSize.y - pivot.y);

                    newCropHeightPixel = Mathf.Clamp(Mathf.Max(dx / 4.0f, dy), 0, maxH);
                    newCropStartPixel = new Vector2(pivot.x - newCropHeightPixel * 4.0f, pivot.y);
                    break;
                }
                case CoverCropHandlerType.LeftBottom:
                {
                    Vector2 pivot = new Vector2(currentStartX + currentWidth, currentStartY + currentHeight);

                    float dx = Mathf.Max(0, pivot.x - mousePixelPos.x);
                    float dy = Mathf.Max(0, pivot.y - mousePixelPos.y);

                    float maxH = Mathf.Min(pivot.x / 4.0f, pivot.y);

                    newCropHeightPixel = Mathf.Clamp(Mathf.Max(dx / 4.0f, dy), 0, maxH);
                    newCropStartPixel = new Vector2(pivot.x - newCropHeightPixel * 4.0f, pivot.y - newCropHeightPixel);
                    break;
                }
                case CoverCropHandlerType.RightTop:
                {
                    Vector2 pivot = new Vector2(currentStartX, currentStartY);

                    float dx = Mathf.Max(0, mousePixelPos.x - pivot.x);
                    float dy = Mathf.Max(0, mousePixelPos.y - pivot.y);

                    float maxH = Mathf.Min((coverPixelSize.x - pivot.x) / 4.0f, coverPixelSize.y - pivot.y);

                    newCropHeightPixel = Mathf.Clamp(Mathf.Max(dx / 4.0f, dy), 0, maxH);
                    newCropStartPixel = pivot;
                    break;
                }
                case CoverCropHandlerType.RightBottom:
                {
                    Vector2 pivot = new Vector2(currentStartX, currentStartY + currentHeight);

                    float dx = Mathf.Max(0, mousePixelPos.x - pivot.x);
                    float dy = Mathf.Max(0, pivot.y - mousePixelPos.y);

                    float maxH = Mathf.Min((coverPixelSize.x - pivot.x) / 4.0f, pivot.y);

                    newCropHeightPixel = Mathf.Clamp(Mathf.Max(dx / 4.0f, dy), 0, maxH);
                    newCropStartPixel = new Vector2(pivot.x, pivot.y - newCropHeightPixel);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(handlerType), handlerType, null);
            }

            // 将计算出的像素结果重新转回百分比形式
            Vector2 newCropStartPercent = new Vector2(newCropStartPixel.x / coverPixelSize.x, newCropStartPixel.y / coverPixelSize.y);
            float newCropHeightPercent = newCropHeightPixel / coverPixelSize.y;

            // 实时更新 Model 数据以实现实时预览，不生成命令
            cropData.CropStartPositionPercent.Value = newCropStartPercent;
            cropData.CropHeightPercent.Value = newCropHeightPercent;
        }

        /// <summary>
        /// 移动整个裁剪框
        /// </summary>
        /// <param name="deltaRatio">X/Y 轴的移动量（相对于图片尺寸的百分比）</param>
        public void OnFrameDragging(Vector2 deltaRatio)
        {
            if (CoverSprite.CurrentValue == null)
                return;

            var sprite = CoverSprite.CurrentValue;
            float imgW = sprite.rect.width;
            float imgH = sprite.rect.height;

            // 获取当前 Model 数据
            var cropData = Model.ChartPackData.CurrentValue;
            Vector2 currentStartPercent = cropData.CropStartPositionPercent.CurrentValue ?? Vector2.zero;
            float currentHeightPercent = cropData.CropHeightPercent.CurrentValue ?? 0f;

            // 将比例转为像素
            Vector2 currentStartPixel = new Vector2(currentStartPercent.x * imgW, currentStartPercent.y * imgH);
            float currentHeightPixel = currentHeightPercent * imgH;
            float currentWidthPixel = currentHeightPixel * 4.0f;

            // 计算像素偏移
            Vector2 deltaPixel = new Vector2(deltaRatio.x * imgW, deltaRatio.y * imgH);
            Vector2 targetPixelPos = currentStartPixel + deltaPixel;

            // 限制范围，确保裁剪框不超出图片边界
            float maxX = imgW - currentWidthPixel;
            float maxY = imgH - currentHeightPixel;
            targetPixelPos.x = Mathf.Clamp(targetPixelPos.x, 0f, maxX);
            targetPixelPos.y = Mathf.Clamp(targetPixelPos.y, 0f, maxY);

            if (targetPixelPos != currentStartPixel)
            {
                Vector2 targetPercentPos = new Vector2(targetPixelPos.x / imgW, targetPixelPos.y / imgH);
                cropData.CropStartPositionPercent.Value = targetPercentPos;
            }
        }

        public override void Dispose()
        {
            CoverSpriteHandler.CurrentValue?.Unload();
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
