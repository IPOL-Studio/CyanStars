Shader "NoteShader/Hold"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}

        _Flicker ("Flicker", float) = 0
        _FlickerSpeed ("FlickerSpeed", float) = 0
        _FlickerRate ("FlickerRate", Range(0, 1)) = 0

        [HDR] _FlickerColor ("FlickerColor", Color) = (0, 0, 0, 0)
        [HDR] _BloomColor ("BloomColor", Color) = (0, 0, 0, 0)

    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            // half _Flicker;
            // float _FlickerSpeed;
            float _FlickerRate;
            float4 _FlickerColor;
            float4 _BloomColor;
            CBUFFER_END

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _Flicker)
                UNITY_DEFINE_INSTANCED_PROP(float, _FlickerSpeed)
            UNITY_INSTANCING_BUFFER_END(Props)

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct a2v
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(a2v v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float UV_Y = i.uv.y;

                UV_Y = abs(UV_Y - 0.5);
                UV_Y = frac(_Time.y * _FlickerSpeed - UV_Y);
                UV_Y = smoothstep(_FlickerRate, 1, cos(UV_Y - 0.5));
                float flicker = (1 - UV_Y) * UNITY_ACCESS_INSTANCED_PROP(Props, _Flicker) * step(i.uv.x, 0.4);

                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                col.rgb += _FlickerColor.rgb * flicker;
                col.rgb += col.a * _BloomColor.rgb;

                return col;
            }
            ENDHLSL
        }
    }
}
