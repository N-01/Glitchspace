Shader "Unlit/BackgroundQuad"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		scroll ("Scroll", float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		CGPROGRAM
		#pragma surface surf Unlit
		#pragma vertex vert

		#include "UnityCG.cginc"
		#include "Perlin/noiseSimplex.cginc" 

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct Input {
			float2 uv_MainTex;
		};

		sampler2D _MainTex;
		float scroll;

		float rand(float2 co) {
			return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
		}

		void vert(inout appdata_full v) {
			v.vertex = float4(v.vertex.x * 2.0, v.vertex.y * 2.0, 0, 1);
		}
			
		void surf(Input IN, inout SurfaceOutput o)
		{
			fixed4 col = tex2D(_MainTex, IN.uv_MainTex);

			float scrollDistorted = scroll;
			float2 noiseCoord = float2(IN.uv_MainTex.x + scrollDistorted, IN.uv_MainTex.y);
			float noise = (snoise(noiseCoord) + snoise(noiseCoord * 10) * 0.6 + snoise(noiseCoord * 20) * 0.3 + snoise(noiseCoord * 100) * 0.15 + snoise(noiseCoord * 500) * 0.25) * 0.7;

			float derivativeOffset = 0.05;
			float left = tex2D(_MainTex, IN.uv_MainTex - (float2(-derivativeOffset, 0))).r;
			float right = tex2D(_MainTex, IN.uv_MainTex - (float2(derivativeOffset, 0))).r;
			float top = tex2D(_MainTex, IN.uv_MainTex - (float2(0, derivativeOffset))).r;
			float bottom = tex2D(_MainTex, IN.uv_MainTex - (float2(0, -derivativeOffset))).r;

			float dx = left - col.r - right;
			float dy = top - col.r - bottom;

			col = tex2D(_MainTex, IN.uv_MainTex - (float2(dx, dy)) * 0.15);

			float3 finalCol = pow(lerp(float3(col.r - dx * 0.2, col.g , col.b - dy * 0.5), float3(noise, noise, noise), 0.16) * 0.85, 1.2) * 1.15;

			o.Albedo = finalCol;
			o.Alpha = 1;
		}

		half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
		{
			return half4(s.Albedo, s.Alpha);
		}


		ENDCG
	}
}
