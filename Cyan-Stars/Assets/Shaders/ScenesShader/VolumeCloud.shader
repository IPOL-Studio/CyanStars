Shader "Scene/VolumeCloud"
{
    Properties
    {
        _MainTex ("MainTex", 2d) = "white" {}
        _Color ("Color", color) = (1, 1, 1, 1)
        _NoiseTex ("NoiseTex", 3d) = "white" {}
        _densityStrength ("DensityStrength", float) = 1
        _LightAbsorption ("lightAbsorption", float) = 1
        _AbsorptionTowardSun ("AbsorptionTowardSun", float) = 1
    }
    SubShader
    {
        Tags{"RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline"}

        blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #define MaxStep 50
            #define LightStep 10


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _MainTex_ST;
            float3 _BoundsMin;
            float3 _BoundsMax;
            float _densityStrength;
            float _LightAbsorption;
            float _AbsorptionTowardSun;
            float4 _Color;

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE3D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCPPRD2;
            };

            float2 rayBoxDst(float3 boundsMin, float3 boundsMax, float3 rayOrigin, float3 invRaydir)
            {
                float3 t0 = (boundsMin - rayOrigin) * invRaydir;
                float3 t1 = (boundsMax - rayOrigin) * invRaydir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);

                float dstA = max(max(tmin.x, tmin.y), tmin.z); //进入点
                float dstB = min(tmax.x, min(tmax.y, tmax.z)); //出去点

                float dstToBox = max(0, dstA);
                float dstInsideBox = max(0, dstB - dstToBox);
                return float2(dstToBox, dstInsideBox);
            }

            float SampleTex3D(float3 startPoint)
            {
                float3 scale = 1 / (_BoundsMax - _BoundsMin);
                float3 center = mul(unity_ObjectToWorld, float4(0, 0, 0 + _Time.y/2, 1));
                return SAMPLE_TEXTURE3D(_NoiseTex, sampler_NoiseTex, (startPoint - center) * scale + float3(0.5, 0.5, 0.5)).r * _densityStrength;
            }

            float LightMarch(float3 rayOrigin)
            {
                float3 rayDir = _MainLightPosition;
                float dstInsideBox = rayBoxDst(_BoundsMin, _BoundsMax, rayOrigin, 1 / rayDir).y;
                float stepSize = dstInsideBox / LightStep;
                float density = 0;
                float3 startPoint = rayOrigin;
                for (int step = 0; step < LightStep; step++)
                {
                    // startPoint += rayOrigin * stepSize;
                    density += SampleTex3D(startPoint) * stepSize;
                }
                float transmittance = exp(-density * _AbsorptionTowardSun);
                return  transmittance;

            }

            float4 RayMarching(float2 rayToContainerInfo, float3 rayOrigin, float3 rayDir)
            {
                float stepSize = rayToContainerInfo.y / MaxStep;
                float3 startPoint = rayOrigin + rayDir * rayToContainerInfo.x;
                float transmittance = 1;
                float3 energy = 0;
                for (int step = 0; step < MaxStep; step++)
                {
                    float density =  SampleTex3D(startPoint);
                    energy += LightMarch(startPoint) * density * transmittance;
                    transmittance *= exp(-density * _LightAbsorption * stepSize);
                    startPoint += rayDir * stepSize;
                }
                return float4(energy, 1 - transmittance);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.worldPos = TransformObjectToWorld( v.vertex.xyz );
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 worldPos = i.worldPos;
                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(worldPos - _WorldSpaceCameraPos);
                float2 rayToContainerInfo = rayBoxDst(_BoundsMin, _BoundsMax, rayOrigin, 1 / rayDir);
                float4 rayMarching = RayMarching(rayToContainerInfo, rayOrigin, rayDir);
                float3 col = rayMarching.rgb * _Color.rgb;
                return float4(col, rayMarching.a);

            }
            ENDHLSL
        }
    }
}
