using System;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomRendererFeature : ScriptableRendererFeature
{
    public class CustomRendererPass : ScriptableRenderPass
    {
        private Setting setting;
        public Material material = null;
        string m_ProfilerTag;

        RenderTargetIdentifier source;
        RenderTargetIdentifier destination;
        RenderTargetHandle m_TemporaryColorTexture;
        public CustomRendererPass(RenderPassEvent evt, Setting setting, string tag)
        {
            renderPassEvent = evt;
            this.setting = setting;
            material = setting.material;
            m_ProfilerTag = tag;
            m_TemporaryColorTexture.Init("_TemporaryColorTexture");
        }

        public void Setup(ScriptableRenderer renderer)
        {
            if (setting.requireDepthNormals)
            {
                ConfigureInput(ScriptableRenderPassInput.Normal);
            }
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if(material == null && renderingData.cameraData.postProcessEnabled)
            {
                Debug.LogError("Material Not Appoint Or Not Enable PostProcess");
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            var renderer = renderingData.cameraData.renderer;

            source = renderer.cameraColorTarget;
            destination = renderer.cameraColorTarget;

            if (source == destination || setting.srcType == setting.dstType && setting.srcType == Target.CameraColor)
            {
                cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, setting.filterMode);
                Blit(cmd, source, m_TemporaryColorTexture.Identifier(), material, setting.materialPassIndex);
                Blit(cmd, m_TemporaryColorTexture.Identifier(), destination);
            }
            else
            {
                Blit(cmd, source, destination, material, setting.materialPassIndex);
            }


            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
        }
    }

    [System.Serializable]
    public class Setting
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public Material material;
        public int materialPassIndex = 0;

        public Target srcType = Target.CameraColor;
        public Target dstType = Target.CameraColor;

        public FilterMode filterMode = FilterMode.Bilinear;

        public bool requireDepthNormals = false;
    }

    public enum Target {
        CameraColor,
        TextureID,
        RenderTextureObject
    }

    public Setting setting = new Setting();
    CustomRendererPass customRendererPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        customRendererPass.Setup(renderer);
        renderer.EnqueuePass(customRendererPass);
    }

    public override void Create()
    {
        var passIndex = setting.material != null ? setting.material.passCount - 1 : 1;
		setting.materialPassIndex = Mathf.Clamp(setting.material.passCount, -1, passIndex);
        customRendererPass = new CustomRendererPass(setting.renderPassEvent, setting, name);
    }
}
