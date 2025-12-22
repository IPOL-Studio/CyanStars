using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
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

        private class BandRenderPass : ScriptableRenderPass
        {
            private string profilerTag;
            private Settings settings;
            // private RenderTargetIdentifier cameraColor;
            private RTHandle tempRT;

            public BandRenderPass(string profilerTag, Settings settings)
            {
                this.profilerTag = profilerTag;
                this.settings = settings;
            }

            // public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            // {
            //     if (renderingData.cameraData.cameraType == CameraType.SceneView)
            //         return;
            //
            //     cameraColor = renderingData.cameraData.renderer.cameraColorTarget;
            //     cmd.GetTemporaryRT(tempRT.id, renderingData.cameraData.cameraTargetDescriptor);
            // }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                // var resourceData = frameData.Get<UniversalResourceData>();
                // var cameraData = frameData.Get<UniversalCameraData>();
                //
                // if (cameraData.cameraType == CameraType.SceneView)
                //     return;
                //
                // var cameraColorHandle = resourceData.cameraColor;
                // CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
                // using (new ProfilingScope(cmd, new ProfilingSampler(profilerTag)))
                // {
                //     RenderingUtils.ReAllocateHandleIfNeeded(ref tempRT, cameraColorHandle.GetDescriptor(renderGraph), "BandPass");
                //     cmd.SetGlobalTexture("_ColorTexture", cameraColorHandle);
                //     cmd.Blit(cameraColorHandle, tempRT.nameID, settings.Material);
                //     cmd.Blit(tempRT.nameID, cameraColorHandle);
                // }

                // var desc = cameraData.get;
                // cmd.GetTemporaryRT(tempRT.id, renderingData.cameraData.cameraTargetDescriptor);
            }

            // public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            // {
            //     if (renderingData.cameraData.cameraType == CameraType.SceneView)
            //         return;
            //
            //     CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
            //     using (new ProfilingScope(cmd, new ProfilingSampler(profilerTag)))
            //     {
            //         cmd.SetGlobalTexture("_ColorTexture", cameraColor);
            //         cmd.Blit(cameraColor, tempRT.Identifier(), settings.Material);
            //         cmd.Blit(tempRT.Identifier(), cameraColor);
            //     }
            //
            //     context.ExecuteCommandBuffer(cmd);
            //     CommandBufferPool.Release(cmd);
            // }

            public override void OnCameraCleanup(CommandBuffer cmd)
            {
                tempRT.Release();
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
            renderer.EnqueuePass(bandPass);
        }
    }
}
