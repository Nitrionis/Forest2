Shader "Unlit/WheelMain"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
		    #pragma multi_compile_instancing
			#pragma target 3.0

            #include "UnityCG.cginc"
		    #include "Lighting.cginc"

		    inline float3 LightingLambertVS(float3 normal, float3 lightDir)
			{
				fixed diff = max(0.3, dot(normal, lightDir));
				return _LightColor0.rgb * diff;
			}

            struct appdata
			{
				float3 vertex : POSITION;
				fixed3 normal : NORMAL;
				fixed4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

            struct v2f
            {
                fixed4 color : COLOR0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

			uniform float4x4 _Rotation;

            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.vertex = mul(_Rotation, float4(v.vertex, 1));
				o.vertex = UnityObjectToClipPos(o.vertex);

                float3 worldNormal = UnityObjectToWorldNormal(v.normal);

				o.color = fixed4(v.color.xyz * LightingLambertVS(worldNormal, _WorldSpaceLightPos0.xyz), 1);
				//o.color = fixed4(v.color.xyz * LightingLambertVS(worldNormal, _WorldSpaceLightPos0.xyz), 1);

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 col = i.color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
