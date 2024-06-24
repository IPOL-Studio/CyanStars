Shader "Shaders/BandShader"
{
    Properties
    {
        _Size ("整体大小", int) = 10
        _Aspect ("条带数", float) = 5
        _YSizeOffset ("误差值", float) = 0
        _XOffset ("左右偏移", float) = 0
        _YOffset ("上下偏移", float) = 0
        _Width ("条带左右范围限制", float) = 0
        _Color ("条带颜色", color) = (1, 1, 1, 1)
        _GeadientColor ("渐变颜色", color) = (1, 1, 1, 1)
        _Seed ("随机种", float) = 0
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
            float _YSizeOffset;
            float _XOffset;
            float _YOffset;
            float _Width;
            float4 _Color;
            float4 _GeadientColor;
            float _Seed;
            float _GeadientOffset;

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

            float hash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * .1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 aspect = float2(_Aspect, 1);
                float2 offset = float2(_XOffset, _YOffset);
                float2 uv = i.uv * aspect * _Size + offset;
                float2 gridUV = float2(frac(uv)) - 0.5;
                float2 gridID = float2(floor(uv));
                float2 GridCount = aspect * _Size;

                float ySize = 0.5 - abs((gridID.x + 1) / GridCount.x - 0.5);
                float yMask = step(gridID.y, GridCount.y - 1) - step(gridID.y, GridCount.y - 2);
                float gridNoise = hash12(gridID + _Seed) * 0.1;

                float x = step(abs(gridUV.x), 0.2);
                float yBase = step(abs(gridUV.y), 0.01);
                float y = saturate(step(abs(gridUV.y), clamp(0, 0.5, ySize + _YSizeOffset * 0.1 + gridNoise)) + yBase);
                y *= step(abs(i.uv.x - 0.5), _Width * 0.1);

                float mask = x * y * yMask;
                float gradient = smoothstep(1.1, 0.9 + _GeadientOffset * 0.01, i.uv.y);

                float4 col = SAMPLE_TEXTURE2D(_ColorTexture, sampler_ColorTexture, i.uv.xy);
                col.rgb = col.rgb * gradient + _GeadientColor.rgb * (1 - gradient);
                return col * (1 - mask) + float4(_Color.rgb * _Color.a + col.rgb * (1 - _Color.a), 1) * mask;
            }
            ENDHLSL
        }
    }
}
