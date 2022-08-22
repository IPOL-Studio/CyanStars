Shader "Unlit/holo"
{
    Properties
    {
        _MainTex ("MainTex", 2d) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _HoloTex ("HoloTex", 2d) = "white" {}
        _glitchRate ("glitchRate", float) = 0.2
    }
    SubShader
    {
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}

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
            float _glitchRate;
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
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float a = frac(_Time.y);
                a = step(a, _glitchRate);
                float4 holo =  SAMPLE_TEXTURE2D(_HoloTex, sampler_HoloTex, i.uv) * a;
                return holo;
            }
            ENDHLSL
        }
    }
}
