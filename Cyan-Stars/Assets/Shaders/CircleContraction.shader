Shader "UI/CircleContraction"
{
	Properties
	{
		[PerRendererData] _MainTex ("MainTex", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Slider ("Slider", range(0, 1)) = 0
	}

	SubShader
	{
		Tags{"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True"}

        Cull Off   //关闭剔除
		Lighting Off
		ZWrite Off  //关闭深度写入
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

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
            float4 _Color;
			float _Slider;

			v2f vert(a2v i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(i.vertex);
		    	o.uv = i.uv;
                o.color = i.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
			    float2 screenUV = i.uv - 0.5;
			    screenUV.x *= _ScreenParams.x / _ScreenParams.y;
                float centerUV = length(abs(screenUV));
			    float mask = step(_Slider, centerUV);
                fixed4 color = half4((tex2D(_MainTex,i.uv) * i.color * _Color).rgb, mask);
				return color;
			}
		ENDCG
		}
	}
}
