#nullable enable

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace CyanStars.Graphics.UI
{
    /// <summary>
    /// 弹窗面板范围内的背景模糊
    /// </summary>
    /// <remarks>
    /// 在相机渲染完透明物体之后，对相机颜色做降采样高斯模糊，再用弹窗面板贴图的 alpha 作为遮罩，
    /// 只把模糊结果混回面板所在的区域，面板以外的画面保持清晰。<br />
    /// 由于 Screen Space - Overlay 的弹窗由 UI 系统在所有相机渲染完毕后才绘制，弹窗本身不会被模糊。<br />
    /// 模糊强度由全局变量 <see cref="BlurRadiusId" /> 控制，为 0 时不会产生任何额外的渲染开销。
    /// </remarks>
    public class UIPopupBlurFeature : ScriptableRendererFeature
    {
        /// <summary>
        /// 模糊半径的全局 shader 属性，单位为「半分辨率像素」
        /// </summary>
        public static readonly int BlurRadiusId = Shader.PropertyToID("_UIPopupBlurRadius");

        /// <summary>
        /// 面板屏幕矩形的全局 shader 属性 (xMin, yMin, xMax, yMax)，归一化屏幕坐标
        /// </summary>
        public static readonly int PanelRectId = Shader.PropertyToID("_UIPopupBlurPanelRect");

        /// <summary>
        /// 面板 sprite 在纹理中的 uv 范围的全局 shader 属性 (uMin, vMin, uSize, vSize)
        /// </summary>
        public static readonly int PanelUVId = Shader.PropertyToID("_UIPopupBlurPanelUV");

        /// <summary>
        /// 面板贴图的全局 shader 属性，其 alpha 通道用作遮罩
        /// </summary>
        public static readonly int PanelMaskId = Shader.PropertyToID("_UIPopupBlurPanelMask");

        /// <summary>
        /// 面板 alpha 到遮罩的映射区间
        /// </summary>
        public static readonly int MaskRangeId = Shader.PropertyToID("_UIPopupBlurMaskRange");

        [System.Serializable]
        public class Settings
        {
            /// <summary>
            /// 插入点，需要保证在相机渲染完所有 UI 之后
            /// </summary>
            public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRenderingTransparents;

            /// <summary>
            /// 模糊使用的材质，需要基于 CyanStars/UI/PopupBlur 创建
            /// </summary>
            public Material Material = null!;

            /// <summary>
            /// 横纵各做几次高斯模糊。次数越多半径越大、越平滑，开销也越高
            /// </summary>
            [Range(1, 6)]
            public int Iterations = 3;
        }

        private class CombinePassData
        {
            internal Material Material = null!;
            internal TextureHandle Blurred;
            internal TextureHandle Original;
        }

        private class UIPopupBlurPass : ScriptableRenderPass
        {
            private const int PassDownsample = 0;
            private const int PassHorizontal = 1;
            private const int PassVertical = 2;
            private const int PassCombine = 3;

            private static readonly int BlitTextureId = Shader.PropertyToID("_BlitTexture");
            private static readonly int BlitScaleBiasId = Shader.PropertyToID("_BlitScaleBias");
            private static readonly int OriginalTextureId = Shader.PropertyToID("_UIPopupBlurOriginal");
            private static readonly MaterialPropertyBlock CombinePropertyBlock = new();

            private readonly Settings Settings;

            public UIPopupBlurPass(Settings settings)
            {
                this.Settings = settings;
                profilingSampler = new ProfilingSampler(nameof(UIPopupBlurPass));

                // 需要采样相机颜色，因此必须经由中间纹理
                requiresIntermediateTexture = true;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                if (!resourceData.cameraColor.IsValid())
                    return;

                // 相机直接渲染到后备缓冲时无法采样颜色
                if (resourceData.isActiveTargetBackBuffer)
                    return;

                Material? material = Settings.Material;
                if (material == null)
                    return;

                TextureHandle cameraColor = resourceData.activeColorTexture;

                // 全分辨率备份 + 两张半分辨率临时纹理
                TextureDesc desc = renderGraph.GetTextureDesc(cameraColor);
                desc.clearBuffer = false;
                desc.filterMode = FilterMode.Bilinear;
                desc.wrapMode = TextureWrapMode.Clamp;
                desc.msaaSamples = MSAASamples.None;
                desc.bindTextureMS = false;
                desc.useDynamicScale = false;

                desc.name = "_UIPopupBlurOriginal";
                TextureHandle original = renderGraph.CreateTexture(desc);

                desc.width = Mathf.Max(1, desc.width / 2);
                desc.height = Mathf.Max(1, desc.height / 2);
                desc.name = "_UIPopupBlurHalfA";
                desc.name = "_UIPopupBlurHalfB";
                TextureHandle halfResA = renderGraph.CreateTexture(desc);
                TextureHandle halfResB = renderGraph.CreateTexture(desc);

                // 1. 备份原图，最后按遮罩混合时使用
                renderGraph.AddBlitPass(cameraColor, original, Vector2.one, Vector2.zero, passName: "UIPopupBlur Copy Original");

                // 2. 降采样
                renderGraph.AddBlitPass(new(cameraColor, halfResA, material, PassDownsample), passName: "UIPopupBlur Downsample");

                // 3. 多次可分离高斯，在小半径下反复模糊，比一次性大半径更平滑、更不容易出现重影
                int iterations = Mathf.Clamp(Settings.Iterations, 1, 6);
                for (int i = 0; i < iterations; i++)
                {
                    renderGraph.AddBlitPass(new(halfResA, halfResB, material, PassHorizontal), passName: "UIPopupBlur Horizontal");
                    renderGraph.AddBlitPass(new(halfResB, halfResA, material, PassVertical), passName: "UIPopupBlur Vertical");
                }

                // 4. 按面板遮罩把模糊结果混回相机颜色（面板以外保持原样）
                using var builder = renderGraph.AddRasterRenderPass<CombinePassData>("UIPopupBlur Combine", out var passData, profilingSampler);
                passData.Material = material;
                passData.Blurred = halfResA;
                passData.Original = original;

                builder.UseTexture(passData.Blurred, AccessFlags.Read);
                builder.UseTexture(passData.Original, AccessFlags.Read);
                builder.SetRenderAttachment(cameraColor, 0, AccessFlags.Write);

                builder.SetRenderFunc((CombinePassData data, RasterGraphContext context) =>
                {
                    ExecuteCombinePass(context.cmd, data.Blurred, data.Original, data.Material, PassCombine);
                });
            }

            private static void ExecuteCombinePass(RasterCommandBuffer cmd, RTHandle blurred, RTHandle original, Material material, int passIndex)
            {
                CombinePropertyBlock.Clear();
                CombinePropertyBlock.SetTexture(BlitTextureId, blurred);
                CombinePropertyBlock.SetTexture(OriginalTextureId, original);
                CombinePropertyBlock.SetVector(BlitScaleBiasId, new Vector4(1, 1, 0, 0));

                cmd.DrawProcedural(Matrix4x4.identity, material, passIndex, MeshTopology.Triangles, 3, 1, CombinePropertyBlock);
            }
        }

        [SerializeField]
        private Settings settings = new();

        private UIPopupBlurPass? blurPass;

        public override void Create()
        {
            blurPass = new UIPopupBlurPass(settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (blurPass == null)
                return;

            // 只在游戏相机上生效，跳过预览/反射等相机
            if (renderingData.cameraData.cameraType != CameraType.Game)
                return;

            // 半径归零时直接跳过，避免不必要的全屏 Blit
            if (Shader.GetGlobalFloat(BlurRadiusId) <= 0f)
                return;

            blurPass.renderPassEvent = settings.RenderPassEvent;
            renderer.EnqueuePass(blurPass);
        }
    }
}
