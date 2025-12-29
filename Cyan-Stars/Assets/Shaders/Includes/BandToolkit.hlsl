#ifndef CYANSTARS_BAND_INCLUDED
#define CYANSTARS_BAND_INCLUDED

float4 _Aspect;
int _Width;
StructuredBuffer<float> grid;

void CreateBand_float(float4 bgCol, float2 iuv, float4 color, float4 geadientColor, float geadientOffset, out float4 v)
{
    float2 aspect = _Aspect.xy;
    float2 offset = float2(_Aspect.z, _Aspect.w);
    float2 uv = iuv * aspect + offset;
    float2 gridUV = float2(frac(uv)) - 0.5;
    float2 gridID = float2(floor(uv));

    float gradient = smoothstep(1.1, 0.9 - geadientOffset * 0.01, iuv.y);
    float yMask = step(gridID.y, aspect.y - 1) - step(gridID.y, aspect.y - 2);

    //如果不在条带的范围就提前退出
    if (gridID.x < _Width || gridID.x >= aspect.x - _Width - 1 || !yMask)
    {
        v = bgCol * gradient + geadientColor * (1 - gradient);
        return;
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

    float4 gradientCol = bgCol * gradient + geadientColor * (1 - gradient);
    v = gradientCol * (1 - mask) + float4(color.rgb * color.a + bgCol.rgb * (1 - color.a), 1) * mask;
}

#endif