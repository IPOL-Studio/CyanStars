Shader "CyanStars/UI/PopupBlur"
{
    // 弹窗面板范围内的背景模糊：
    //   0. 备份一份全分辨率原图
    //   1. 降采样到半分辨率
    //   2. 来回做若干次可分离高斯模糊
    //   3. 用面板贴图的 alpha 作为遮罩，把模糊结果混回相机颜色
    // 遮罩来自弹窗背景图自身的 alpha，因此模糊范围与面板形状完全一致，不会糊到面板外面
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        // 每一趟高斯的抽头范围，单位是「半分辨率像素」，为 0 时不产生任何模糊
        float _UIPopupBlurRadius;

        // 面板在屏幕上的矩形 (xMin, yMin, xMax, yMax)，归一化屏幕坐标（原点在左下）
        float4 _UIPopupBlurPanelRect;

        // 面板 sprite 在其纹理中的 uv 范围 (uMin, vMin, uSize, vSize)
        float4 _UIPopupBlurPanelUV;

        // 面板 alpha 到遮罩的映射区间
        float2 _UIPopupBlurMaskRange;

        TEXTURE2D_X(_UIPopupBlurOriginal);
        TEXTURE2D(_UIPopupBlurPanelMask);

        // 9 抽头高斯核权重，下标 0 为中心，1 ~ 4 为对称的抽头
        static const float BlurWeights[5] =
        {
            0.2270270270, 0.1945945946, 0.1216216216, 0.0540540541, 0.0162162162
        };

        float4 SampleSource(float2 uv)
        {
            return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
        }

        // 沿单一方向做一次高斯模糊，stepUV 为相邻抽头之间的 UV 间隔
        float4 BlurDirection(float2 uv, float2 stepUV)
        {
            float4 color = SampleSource(uv) * BlurWeights[0];

            [unroll]
            for (int i = 1; i < 5; i++)
            {
                float2 offset = stepUV * i;
                color += (SampleSource(uv + offset) + SampleSource(uv - offset)) * BlurWeights[i];
            }

            return color;
        }

        // Blit 的纹理 uv 与屏幕 uv 在部分平台上会上下翻转，这里统一换算成屏幕 uv（原点在左下）
        float2 ToScreenUV(float2 uv)
        {
            #if UNITY_UV_STARTS_AT_TOP
            if (_ProjectionParams.x < 0.0)
                uv.y = 1.0 - uv.y;
            #endif
            return uv;
        }
        ENDHLSL

        // Pass 0：4 抽头盒式降采样到半分辨率
        Pass
        {
            Name "UIPopupBlurDownsample"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragDownsample

            float4 FragDownsample(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // 此时的源为全分辨率相机颜色
                float2 offset = 0.5 / _ScreenParams.xy;
                float2 uv = input.texcoord;

                float4 color = SampleSource(uv + float2(-offset.x, -offset.y));
                color += SampleSource(uv + float2(offset.x, -offset.y));
                color += SampleSource(uv + float2(-offset.x, offset.y));
                color += SampleSource(uv + float2(offset.x, offset.y));

                return color * 0.25;
            }
            ENDHLSL
        }

        // Pass 1：半分辨率下的横向高斯模糊
        Pass
        {
            Name "UIPopupBlurHorizontal"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragHorizontal

            float4 FragHorizontal(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // 半分辨率纹理的 1 像素等于 _ScreenParams 的 2 像素
                float2 halfResTexel = 2.0 / _ScreenParams.xy;
                float2 stepUV = float2(halfResTexel.x, 0.0) * (_UIPopupBlurRadius * 0.25);

                return BlurDirection(input.texcoord, stepUV);
            }
            ENDHLSL
        }

        // Pass 2：半分辨率下的纵向高斯模糊
        Pass
        {
            Name "UIPopupBlurVertical"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragVertical

            float4 FragVertical(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 halfResTexel = 2.0 / _ScreenParams.xy;
                float2 stepUV = float2(0.0, halfResTexel.y) * (_UIPopupBlurRadius * 0.25);

                return BlurDirection(input.texcoord, stepUV);
            }
            ENDHLSL
        }

        // Pass 3：按面板遮罩把模糊结果混回原图
        Pass
        {
            Name "UIPopupBlurCombine"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragCombine

            float4 FragCombine(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;

                float4 blurred = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                float4 original = SAMPLE_TEXTURE2D_X(_UIPopupBlurOriginal, sampler_LinearClamp, uv);

                // 面板局部 uv
                float2 screenUV = ToScreenUV(uv);
                float4 panelRect = _UIPopupBlurPanelRect;
                float2 panelSize = max(panelRect.zw - panelRect.xy, 1e-5);
                float2 localUV = (screenUV - panelRect.xy) / panelSize;
                float2 maskUV = _UIPopupBlurPanelUV.xy + localUV * _UIPopupBlurPanelUV.zw;

                float panelAlpha = SAMPLE_TEXTURE2D(_UIPopupBlurPanelMask, sampler_LinearClamp, maskUV).a;
                float mask = smoothstep(_UIPopupBlurMaskRange.x, _UIPopupBlurMaskRange.y, panelAlpha);

                return lerp(original, blurred, mask);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
