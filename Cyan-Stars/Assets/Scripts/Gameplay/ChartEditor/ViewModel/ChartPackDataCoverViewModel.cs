#nullable enable

using System;
using System.Collections.Generic;
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

        // 加载代号，自增；与在途加载的代号不一致时该次加载作废
        private int loadGeneration;
        private bool isDisposed;


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

            // 曲绘路径的绑定：撤销/重做也会改路径，但不足以据此判断是否重置裁剪信息，
            // 故初始化时只加载一次图片，导入新图时由 SetCoverFilePath 一并刷新图像和裁剪信息
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
        private async Task LoadCoverSpriteAsync(string coverRelativePath)
        {
            await ReplaceCoverSpriteAsync(PathUtil.Combine(Model.WorkspacePath, coverRelativePath));
        }

        /// <summary>
        /// 一次曲绘加载的结果
        /// </summary>
        private readonly struct CoverLoadResult
        {
            /// <summary>
            /// 本次加载是否仍然是最新的一次。为 false 表示已被后来的加载顶替，
            /// 调用方不能再改由曲绘派生的状态（例如裁剪信息）
            /// </summary>
            public readonly bool IsCurrent;

            /// <summary>
            /// 加载出来的图片；路径为空、加载失败或已被顶替时为 null
            /// </summary>
            public readonly Sprite? Sprite;


            public CoverLoadResult(bool isCurrent, Sprite? sprite)
            {
                IsCurrent = isCurrent;
                Sprite = sprite;
            }
        }

        /// <summary>
        /// 按目标路径加载并换上曲绘
        /// </summary>
        /// <param name="targetAbsolutePath">曲绘在工作区里的目标绝对路径；传 null 只卸载不加载</param>
        /// <returns>本次加载的结果，调用方需先看 <see cref="CoverLoadResult.IsCurrent"/> 再决定要不要写派生状态</returns>
        /// <remarks>
        /// 参数是目标路径而非可直接读取的路径：目标路径上的文件只有保存时才更新，
        /// 本方法统一经 <see cref="TempFileStore.ResolveLatest"/> 换成「有暂存副本就读副本」。
        /// 这是本类唯一改动 <see cref="CoverSpriteHandler"/> 的地方；加载期间若被新的加载顶替，
        /// 本次拿到的句柄由自己卸载。
        /// </remarks>
        private async Task<CoverLoadResult> ReplaceCoverSpriteAsync(string? targetAbsolutePath)
        {
            UnloadCoverSprite();

            if (string.IsNullOrEmpty(targetAbsolutePath))
                return new CoverLoadResult(true, null);

            int generation = ++loadGeneration;

            string readablePath = Model.AssetStore.ResolveLatest(targetAbsolutePath);
            AssetHandler<Sprite?> handler = (await GameRoot.Asset.LoadAssetAsync<Sprite?>(readablePath))!;

            if (generation != loadGeneration || isDisposed)
            {
                handler.Unload();
                return new CoverLoadResult(false, null);
            }

            CoverSpriteHandler.Value = handler;
            return new CoverLoadResult(true, handler.Asset);
        }

        /// <summary>
        /// 卸载当前正在显示的曲绘，并让所有在途加载作废
        /// </summary>
        /// <remarks>自增 <c>loadGeneration</c> 使正在 await 的加载恢复后发现自己已不是最新，
        /// 从而卸载自己拿到的句柄，不会覆盖本方法刚清空的状态</remarks>
        private void UnloadCoverSprite()
        {
            ++loadGeneration;

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
            GameRoot.File.OpenLoadFilePathBrowser(SetCoverFilePath, title: "打开曲绘", filters: new[] { GameRoot.File.SpriteFilter });
        }

        private void SetCoverFilePath(string newOriginFilePath)
        {
            // 曲绘文件名固定为 Assets/Cover.png；导入新曲绘时把暂存句柄指到它，保存时覆盖旧文件

            var newTargetRelativePath = PathUtil.Combine(ChartPackDataLoader.ChartPackAssetsFolder, CoverFileName); // Assets/Cover.png

            var oldTargetRelativePath = Model.ChartPackData.CurrentValue.CoverFilePath.CurrentValue;
            if (string.IsNullOrEmpty(oldTargetRelativePath))
                oldTargetRelativePath = "";

            var oldTargetAbsolutePath = oldTargetRelativePath != ""
                ? PathUtil.Combine(Model.WorkspacePath, oldTargetRelativePath)
                : "";

            var newTargetAbsolutePath = PathUtil.Combine(Model.WorkspacePath, newTargetRelativePath);


            // 记下旧曲绘，撤销时要还回去
            IReadonlyStagedFileHandle? oldStagedFile = Model.AssetStore.FindByTargetPath(oldTargetAbsolutePath);
            Vector2? oldCropStartPosPercent = Model.ChartPackData.CurrentValue.CropStartPositionPercent.Value;
            float? oldCropHeightPercent = Model.ChartPackData.CurrentValue.CropHeightPercent.Value;


            // 先只复制到缓存区、不指定目标路径，免得新暂存文件立刻把旧的顶掉。
            // 复制可能失败（选中的文件已失效、磁盘满等），异常必须在这里收住：再往外就是对话框回调
            IReadonlyStagedFileHandle newStagedFileHandle;
            try
            {
                newStagedFileHandle = Model.AssetStore.Stage(newOriginFilePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"把曲绘复制进缓存区时失败：{e.Message}");

                PopupView.Show("无法导入曲绘",
                    "选中的图片无法复制到缓存区，具体原因见日志。",
                    true,
                    new Dictionary<string, Action?> { ["确定"] = null }
                );
                return;
            }

            CommandStack.ExecuteCommand(
                async () =>
                {
                    // 换指向：旧句柄摘掉、新句柄指过去。曲绘路径是固定常量（Assets/Cover.png），
                    // 属于同一路径换内容提供者，不做音乐导入那样的引用计数检查
                    if (oldStagedFile != null)
                    {
                        Model.AssetStore.Retarget(oldStagedFile, null);
                    }

                    Model.AssetStore.Retarget(newStagedFileHandle, newTargetAbsolutePath);

                    // 加载图片、更新谱包引用地址、更新裁剪信息
                    Model.ChartPackData.CurrentValue.CoverFilePath.Value = newTargetRelativePath;
                    CoverLoadResult loadResult = await ReplaceCoverSpriteAsync(newTargetAbsolutePath);

                    // 已被后来的导入顶替：曲绘和裁剪信息都归那次操作管，这里不再写
                    if (!loadResult.IsCurrent)
                        return;

                    if (loadResult.Sprite == null)
                    {
                        Model.ChartPackData.CurrentValue.CropStartPositionPercent.Value = null;
                        Model.ChartPackData.CurrentValue.CropHeightPercent.Value = null;
                    }
                    else
                    {
                        GetDefaultCoverCropData(loadResult.Sprite, out Vector2 newCropStartPos, out float newCropHeight);
                        Model.ChartPackData.CurrentValue.CropStartPositionPercent.Value = newCropStartPos;
                        Model.ChartPackData.CurrentValue.CropHeightPercent.Value = newCropHeight;
                    }
                },
                async () =>
                {
                    // 撤销：新的和目标路径解绑，旧的指回旧路径
                    Model.AssetStore.Retarget(newStagedFileHandle, null);

                    if (oldStagedFile != null)
                    {
                        Model.AssetStore.Retarget(oldStagedFile, oldTargetAbsolutePath);
                    }

                    // 加载图片、更新谱包引用地址、更新裁剪信息
                    Model.ChartPackData.CurrentValue.CoverFilePath.Value = oldTargetRelativePath;
                    CoverLoadResult loadResult =
                        await ReplaceCoverSpriteAsync(string.IsNullOrEmpty(oldTargetAbsolutePath) ? null : oldTargetAbsolutePath);

                    // 已被后来的操作顶替：它会自己把曲绘和裁剪信息设成正确的值
                    if (!loadResult.IsCurrent)
                        return;

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
            isDisposed = true;

            // 在途加载的句柄由异步回调自己卸载，这里只卸载当前显示的这一份
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
