Shader "Scene/KeyWaterWave"
{
    Properties
    {
        _MainTex ("MainTex", 2d) = "white" {}
        _NoiseTex ("NoiseTex", 2d) = "white" {}
        [HDR]_Color ("Color", color) = (1, 1, 1, 1)
        _XSpeed ("XSpeed", range(-10, 10)) = 1
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
            float _XSpeed;
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
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.uv1 = TRANSFORM_TEX(v.uv, _NoiseTex);
                o.uv1.x += frac(_Time.x * _XSpeed);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                half warp = (SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv1) - 0.5) * _warpStrength;
                // half uvBias = (warp - 0.5) * _warpStrength;
                // half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(i.uv.x, i.uv.y + uvBias * 2));
                // half noise = lerp(1.0, warp * 2.0, _NoiseStrength);
                // noise = max(0.0, noise);
                half mask = smoothstep(0.1, 0, abs(i.uv.y - 0.5));
                float2 uv = i.uv - 0.5;
                float c1 = (uv.x + 1.1) * 0.01/*线宽*/ * abs( 1.0 / sin(uv.y + sin(uv.x * 40.0/*波长*/ * warp + 7.0 * warp + _Time.y * 0.75) * 0.1));
                float c = clamp(c1, 0.0, 1.0);
                c = smoothstep(0.1, 1, c);
                return c * mask * _Color;
                // half opacity = _Opacity * noise;
                // return float4(col.rgb * _Color.rgb * mask, opacity);

            }
            ENDHLSL
        }
    }
}
