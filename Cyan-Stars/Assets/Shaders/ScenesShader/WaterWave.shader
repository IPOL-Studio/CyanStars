Shader "Scene/WaterWave"
{
    Properties
    {
        _MainTex ("MainTex", 2d) = "white" {}
        _NoiseTex ("NoiseTex", 2d) = "white" {}
        _Color ("Color", color) = (1, 1, 1, 1)
        _YSpeed ("YSpeed", range(-10, 10)) = 1
        _NoiseStrength ("NoiseStrength", range(0, 5)) = 1
        _warpStrength ("warpStrength", range(0, 1)) = 1
        _Opacity ("Opacity", range(0, 1)) = 1
    }
    SubShader
    {
        Tags{"RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline"}

        blend One OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _NoiseTex_ST;
            float4 _Color;
            float _YSpeed;
            float _NoiseStrength;
            float _warpStrength;
            float _Opacity;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv0.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv0.zw = v.uv;
                o.uv1 = TRANSFORM_TEX(v.uv, _NoiseTex);
                o.uv1.y = o.uv1.y + frac(_YSpeed * _Time.x);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                half warp = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv1);
                half2 uvBias = (warp - 0.5) * _warpStrength;
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv0 + uvBias);
                half noise = lerp(1.0, warp * 2.0, _NoiseStrength);
                noise = max(0.0, noise);
                half mask = smoothstep(0.5, 1, 1 - abs(i.uv0.z - 0.5));
                half opacity = _Opacity * noise;
                return float4(col.rgb * _Color.rgb * mask, opacity);
            }
            ENDHLSL
        }
    }
}
