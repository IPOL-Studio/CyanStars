Shader "SceneShader/Rotate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Color ("Color", color) = (1, 1, 1, 1)
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
            float4 _Color;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normalWS : NORMAL;
            };

            float2x2 Rotate()
            {
                float sin_radianY, cos_radianY;
                sincos(_Time.y * 0.1, sin_radianY, cos_radianY);
                float2x2 Rotate_Matrix_Z = float2x2(cos_radianY, sin_radianY, -sin_radianY, cos_radianY);
                return  Rotate_Matrix_Z;
            }

            v2f vert (appdata v)
            {
                v2f o;

                v.vertex.xy = mul(Rotate(), float2(v.vertex.x, v.vertex.y));

                o.normalWS = v.normal;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }



            half4 frag (v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float3 normalWS = normalize(float3(mul(Rotate(), float2(i.normalWS.x, i.normalWS.y)), i.normalWS.z));
                float3 worldLight = normalize(_MainLightPosition.xyz);
                float NdotL = max(0.0, dot(normalWS, worldLight));
                return _Color;
            }
            ENDHLSL
        }
    }
}
