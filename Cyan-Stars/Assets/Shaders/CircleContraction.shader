Shader "UI/CircleContraction"
{
    Properties
    {
        [PerRendererData] _MainTex ("MainTex", 2D) = "white" {}
        [Toggle(_SCREEN_PARAMS_ON)] _SCREEN_PARAMS ("Enable Screen Size", float) = 0
        _Radius ("Radius", range(0, 1)) = 0
        _Blur ("Blur", range(0, 0.1)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off //关闭剔除
        Lighting Off
        ZWrite Off //关闭深度写入
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _SCREEN_PARAMS_ON _SCREEN_PARAMS_OFF

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct a2v
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                half2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Radius;
            float _Blur;
            float2 _RenderTargetSize;

            v2f vert(a2v i)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = TransformObjectToHClip(i.vertex.xyz);
                o.uv = i.uv;
                o.color = i.color;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 screenUV = i.uv - 0.5;

                #if defined (_SCREEN_PARAMS_ON)
                    screenUV.x *= _ScreenParams.x / _ScreenParams.y;
                #else
                    screenUV.x *= _RenderTargetSize.x / _RenderTargetSize.y;
                #endif

                float centerUV = length(abs(screenUV));
                float r = _Radius * 1.05;
                float mask = smoothstep(r - _Blur, r, centerUV);
                half4 color = half4((tex2D(_MainTex, i.uv) * i.color).rgb, mask * i.color.a);
                return color;
            }
            ENDHLSL
        }
    }
}
