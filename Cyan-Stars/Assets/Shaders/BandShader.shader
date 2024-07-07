Shader "Shaders/BandShader"
{
    Properties
    {
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

            float4 _Aspect;
            int _Width;
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
                float2 aspect = _Aspect.xy;
                float2 offset = float2(_Aspect.z, _Aspect.w);
                float2 uv = i.uv * aspect + offset;
                float2 gridUV = float2(frac(uv)) - 0.5;
                float2 gridID = float2(floor(uv));

                float gradient = smoothstep(1.1, 0.9 - _GeadientOffset * 0.01, i.uv.y);
                float yMask = step(gridID.y, aspect.y - 1) - step(gridID.y, aspect.y - 2);

                //如果不在条带的范围就提前退出
                if (gridID.x < _Width || gridID.x >= aspect.x - _Width - 1 || !yMask)
                {
                    float4 bgCol = SAMPLE_TEXTURE2D(_ColorTexture, sampler_ColorTexture, i.uv.xy);
                    return bgCol * gradient + _GeadientColor * (1 - gradient);
                }

                //读取偏移量
                float gridGrow = grid[gridID.x - _Width] * 0.5;
                //每个小格子的xmask
                float x = step(abs(gridUV.x), 0.2);
                //最小的ymask
                float yBase = step(abs(gridUV.y), 0.01);
                //每个小格子的ymask
                float y = saturate(step(abs(gridUV.y), clamp(0, 0.5, gridGrow)) + yBase);

                float mask = x * y * yMask;

                //采样颜色
                float4 bgCol = SAMPLE_TEXTURE2D(_ColorTexture, sampler_ColorTexture, i.uv.xy);
                float4 gradientCol = bgCol * gradient + _GeadientColor * (1 - gradient);
                return gradientCol * (1 - mask) + float4(_Color.rgb * _Color.a + bgCol.rgb * (1 - _Color.a), 1) * mask;
            }
            ENDHLSL
        }
    }
}
