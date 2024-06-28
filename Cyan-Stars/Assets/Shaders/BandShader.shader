Shader "Shaders/BandShader"
{
    Properties
    {
        _Size ("整体大小", int) = 10
        _Aspect ("条带数", float) = 5
        _XOffset ("左右偏移", float) = 0
        _YOffset ("上下偏移", float) = 0
        _Width ("条带左右范围限制", float) = 0
        _Color ("条带颜色", color) = (1, 1, 1, 1)
        _GeadientColor ("渐变颜色", color) = (1, 1, 1, 1)
        _GeadientOffset ("渐变长度", float) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_ColorTexture);
            SAMPLER(sampler_ColorTexture);

            float _Size;
            float _Aspect;
            float _XOffset;
            float _YOffset;
            float _Width;
            float4 _Color;
            float4 _GeadientColor;
            float _GeadientOffset;
            StructuredBuffer<float> grid;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 aspect = float2(_Aspect, 1);
                float2 offset = float2(_XOffset, _YOffset);
                float2 uv = i.uv * aspect * _Size + offset;
                float2 gridUV = float2(frac(uv)) - 0.5;
                float2 gridID = float2(floor(uv));
                float2 GridCount = aspect * _Size;

                float yMask = step(gridID.y, GridCount.y - 1) - step(gridID.y, GridCount.y - 2);
                float gridGrow = grid[gridID.x];

                float x = step(abs(gridUV.x), 0.2);
                float yBase = step(abs(gridUV.y), 0.01);
                float y = saturate(step(abs(gridUV.y), clamp(0, 0.5, gridGrow)) + yBase);
                y *= step(abs(i.uv.x - 0.5), _Width * 0.1);

                float mask = x * y * yMask;
                float gradient = smoothstep(1.1, 0.9 + _GeadientOffset * 0.01, i.uv.y);

                float4 bgCol = SAMPLE_TEXTURE2D(_ColorTexture, sampler_ColorTexture, i.uv.xy);
                float4 gradientCol = bgCol * gradient + _GeadientColor * (1 - gradient);
                return gradientCol * (1 - mask) + float4(_Color.rgb * _Color.a + bgCol.rgb * (1 - _Color.a), 1) * mask;
            }
            ENDHLSL
        }
    }
}
