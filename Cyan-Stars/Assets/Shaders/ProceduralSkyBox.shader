//reference : https://www.shadertoy.com/view/XtGGRt
//reference : https://www.shadertoy.com/view/3slXDB
Shader "Skybox/ProceduralSkyBox"
{
    Properties
    {
        _MoonTex ("MoonTex", 2d) = "white" {}
        _StarTex ("StarTex", 2d) = "white" {}
        _NoiseTex ("NoiseTex", 2d) = "white" {}
        [HDR]_SunColor ("SunColor", color) = (1, 1, 1, 1)
        _SunSetColor ("SunSetColor", color) = (1, 1, 1, 1)
        _DayTopColor ("DayTopColor", color) = (1, 1, 1, 1)
        _DayBottomColor ("DayBottomColor", color) = (1, 1, 1, 1)
        _NightTopColor ("NightTopColor", color) = (1, 1, 1, 1)
        _NightBottomColor ("NightBottomColor", color) = (1, 1, 1, 1)
        _HorizonDayColor ("HorizonDayColor", color) = (1, 1, 1, 1)
        _HorizonNightColor ("HorizonNightColor", color) = (1, 1, 1, 1)
    	_SkyGradientDayColTime ("SkyGradientDayColTime", range(0, 1)) = 0.4
        _SunSize ("SunSize", range(0, 1)) = 0.05
        _SunGlow ("SunGlow", range(1, 10)) = 1
        _MoonSize ("MoonSize", float) = 1
	    _MoonGlowSize ("MoonGlowSize", float) = 1
        [HDR]_MoonColor ("MoonColor", color) = (1, 1, 1, 1)
	    _MoonGlowColor ("MoonGlowColor", color) = (1, 1, 1, 1)
    	_StarDensity ("StarDensity", float) = 1
    	_StarTwinkleFrequency ("StarTwinkleFrequency", float) = 1
    	_StarHeight ("StarHeight", range(0, 1)) = 0
    	_AuroraColor ("AuroraColor", color) = (1, 1, 1, 1)
    	_AuroraSpeed ("_AuroraSpeed" , float) = 1
    	_SurAuroraColFactor ("SurAuroraColFactor", range(0, 1)) = 0.5
    	_AuroraStrength ("AuroraStrength", float) = 0.2
    }
    SubShader
    {
        Tags{"Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline"}

        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _MoonTex_ST;
            float4 _StarTex_ST;
            float4 _CloudNoiseTex_ST;
            float4 _SunColor;
            float4 _MoonColor;
            float4 _MoonGlowColor;
            float4 _SunSetColor;
            float4 _DayTopColor;
            float4 _DayBottomColor;
            float4 _NightTopColor;
            float4 _NightBottomColor;
            float4 _HorizonDayColor;
            float4 _HorizonNightColor;
            float4x4 _LtoW;
            float _SkyGradientDayColTime;
            float _SunSize;
            float _SunGlow;
            float _MoonSize;
            float _MoonGlowSize;

			float _PlanetRadius;
            float _AtmosphereHeight;
			float2 _DensityScaleHeight;
			float _MieG;
			float3 _ScatteringM;
			float3 _ExtinctionM;
            float4 _IncomingLight;

            float _StarDensity;
            float _StarTwinkleFrequency;
            float _StarHeight;

            float4 _AuroraColor;
            float _AuroraSpeed;
            float _SurAuroraColFactor;
            float _AuroraStrength;

            TEXTURE2D(_MoonTex); SAMPLER(sampler_MoonTex);
            TEXTURE2D(_StarTex); SAMPLER(sampler_StarTex);
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            	float3 tangent : TANGENT;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            //https://www.jianshu.com/p/1b008ed86627
            //TODO: 按照网页中的简化公式写一下
            //射线求交函数
            float2 RaySphereIntersection(float3 rayOrigin, float3 rayDir, float3 sphereCenter, float sphereRadius)
			{
				rayOrigin -= sphereCenter;
				float a = dot(rayDir, rayDir);
				float b = 2.0 * dot(rayOrigin, rayDir);
				float c = dot(rayOrigin, rayOrigin) - (sphereRadius * sphereRadius);
				float d = b * b - 4 * a * c;
				if (d < 0)
				{
					return -1;
				}
				else
				{
					d = sqrt(d);
					return float2(-b - d, -b + d) / (2 * a);
				}
			}

            void ComputeOutLocalDensity(float3 position, float3 lightDir, out float localDPA, out float DPC)
			{
				float3 planetCenter = float3(0, -_PlanetRadius, 0);
				float height = length(position - planetCenter) - _PlanetRadius;
				localDPA = exp(-height / _DensityScaleHeight.y);

				DPC = 0;
			}

            float MiePhaseFunction(float cosAngle)
			{
				// m
				float g = _MieG;
				float g2 = g * g;
				float phase = (1.0 / (4.0 * PI)) * ((3.0 * (1.0 - g2)) / (2.0 * (2.0 + g2))) * ((1 + cosAngle * cosAngle) / (pow((1 + g2 - 2 * g * cosAngle), 3.0 / 2.0)));
				return phase;
			}

			float4 IntegrateInscattering(float3 rayStart,float3 rayDir,float rayLength, float3 lightDir,float sampleCount)
			{
				float3 stepVector = rayDir * (rayLength / sampleCount);
				float stepSize = length(stepVector);

				float3 scatterMie = 0;

				float densityCP = 0;
				float densityPA = 0;
				float localDPA = 0;

				float prevLocalDPA;
				float3 prevTransmittance;

				ComputeOutLocalDensity(rayStart,lightDir, localDPA, densityCP);

				densityPA += localDPA * (stepSize / 2);
				prevLocalDPA = localDPA;

				float Transmittance = exp(-densityPA * _ExtinctionM) * localDPA;

				prevTransmittance = Transmittance;

				[loop]
				for(float i = 1.0; i < sampleCount; i += 1.0)
				{
					float3 P = rayStart + stepVector * i;

					ComputeOutLocalDensity(P, lightDir, localDPA, densityCP);
					densityPA += (prevLocalDPA + localDPA) * (stepSize / 2);

					Transmittance = exp(-densityPA * _ExtinctionM) * localDPA;

					scatterMie += (prevTransmittance + Transmittance) * (stepSize / 2);

					prevTransmittance = Transmittance;
					prevLocalDPA = localDPA;
				}

				scatterMie = scatterMie * MiePhaseFunction(dot(rayDir, -lightDir.xyz));

				float3 lightInscatter = _ScatteringM * scatterMie * _IncomingLight.xyz;

				return float4(lightInscatter,1);
			}

            float3 ACESFilm(float3 x)
			{
				float a = 2.51f;
				float b = 0.03f;
				float c = 2.43f;
				float d = 0.59f;
				float e = 0.14f;
				return saturate((x * (a * x + b)) / (x * (c * x + d) + e));
			}

			float AuroraHash(float n ) {
    		    return frac(sin(n)*758.5453);
    		}

    		float AuroraNoise(float3 x)
    		{
    		    float3 p = floor(x);
    		    float3 f = frac(x);
    		    float n = p.x + p.y*57.0 + p.z*800.0;
    		    float res = lerp(lerp(lerp( AuroraHash(n+  0.0), AuroraHash(n+  1.0),f.x), lerp( AuroraHash(n+ 57.0), AuroraHash(n+ 58.0),f.x),f.y),
			    	    lerp(lerp( AuroraHash(n+800.0), AuroraHash(n+801.0),f.x), lerp( AuroraHash(n+857.0), AuroraHash(n+858.0),f.x),f.y),f.z);
    		    return res;
    		}

    		//极光分型
    		float Aurorafbm(float3 p )
    		{
    		    float f  = 0.50000*AuroraNoise( p );
    		    p *= 2.02;
    		    f += 0.25000*AuroraNoise( p );
    		    p *= 2.03;
    		    f += 0.12500*AuroraNoise( p );
    		    p *= 2.01;
    		    f += 0.06250*AuroraNoise( p );
    		    p *= 2.04;
    		    f += 0.03125*AuroraNoise( p );
    		    return f*1.032258;
    		}

    		float GetAurora(float3 p)
    		{
    			p+=Aurorafbm(float3(p.x,p.y,0.0)*0.5)*2.25;
    			float a = smoothstep(.0, .9, Aurorafbm(p*2.)*2.2-1.1);

    			return a<0.0 ? 0.0 : a;
    		}

			float2x2 RotateMatrix(float a){
    		    float c = cos(a);
    		    float s = sin(a);
    		    return float2x2(c,s,-s,c);
    		}

    		float tri(float x){
    		    return clamp(abs(frac(x)-0.5),0.01,0.49);
    		}

    		float2 tri2(float2 p){
    		    return float2(tri(p.x)+tri(p.y),tri(p.y+tri(p.x)));
    		}

    		// 极光噪声
    		float SurAuroraNoise(float2 pos)
    		{
    		    float intensity=1.8;
    		    float size=2.5;
    			float rz = 0;
    		    pos = mul(RotateMatrix(pos.x*0.06),pos);
    		    float2 bp = pos;
    			for (int i=0; i<5; i++)
    			{
    		        float2 dg = tri2(bp*1.85)*.75;
    		        dg = mul(RotateMatrix(_Time.y*_AuroraSpeed),dg);
    		        pos -= dg/size;

    		        bp *= 1.3;
    		        size *= .45;
    		        intensity *= .42;
    				pos *= 1.21 + (rz-1.0)*.02;

    		        rz += tri(pos.x+tri(pos.y))*intensity;
    		        pos = mul(-float2x2(0.95534, 0.29552, -0.29552, 0.95534),pos);
    			}
    		    return clamp(1.0/pow(rz*29., 1.3),0,0.55);
    		}

			float SurHash(float2 n){
    		     return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
    		}

    		float4 SurAurora(float3 pos,float3 ro)
    		{
    		    float4 col = float4(0,0,0,0);
    		    float4 avgCol = float4(0,0,0,0);

    		    // 逐层
    		    for(int i=0;i<30;i++)
    		    {
    		        // 坐标
    		        float of = 0.006*SurHash(pos.xy)*smoothstep(0,15, i);
    		        float pt = ((0.8+pow(i,1.4)*0.002)-ro.y)/(pos.y*2.0+0.8);
    		        pt -= of;
    		    	float3 bpos = ro + pt*pos;
    		        float2 p = bpos.zx;

    		        // 颜色
    		        float noise = SurAuroraNoise(p);
    		        float4 col2 = float4(0,0,0, noise);
    		        col2.rgb = (sin(1.0-float3(2.15,-.5, 1.2)+i*_SurAuroraColFactor*0.1)*0.8+0.5)*noise;
    		        avgCol =  lerp(avgCol, col2, 0.5);
    		        col += avgCol*exp2(-i*0.065 - 2.5)*smoothstep(0.,5., i);

    		    }

    		    col *= (clamp(pos.y*15.+.4,0.,1.));

    		    return col*1.8;

    		}

            float4 frag (v2f i) : SV_Target
            {
                // float3 sunCol = lerp(_SunSetColor, _SunColor, smoothstep(-0.03, 0.03, _MainLightPosition.y)) * saturate(sunArea);

            	//SunAndMoon
            	float sunPoint = distance(i.uv, _MainLightPosition.xyz);
                float sunArea = 1.0 - smoothstep(0.0, _SunSize, sunPoint);
                sunArea *= _SunGlow;
                float3 sunUV = mul(i.uv.xyz, (float3x3)_LtoW);
                float2 moonUV = sunUV.xy * (1 / (_MoonSize + 0.001));
                float4 moonTex = SAMPLE_TEXTURE2D(_MoonTex, sampler_MoonTex, TRANSFORM_TEX(moonUV, _MoonTex));
				float3 sunCol = _SunColor.rgb * saturate(sunArea);
                float3 moonCol = moonTex.rgb * moonTex.a * step(0, sunUV.z) * _MoonColor.rgb;

                float3 sunAndMoonCol = sunCol + moonCol;

            	//MoonGlow
            	float moonPoint = distance(i.uv, -_MainLightPosition.xyz);
				float moonGlowMask = 1.0 - smoothstep(0.0, _MoonGlowSize, moonPoint);
            	float3 moonGlow = moonGlowMask * _MoonGlowColor.rgb;

            	//Sky
                float4 gradientDay = lerp(_DayBottomColor, _DayTopColor, saturate(i.uv.y));
                float4 gradientNight = lerp(_NightBottomColor, _NightTopColor, saturate(i.uv.y));
                float4 skyGradients = lerp(gradientNight, gradientDay, saturate(_MainLightPosition.y + _SkyGradientDayColTime));

            	//Star
                float startMask = lerp(0, 1, -_MainLightPosition.y) * step(_StarHeight, i.uv.y);
            	float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv.xz / i.uv.y + _Time.x * _StarTwinkleFrequency).r;
                float3 star = SAMPLE_TEXTURE2D(_StarTex, sampler_StarTex, i.uv.xz / i.uv.y * _StarDensity).rgb * noise;
                star = saturate(star * startMask);

            	//Mie scattering
				float3 scatteringColor = 0;

				float3 rayStart = float3(0,10,0);
				float3 rayDir = normalize(i.uv.xyz);

				float3 planetCenter = float3(0, -_PlanetRadius, 0);
				float2 intersection = RaySphereIntersection(rayStart, rayDir, planetCenter, _PlanetRadius + _AtmosphereHeight);
				float rayLength = intersection.y;

				intersection = RaySphereIntersection(rayStart, rayDir, planetCenter, _PlanetRadius);
				if (intersection.x > 0)
					rayLength = min(rayLength, intersection.x);

				float4 inscattering = IntegrateInscattering(rayStart, rayDir, rayLength, -_MainLightPosition.xyz, 16);

            	//Aurora
				float3 aurora = float3(_AuroraColor.rgb * GetAurora(float3(i.uv.xy * float2(1.14, 0.95), _Time.y * _AuroraSpeed * 0.33)) * 0.85 +
                        _AuroraColor.rgb * GetAurora(float3(i.uv.xy * float2(1.1, 0.8) , _Time.y * _AuroraSpeed * 0.21)) * 0.77 +
                        _AuroraColor.rgb * GetAurora(float3(i.uv.xy * float2(0.66, 0.87) , _Time.y * _AuroraSpeed * 0.31)) * 0.66 +
                        _AuroraColor.rgb  * GetAurora(float3(i.uv.xy * float2(0.77, 0.55) , _Time.y * _AuroraSpeed * 0.22)) * 0.57);
				float4 auroraCol = float4(aurora, 0.114);

            	//surAurora
            	float4 surAuroraCol = smoothstep(0.0, 1.5, SurAurora(float3(i.uv.x, abs(i.uv.y), i.uv.z),float3(0, 0, -6.7)));

            	//Finally
                return float4(sunAndMoonCol + skyGradients.rgb + star * 3 + auroraCol.rgb * _AuroraStrength + surAuroraCol.rgb * _AuroraStrength + ACESFilm(inscattering.xyz) + moonGlow, 1);
            }
            ENDHLSL
        }
    }
}
