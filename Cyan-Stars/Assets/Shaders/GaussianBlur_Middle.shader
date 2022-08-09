Shader "Custom/GaussianBlur_Middle"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BlurSize ("Blur Size", Float) = 1.0
    }

    SubShader
    {
        HLSLINCLUDE

       	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _BlurSize;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        struct a2v
        {
            float4 vertex : POSITION;
            float2 texcoord : TEXCOORD0;
        };

        struct v2f
        {
            float4 pos : SV_POSITION;
            float2 uv[5] : TEXCOORD0;
        };

        v2f vertBlueVertical(a2v v)
        {
            v2f o;
            o.pos = TransformObjectToHClip(v.vertex.xyz);

            float2 uv = v.texcoord;

            o.uv[0] = uv;
            o.uv[1] = uv + float2(0.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;
            o.uv[2] = uv - float2(0.0, _MainTex_TexelSize.y * 1.0) * _BlurSize;
            o.uv[3] = uv + float2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurSize;
            o.uv[4] = uv - float2(0.0, _MainTex_TexelSize.y * 2.0) * _BlurSize;

            return o;
        }

        v2f vertBlueHorizontal(a2v v)
        {
            v2f o;
            o.pos = TransformObjectToHClip(v.vertex.xyz);

            float2 uv = v.texcoord;

            o.uv[0] = uv;
            o.uv[1] = uv + float2(_MainTex_TexelSize.x * 1.0, 0.0) * _BlurSize;
            o.uv[2] = uv - float2(_MainTex_TexelSize.x * 1.0, 0.0) * _BlurSize;
            o.uv[3] = uv + float2(_MainTex_TexelSize.x * 2.0, 0.0) * _BlurSize;
            o.uv[4] = uv - float2(_MainTex_TexelSize.x * 2.0, 0.0) * _BlurSize;

            return o;
        }

        half4 fragBlur(v2f i) : SV_TARGET
        {
            float weight[3] = {0.4026, 0.2442, 0.0545};

            half3 sum = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv[0]).rgb * weight[0];

            for(int it = 1; it < 3; it++)
            {
                sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv[it*2-1]).rgb * weight[it];
                sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv[it*2]).rgb * weight[it];
            }

            return half4(sum, 1.0);
        }

        ENDHLSL

        ZTest Always ZWrite Off Cull Off

        pass
        {
            Name "GAUSSIAN_BLUR_VERTICAL"

            HLSLPROGRAM

            #pragma vertex vertBlueVertical
            #pragma fragment fragBlur

            ENDHLSL
        }

        pass
        {
            Name "GAUSSIAN_BLUR_HORIZONTAL"

            HLSLPROGRAM

            #pragma vertex vertBlueHorizontal
            #pragma fragment fragBlur

            ENDHLSL
        }
    }
    Fallback Off
}
