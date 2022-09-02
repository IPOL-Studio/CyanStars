using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//此脚本暂时没用上
namespace CyanStars.Render
{
    public class Cloud : ScriptableRendererFeature
{
    private class BlitPass : ScriptableRenderPass
    {
        public Material material = null;
        public Transform transform;

        private static readonly int mainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int color = Shader.PropertyToID("_Color");
        private static readonly int strength = Shader.PropertyToID("_Strength");
        private static readonly int inverseProjectionMatrix = Shader.PropertyToID("_InverseProjectionMatrix");
        private static readonly int inverseViewMatrix = Shader.PropertyToID("_InverseViewMatrix");
        private static readonly int noiseTex = Shader.PropertyToID("_noiseTex");

        private CloudColorStrength cloudColorStrength;

        private BlitSettings settings;

        private RenderTargetIdentifier source { get; set; }
        private RenderTargetIdentifier destination { get; set; }

        private RenderTargetHandle m_TemporaryColorTexture;

        private string profilerTag;

        public BlitPass(RenderPassEvent renderPassEvent, BlitSettings settings, string tag)
        {
            this.renderPassEvent = renderPassEvent;
            material = settings.material;
            this.settings = settings;
            profilerTag = tag;
        }

        public void Setup(ScriptableRenderer renderer)
        {
            if (settings.requireDepthNormals)
                ConfigureInput(ScriptableRenderPassInput.Normal);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            if (material == null)
            {
                Debug.LogError("material not created");
                return;
            }

            if (!renderingData.cameraData.postProcessEnabled)
            {
                return;
            }

            var stack = VolumeManager.instance.stack;
            cloudColorStrength = stack.GetComponent<CloudColorStrength>();
            if (cloudColorStrength == null)
            {
                return;
            }

            if (!cloudColorStrength.IsActive())
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            var renderer = renderingData.cameraData.renderer;

            source = renderer.cameraColorTarget;

            destination = renderer.cameraColorTarget;


            Render(cmd, renderingData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void Render(CommandBuffer cmd, RenderingData renderingData)
        {
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;
            cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc);
            cmd.SetGlobalTexture(mainTexId, source);
            material.SetFloat(strength, cloudColorStrength.strength.value);
            material.SetColor(color, cloudColorStrength.color.value);
            Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(renderingData.cameraData.camera.projectionMatrix, false);
            material.SetMatrix(inverseProjectionMatrix, projectionMatrix.inverse);
            material.SetMatrix(inverseViewMatrix, renderingData.cameraData.camera.cameraToWorldMatrix);
            material.SetTexture(noiseTex, settings.texture3D);
            cmd.Blit(source, m_TemporaryColorTexture.Identifier(), material);
            cmd.Blit(m_TemporaryColorTexture.Identifier(), destination);
        }

        public override void FrameCleanup(CommandBuffer cmd) { }
    }

    [System.Serializable]
    public class BlitSettings
    {
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;
        public Material material = null;
        public int blitMaterialPassIndex = 0;
        public bool requireDepthNormals = false;
        public Texture3D texture3D;
    }

    public BlitSettings settings = new BlitSettings();
    private BlitPass blitPass;

    public override void Create()
    {
        var passIndex = settings.material != null ? settings.material.passCount - 1 : 1;
        settings.blitMaterialPassIndex = Mathf.Clamp(settings.blitMaterialPassIndex, -1, passIndex);
        blitPass = new BlitPass(settings.Event, settings, name);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        blitPass.Setup(renderer);
        renderer.EnqueuePass(blitPass);
    }
}
}

