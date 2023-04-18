Shader "ScenesEffect/PlanarReflections"
{
    Properties
    {
//        _ReflectDistortInt ("ReflectDistortInt", float) = 1
        _NoiseTex ("NoiseTex", 2d) = "white" {}
        _XSpeed ("XSpeed", range(-1, 1)) = 0
        _YSpeed ("YSpeed", range(-1, 1)) = 0
    }
    SubShader
    {
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // CBUFFER_START(UnityPerMaterial)
            // float _ReflectDistortInt;
            // CBUFFER_END
            float4 _NoiseTex_ST;
            float _XSpeed;
            float _YSpeed;

            TEXTURE2D(_PlanarReflectionTexture); SAMPLER(sampler_PlanarReflectionTexture);
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
                o.uv += float2(_XSpeed, _YSpeed) * _Time.x;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv).r - 0.5;
                float4 reflect = SAMPLE_TEXTURE2D(_PlanarReflectionTexture, sampler_PlanarReflectionTexture, i.screenPos.xy / i.screenPos.w + noise * 0.1);
                return reflect;
            }
            ENDHLSL
        }
    }
}
