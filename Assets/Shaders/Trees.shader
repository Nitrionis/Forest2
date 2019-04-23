Shader "Unlit/Trees"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_FogColor("Fog Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile_instancing
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			inline float3 LightingLambertVS(float3 normal, float3 lightDir)
			{
				fixed diff = max(0.3, dot(normal, lightDir));
				return _LightColor0.rgb * diff;
			}

			struct appdata
			{
				float4 vertex : POSITION;
				fixed3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				UNITY_FOG_COORDS(1)
				fixed3 vlight : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			fixed4 _Color;
			fixed4 _FogColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.vertex = UnityObjectToClipPos(v.vertex);
				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				o.vlight = LightingLambertVS(worldNormal, _WorldSpaceLightPos0.xyz);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = fixed4(_Color.xyz * i.vlight, 1);
				//fixed4 col = fixed4(fixed3(1,1,1) * i.vlight, 1);
				UNITY_APPLY_FOG(i.fogCoord, col);
				//col.rgb *= _Color.xyz;
				return col;
			}
			ENDCG
		}
	}
}
