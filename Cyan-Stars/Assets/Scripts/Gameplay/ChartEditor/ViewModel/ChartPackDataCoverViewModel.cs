#nullable enable

using System;
using System.Threading.Tasks;
using CatAsset.Runtime;
using CyanStars.Framework;
using CyanStars.Framework.File;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Utils;
using R3;
using UnityEngine;

namespace CyanStars.Gameplay.ChartEditor.ViewModel
{
    public class ChartPackDataCoverViewModel : BaseViewModel
    {
        private const string CoverFileName = "Cover.png";


        private Vector2? recordedCropStartPos;
        private float? recordedCropHeight;


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
                    Model.ChartPackData.CurrentValue.CropStartPosition,
                    (sprite, startPixel) =>
                    {
                        if (sprite == null || startPixel == null)
                            return Vector2.zero;

                        return new Vector2(
                            Mathf.Clamp01(startPixel.Value.x / sprite.texture.width),
                            Mathf.Clamp01(startPixel.Value.y / sprite.texture.height)
                        );
                    }
                )
                .ToReadOnlyReactiveProperty()
                .AddTo(Disposables);
            CropRightTopPercentPos = Observable
                .CombineLatest(
                    CoverSprite,
                    Model.ChartPackData.CurrentValue.CropStartPosition,
                    Model.ChartPackData.CurrentValue.CropHeight,
                    (sprite, startPixel, height) =>
                    {
                        if (sprite == null || startPixel == null || height == null)
                            return Vector2.zero;

                        return new Vector2(
                            Mathf.Clamp01((startPixel.Value.x + height.Value * 4f) / sprite.texture.width),
                            Mathf.Clamp01((startPixel.Value.y + height.Value) / sprite.texture.height)
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

        private void GetDefaultCoverCropData(Sprite sprite, out Vector2 startPos, out float height)
        {
            float aspectRatio = (float)sprite.texture.width / sprite.texture.height;
            if (aspectRatio >= 4.0f)
            {
                // 宽图，左右裁剪
                height = sprite.texture.height;
                float width = height * 4.0f;
                startPos = new Vector2((sprite.texture.width - width) / 2.0f, 0.0f);
            }
            else
            {
                // 高图，上下裁剪
                float width = sprite.texture.width;
                height = width / 4.0f;
                startPos = new Vector2(0.0f, (sprite.texture.height - height) / 2.0f);
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
            Vector2? oldCropStartPos = Model.ChartPackData.CurrentValue.CropStartPosition.Value;
            float? oldCropHeight = Model.ChartPackData.CurrentValue.CropHeight.Value;


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
                        Model.ChartPackData.CurrentValue.CropStartPosition.Value = null;
                        Model.ChartPackData.CurrentValue.CropHeight.Value = null;
                    }
                    else
                    {
                        GetDefaultCoverCropData(CoverSpriteHandler.Value.Asset, out Vector2 newCropStartPos, out float newCropHeight);
                        Model.ChartPackData.CurrentValue.CropStartPosition.Value = newCropStartPos;
                        Model.ChartPackData.CurrentValue.CropHeight.Value = newCropHeight;
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
                    Model.ChartPackData.CurrentValue.CropStartPosition.Value = oldCropStartPos;
                    Model.ChartPackData.CurrentValue.CropHeight.Value = oldCropHeight;
                }
            );
        }

        public void RecordCropData()
        {
            recordedCropStartPos = Model.ChartPackData.CurrentValue.CropStartPosition.Value;
            recordedCropHeight = Model.ChartPackData.CurrentValue.CropHeight.Value;
        }

        public void CommitCropData()
        {
            Vector2? newCropStartPos = Model.ChartPackData.CurrentValue.CropStartPosition.Value;
            float? newCropHeight = Model.ChartPackData.CurrentValue.CropHeight.Value;

            if (newCropStartPos == recordedCropStartPos && newCropHeight == recordedCropHeight)
                return;

            CommandStack.ExecuteCommand(
                () =>
                {
                    Model.ChartPackData.CurrentValue.CropStartPosition.Value = newCropStartPos;
                    Model.ChartPackData.CurrentValue.CropHeight.Value = newCropHeight;
                },
                () =>
                {
                    Model.ChartPackData.CurrentValue.CropStartPosition.Value = recordedCropStartPos;
                    Model.ChartPackData.CurrentValue.CropHeight.Value = recordedCropHeight;
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

            // 获取当前的状态作为基础（主要用于确定不动点）
            float currentStartX = Model.ChartPackData.CurrentValue.CropStartPosition.CurrentValue?.x ?? 0.0f;
            float currentStartY = Model.ChartPackData.CurrentValue.CropStartPosition.CurrentValue?.y ?? 0.0f;
            float currentHeight = Model.ChartPackData.CurrentValue.CropHeight.CurrentValue ?? 0.0f;
            float currentWidth = currentHeight * 4.0f; // 始终保持 4:1

            Vector2 newCropStartPos;
            float newCropHeight = 0f;


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

                    newCropHeight = Mathf.Max(dx / 4.0f, dy);
                    newCropHeight = Mathf.Min(newCropHeight, maxH);

                    newCropStartPos = new Vector2(pivot.x - newCropHeight * 4.0f, pivot.y);
                    break;
                }
                case CoverCropHandlerType.LeftBottom:
                {
                    Vector2 pivot = new Vector2(currentStartX + currentWidth, currentStartY + currentHeight);

                    float dx = Mathf.Max(0, pivot.x - mousePixelPos.x);
                    float dy = Mathf.Max(0, pivot.y - mousePixelPos.y);

                    float maxH = Mathf.Min(pivot.x / 4.0f, pivot.y);

                    newCropHeight = Mathf.Max(dx / 4.0f, dy);
                    newCropHeight = Mathf.Min(newCropHeight, maxH);

                    newCropStartPos = new Vector2(pivot.x - newCropHeight * 4.0f, pivot.y - newCropHeight);
                    break;
                }
                case CoverCropHandlerType.RightTop:
                {
                    Vector2 pivot = new Vector2(currentStartX, currentStartY);

                    float dx = Mathf.Max(0, mousePixelPos.x - pivot.x);
                    float dy = Mathf.Max(0, mousePixelPos.y - pivot.y);

                    float maxH = Mathf.Min((coverPixelSize.x - pivot.x) / 4.0f, coverPixelSize.y - pivot.y);

                    newCropHeight = Mathf.Max(dx / 4.0f, dy);
                    newCropHeight = Mathf.Min(newCropHeight, maxH);

                    newCropStartPos = pivot;
                    break;
                }
                case CoverCropHandlerType.RightBottom:
                {
                    Vector2 pivot = new Vector2(currentStartX, currentStartY + currentHeight);

                    float dx = Mathf.Max(0, mousePixelPos.x - pivot.x);
                    float dy = Mathf.Max(0, pivot.y - mousePixelPos.y);

                    float maxH = Mathf.Min((coverPixelSize.x - pivot.x) / 4.0f, pivot.y);

                    newCropHeight = Mathf.Max(dx / 4.0f, dy);
                    newCropHeight = Mathf.Min(newCropHeight, maxH);

                    newCropStartPos = new Vector2(pivot.x, pivot.y - newCropHeight);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(handlerType), handlerType, null);
            }

            // 实时更新 Model 数据以实现实时预览，不生成命令
            Model.ChartPackData.CurrentValue.CropStartPosition.Value = newCropStartPos;
            Model.ChartPackData.CurrentValue.CropHeight.Value = newCropHeight;
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
