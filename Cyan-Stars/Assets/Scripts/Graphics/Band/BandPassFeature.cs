using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace CyanStars.Graphics.Band
{
    public class BandPassFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent RenderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            public Material Material;
        }

        private class PassData
        {
            internal Material material;
            internal TextureHandle inputTexture;
        }

        // 我不知道为什么和 FUllScreenPassRendererFeature 的核心写得差不多
        // 但是在这里就是工作不正常
        // 姑且留着这个玩意，但是相关实现先迁到 FSPRF 上了
        private class BandRenderPass : ScriptableRenderPass
        {
            private string profilerTag;
            private Settings settings;

            public BandRenderPass(string profilerTag, Settings settings)
            {
                this.profilerTag = profilerTag;
                this.settings = settings;
                profilingSampler = new ProfilingSampler(profilerTag);
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                Debug.Assert(resourceData.cameraColor.IsValid());

                var targetDesc = renderGraph.GetTextureDesc(resourceData.cameraColor);
                targetDesc.name = "_CameraColorFullScreenPass";
                targetDesc.clearBuffer = false;

                var src = resourceData.activeColorTexture;
                var dest = renderGraph.CreateTexture(targetDesc);

                renderGraph.AddBlitPass(src, dest, Vector2.one, Vector2.zero, passName: "Copy Color Full Screen");

                src = dest;
                dest = resourceData.activeColorTexture;

                renderGraph.AddBlitPass(new(src, dest, settings.Material, 0), passName: "Blit Band Effect");
            }
        }

        private BandRenderPass bandPass;

        [SerializeField]
        private Settings settings;

        public override void Create()
        {
            bandPass = new BandRenderPass(name, settings) { renderPassEvent = settings.RenderPassEvent };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.SceneView)
                renderer.EnqueuePass(bandPass);
        }
    }
}
