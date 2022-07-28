Shader "NoteEffect/HoldNote"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Mask ("Mask", 2D) = "white" {}

        _Flicker ("Flicker", float) = 0
        _FlickerSpeed ("FlickerSpeed", float) = 0
        _FlickerRate ("FlickerRate", float) = 0

        [HDR] _FlickerColor ("FlickerColor", Color) = (0, 0, 0, 0)
        [HDR] _MaskColor ("MaskColor", Color) = (0, 0, 0, 0)

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

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Mask_ST;
            half _Flicker;
            half _FlickerSpeed;
            half _FlickerRate;

            float4 _FlickerColor;
            float4 _MaskColor;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_Mask); SAMPLER(sampler_Mask);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half UV_Y = i.uv.y;
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                if(i.uv.x < 0.4 && i.uv.y > 0.1 && i.uv.y < 0.9)
                {
                    UV_Y = abs(UV_Y - 0.5) * _FlickerRate;
                    UV_Y = frac(_Time.y * _FlickerSpeed - UV_Y) * _Flicker;
                    UV_Y = smoothstep(0.9, 1, cos(UV_Y - 0.5));
                    col += col * (1 - UV_Y) * _FlickerColor;
                }
                col += SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.uv) * _MaskColor;
                return col;
            }
            ENDHLSL
        }
    }
}
