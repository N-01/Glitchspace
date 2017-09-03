Shader "Unlit/FullscreenQuad"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		CGPROGRAM
		#pragma surface surf Unlit
		#pragma vertex vert

		#include "UnityCG.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct Input {
			float2 uv_MainTex;
		};

		sampler2D _MainTex;
		float4 _Color;

		float rand(float2 co) {
			return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
		}

		void vert(inout appdata_full v) {
			v.vertex = float4(v.vertex.x * 2.0, v.vertex.y * 2.0, 0, 1);
		}
			
		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 col = tex2D(_MainTex, IN.uv_MainTex) * _Color * 1.5;

			o.Albedo = col;
			o.Alpha = 1;
		}

		half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
		{
			return half4(s.Albedo, s.Alpha);
		}
		ENDCG
	}
}
