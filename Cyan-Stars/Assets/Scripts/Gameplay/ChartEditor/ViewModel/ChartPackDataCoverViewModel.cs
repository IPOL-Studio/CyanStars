#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CatAsset.Runtime;
using CyanStars.Chart.Loading;
using CyanStars.Framework;
using CyanStars.Framework.File;
using CyanStars.Gameplay.ChartEditor.Command;
using CyanStars.Gameplay.ChartEditor.Model;
using CyanStars.Gameplay.ChartEditor.View;
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

        private CancellationTokenSource? coverLoadCts;


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
            // 由于可以撤销重做，故不足以根据路径变化事件来判断是否需要重置裁剪位置和高度。
            // 考虑到异步加载的问题，在导入新图时由 API 一并刷新图像、裁剪位置和高度。
            // 此处在初始化时仅一次性加载图像，不做绑定。
            ReadOnlyReactiveProperty<string?> filePath = Model.ChartPackData
                .Select(data => data.CoverFilePath.AsObservable())
                .Switch()
                .ToReadOnlyReactiveProperty()
                .AddTo(base.Disposables);
            if (!string.IsNullOrEmpty(filePath.CurrentValue))
                _ = LoadCoverSpriteAsync(filePath.CurrentValue);

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

        /// <summary>
        /// 按谱包里记录的相对路径加载曲绘
        /// </summary>
        /// <param name="coverRelativePath">谱包数据里的相对路径</param>
        private async Task<Sprite?> LoadCoverSpriteAsync(string coverRelativePath)
        {
            return await ReplaceCoverSpriteAsync(PathUtil.Combine(Model.WorkspacePath, coverRelativePath));
        }

        /// <summary>
        /// 按目标路径加载并换上曲绘
        /// </summary>
        /// <param name="targetAbsolutePath">曲绘在工作区里的目标绝对路径；传 null 只卸载不加载</param>
        /// <returns>本次加载出来的图片；路径为空、加载失败时为 null</returns>
        private async Task<Sprite?> ReplaceCoverSpriteAsync(string? targetAbsolutePath)
        {
            UnloadCoverSprite();

            if (string.IsNullOrEmpty(targetAbsolutePath))
                return null;

            // TODO: 当前依赖 CatAsset 取消后不恢复续体的行为；若该行为变化，需在 await 后补充取消检查
            var cts = new CancellationTokenSource();
            coverLoadCts = cts;

            try
            {
                string readablePath = Model.ResolveAssetReadablePath(targetAbsolutePath);
                AssetHandler<Sprite?> handler = await GameRoot.Asset.LoadAssetAsync<Sprite?>(readablePath, cts.Token);

                // 本次加载已经完成且没有被取消
                CoverSpriteHandler.Value = handler;
                return handler.Asset;
            }
            finally
            {
                // 加载被取消时续体不会恢复，coverLoadCts 已由取消方复位
                if (ReferenceEquals(coverLoadCts, cts))
                    coverLoadCts = null;

                cts.Dispose();
            }
        }

        /// <summary>
        /// 卸载当前正在显示的曲绘，并取消在途加载
        /// </summary>
        private void UnloadCoverSprite()
        {
            CancellationTokenSource? cts = coverLoadCts;
            coverLoadCts = null;
            cts?.Cancel();

            CoverSpriteHandler.CurrentValue?.Unload();
            CoverSpriteHandler.Value = null;
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
            GameRoot.File.OpenLoadFileHandleBrowser(
                SetCoverFile,
                title: "打开曲绘",
                filters: new[]
                {
                    GameRoot.File.SpriteFilter
                },
                cacheKind: FileCacheKind.ChartEditor
            );
        }

        /// <summary>
        /// 把玩家选中的曲绘导入为谱包的曲绘
        /// </summary>
        /// <param name="coverFileHandle">选中文件的句柄，句柄的生命周期转交给本方法</param>
        /// <remarks>
        /// <para>曲绘文件名固定为 Assets/Cover.png：导入时把新句柄指到它，保存时覆盖旧文件</para>
        /// <para>旧曲绘若是本次会话导入的，会先解绑再让新句柄占用目标路径，撤销时再换回来</para>
        /// </remarks>
        private void SetCoverFile(FileHandle coverFileHandle)
        {
            var newTargetRelativePath = PathUtil.Combine(ChartPackDataLoader.ChartPackAssetsFolder, CoverFileName); // Assets/Cover.png

            var oldTargetRelativePath = Model.ChartPackData.CurrentValue.CoverFilePath.CurrentValue;
            if (oldTargetRelativePath == null)
                oldTargetRelativePath = "";

            var oldTargetAbsolutePath = Model.GetAssetAbsolutePath(oldTargetRelativePath);
            var newTargetAbsolutePath = Model.GetAssetAbsolutePath(newTargetRelativePath);

            // 提前校验保存目标，失败则不修改任何数据
            if (!PlatformFilePortal.TrySetSaveTarget(coverFileHandle, newTargetAbsolutePath))
            {
                PlatformFilePortal.TryReleaseFile(coverFileHandle);

                PopupView.Show("无法导入曲绘",
                    "无法把选中的图片保存到谱包工作区，请检查日志。",
                    true,
                    new Dictionary<string, Action?>
                    {
                        ["确定"] = null
                    }
                );
                return;
            }

            // 旧曲绘的句柄先解绑，稍后由新句柄占用目标路径，撤销时再换回来
            FileHandle? oldAssetHandle = Model.DetachAssetHandle(oldTargetAbsolutePath);
            Vector2? oldCropStartPosPercent = Model.ChartPackData.CurrentValue.CropStartPositionPercent.Value;
            float? oldCropHeightPercent = Model.ChartPackData.CurrentValue.CropHeightPercent.Value;

            // TODO: 下列异步 lambda 在 await 之后抛出的异常无人接管，后续考虑改造成可等待的命令
            CommandStack.ExecuteCommand(
                async () =>
                {
                    Model.AddAssetHandle(newTargetAbsolutePath, coverFileHandle);

                    Model.ChartPackData.CurrentValue.CoverFilePath.Value = newTargetRelativePath;
                    Sprite? sprite = await ReplaceCoverSpriteAsync(newTargetAbsolutePath);

                    if (sprite == null)
                    {
                        Model.ChartPackData.CurrentValue.CropStartPositionPercent.Value = null;
                        Model.ChartPackData.CurrentValue.CropHeightPercent.Value = null;
                    }
                    else
                    {
                        GetDefaultCoverCropData(sprite, out Vector2 newCropStartPos, out float newCropHeight);
                        Model.ChartPackData.CurrentValue.CropStartPositionPercent.Value = newCropStartPos;
                        Model.ChartPackData.CurrentValue.CropHeightPercent.Value = newCropHeight;
                    }
                },
                async () =>
                {
                    Model.DetachAssetHandle(newTargetAbsolutePath);

                    if (oldAssetHandle != null)
                        Model.ReattachAssetHandle(oldTargetAbsolutePath, oldAssetHandle);

                    Model.ChartPackData.CurrentValue.CoverFilePath.Value = oldTargetRelativePath;
                    await ReplaceCoverSpriteAsync(string.IsNullOrEmpty(oldTargetAbsolutePath) ? null : oldTargetAbsolutePath);

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
            bool changed = cropData.CropStartPositionPercent.Value != newCropStartPercent ||
                cropData.CropHeightPercent.Value != newCropHeightPercent;
            cropData.CropStartPositionPercent.Value = newCropStartPercent;
            cropData.CropHeightPercent.Value = newCropHeightPercent;

            // 拖拽预览绕过命令栈直接写 Model，手动标记脏状态
            if (changed)
                CommandStack.MarkDirty();
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

                // 拖拽预览绕过命令栈直接写 Model，手动标记脏状态
                CommandStack.MarkDirty();
            }
        }

        public override void Dispose()
        {
            // 取消在途加载并卸载当前曲绘：被取消的加载不会恢复执行，因而不会再碰已经销毁的 VM
            UnloadCoverSprite();
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
