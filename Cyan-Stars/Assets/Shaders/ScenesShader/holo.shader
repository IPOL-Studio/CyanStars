Shader "SceneShader/Holo"
{
    Properties
    {
        _MainTex ("MainTex", 2d) = "white" {}
        [HDR]_Color("Color", Color) = (1, 1, 1, 1)
        _HoloTex ("HoloTex", 2d) = "white" {}
    }
    SubShader
    {
        Tags{"RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline"}

        blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float4 _MainTex_ST;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_HoloTex); SAMPLER(sampler_HoloTex);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            float randomNoise(float x, float y)
            {
                return frac(sin(dot(float2(x, y), float2(12.9898, 78.233))) * 43758.5453);
            }

            float trunc(float x, float num_levels)
            {
                return floor(x * num_levels) / num_levels;
            }

            float4 frag (v2f i) : SV_Target
            {
                float truncTime = trunc(_Time.x, 150.0);
                float uv_trunc = randomNoise(trunc(i.uv.y, 10) , 100.0 * truncTime);
                float2 uv_blockLine = i.uv;
                uv_blockLine = saturate(uv_blockLine + float2(0.05 * uv_trunc, 0));
                float4 holo =  SAMPLE_TEXTURE2D(_HoloTex, sampler_HoloTex, float2(i.uv.x, i.uv.y + _Time.x * 2));
                float4 col =  SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, abs(uv_blockLine)) * _Color;
                return float4(col.xyz, holo.x);
            }
            ENDHLSL
        }
    }
}
