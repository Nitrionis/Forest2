﻿Shader "Unlit/Skybox"
{
	Properties
	{
		_Tint("Tint Color", Color) = (.5, .5, .5, .5)
		[Gamma] _Exposure("Exposure", Range(0, 8)) = 1.0
		_Rotation("Rotation", float) = 1
		[NoScaleOffset] _Tex("Cubemap   (HDR)", Cube) = "grey" {}
	}

	SubShader
	{
	Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
	Cull Off ZWrite Off

	Pass
	{
		CGPROGRAM
		#pragma target 3.0
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"

		samplerCUBE _Tex;
		half4 _Tex_HDR;
		half4 _Tint;
		half _Exposure;
		float _Rotation;

		float3 RotateAroundYInDegrees(float3 vertex, float degrees)
		{
			float alpha = degrees * UNITY_PI / 180.0;
			float sina, cosa;
			sincos(alpha, sina, cosa);
			float2x2 m = float2x2(cosa, -sina, sina, cosa);
			return float3(mul(m, vertex.xz), vertex.y).xzy;
		}

		struct appdata
		{
			float4 vertex : POSITION;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f
		{
			float4 vertex : SV_POSITION;
			float3 worldPos : COLOR0;
			float3 texcoord : TEXCOORD0;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		v2f vert(appdata v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			float3 rotated = RotateAroundYInDegrees(v.vertex, _Time.y * _Rotation);
			o.worldPos = rotated;
			o.vertex = UnityObjectToClipPos(rotated);
			o.texcoord = v.vertex.xyz;
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			half4 tex = texCUBE(_Tex, i.texcoord);
			half3 c = DecodeHDR(tex, _Tex_HDR);
			c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
			c *= _Exposure;
			float3 normalizePos = normalize(i.worldPos);
			float fogMask = lerp(saturate(pow((0.0 + (abs(normalizePos.y) - 0.0) * (1.0 - 0.0) / (0.6 - 0.0)), ( 1.0 - 0.01 ))), 0.0, 0.0);
			float s = ((1 + sign(i.worldPos.y))) * 0.5;
			fogMask = lerp(0, fogMask, s);
			return lerp(unity_FogColor, half4(c, 1), fogMask);
		}
	ENDCG
	}
	}
	Fallback Off
}