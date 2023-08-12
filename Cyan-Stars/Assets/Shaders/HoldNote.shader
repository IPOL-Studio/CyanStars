Shader "NoteEffect/HoldNote"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset]_Mask ("Mask", 2D) = "white" {}

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
            // #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            //https://docs.unity3d.com/cn/current/Manual/GPUInstancing.html
            //使用 GPU 实例化无法有效处理具有少量顶点的网格，因为 GPU 无法以充分利用 GPU 资源的方式分配工作。这种处理效率低下会对性能产生不利影响。开始效率低下的阈值取决于 GPU，但作为一般规则，不要对顶点数少于 256 的网格使用 GPU 实例化。
            //注释掉了CBUFFER是因为SRP Batches优先级比较高会阻止GPUInstancing
            // CBUFFER_START(UnityPerMaterial)
            // half _Flicker;
            half _FlickerSpeed;
            half _FlickerRate;

            float4 _FlickerColor;
            float4 _MaskColor;
            // CBUFFER_END

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _Flicker)
            UNITY_INSTANCING_BUFFER_END(Props)

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_Mask); SAMPLER(sampler_Mask);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                // UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                // UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert (appdata v)
            {
                v2f o;
                // UNITY_SETUP_INSTANCE_ID(v);
                // UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                // UNITY_SETUP_INSTANCE_ID(i);
                half UV_Y = i.uv.y;
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                if(i.uv.x < 0.4)
                {
                    UV_Y = abs(UV_Y - 0.5) * _FlickerRate;
                    UV_Y = frac(_Time.y * _FlickerSpeed - UV_Y);
                    UV_Y = smoothstep(0.9, 1, cos(UV_Y - 0.5));
                    col += col * (1 - UV_Y) * _FlickerColor * UNITY_ACCESS_INSTANCED_PROP(Props, _Flicker);
                }
                col += SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.uv).r * _MaskColor;
                return col;
            }
            ENDHLSL
        }
    }
}
