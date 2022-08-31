Shader "SceneShader/Shelter"
{
    Properties
    {
        _CellNoiseTex ("NoiseTex", 2d) = "white" {}
        _Mask ("Mask", 2D) = "white" {}
        _Speed ("Speed", float) = 1
        _Strength ("Strength", float) = 1
        _Disslove ("Disslove", float) = 1
        [HDR]_Color ("Color", color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}

        ZTest Always
Blend SrcAlpha One

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float _Speed;
            float _Strength;
            float _Disslove;
            CBUFFER_END

            TEXTURE2D(_Mask); SAMPLER(sampler_Mask);
            TEXTURE2D(_CellNoiseTex); SAMPLER(sampler_CellNoiseTex);

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
                o.uv = v.uv;
                return o;
            }

            //Unity ShaderGraph Twirl Node
            //https://docs.unity3d.com/Packages/com.unity.shadergraph@10.8/manual/Twirl-Node.html
            void Unity_Twirl_float(float2 UV, float2 Center, float Strength, float2 Offset, out float2 Out)
            {
                float2 delta = UV - Center;
                float angle = Strength * length(delta);
                float x = cos(angle) * delta.x - sin(angle) * delta.y;
                float y = sin(angle) * delta.x + cos(angle) * delta.y;
                Out = float2(x + Center.x + Offset.x, y + Center.y + Offset.y);
            }

            float cicle(float2 uv,float antialias)
            {
			    float d = length(uv - float2(0.5, 0.5));
			    float t = smoothstep(0, antialias, d);
			    return float(1.0 - t);
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 uv;
                float2 offset = _Time.y * _Speed;
                Unity_Twirl_float(i.uv, float2(0.5, 0.5), _Strength, offset, uv);
                float4 col = SAMPLE_TEXTURE2D(_CellNoiseTex, sampler_CellNoiseTex, uv);
                float mask = cicle(i.uv, 0.7);
                col = pow(col, _Disslove) * mask * _Color;
                // clip(col.a - 0.1);
                return col;
            }
            ENDHLSL
        }
    }
}
