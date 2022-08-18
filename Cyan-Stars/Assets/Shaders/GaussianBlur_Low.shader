Shader "Custom/GaussianBlur_Low"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_offsets("Offset",vector) = (0,0,0,0)
	}
	HLSLINCLUDE
	    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        //#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _offsets;
        CBUFFER_END

        struct ver_blur
	    {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f_blur
	    {
	        float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
	        float4 uv01 : TEXCOORD1;
	        float4 uv23 : TEXCOORD2;
	        float4 uv45 : TEXCOORD3;
            UNITY_VERTEX_OUTPUT_STEREO
        };

	    TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);


	    //vertex shader
	    v2f_blur vert_blur(ver_blur v)
	    {
	    	v2f_blur o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	    	o.pos = TransformObjectToHClip(v.positionOS.xyz);
	    	o.uv = v.uv;
            _offsets *= _MainTex_TexelSize.xyxy;
            o.uv01 = v.uv.xyxy + _offsets.xyxy * float4(1, 1, -1, -1);
	    	o.uv23 = v.uv.xyxy + _offsets.xyxy * float4(1, 1, -1, -1) * 2.0;
	    	o.uv45 = v.uv.xyxy + _offsets.xyxy * float4(1, 1, -1, -1) * 3.0;

	    	return o;
	    }

	    //fragment shader
	    float4 frag_blur(v2f_blur i) : SV_Target
	    {
	    	half4 color = half4(0,0,0,0);
	    	//将像素本身以及像素左右（或者上下，取决于vertex shader传进来的uv坐标）像素值的加权平均
	    	color = 0.4 * SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);
	    	color += 0.15 * SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv01.xy);
	    	color += 0.15 * SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv01.zw);
	    	color += 0.10 * SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv23.xy);
	    	color += 0.10 * SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv23.zw);
	    	color += 0.05 * SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv45.xy);
	    	color += 0.05 * SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv45.zw);
	    	return color;
	    }
	ENDHLSL

	//开始SubShader
	SubShader
	{
        Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
        LOD 100

	    //后处理效果一般都是这几个状态
	    ZTest Always Cull Off ZWrite Off

        Pass
        {
            Name "Gaussian Blur"//使用上面定义的vertex和fragment shader
            HLSLPROGRAM
	            #pragma vertex vert_blur
		        #pragma fragment frag_blur
		    ENDHLSL
        }
    }
	//后处理效果一般不给fallback，如果不支持，不显示后处理即可
}
