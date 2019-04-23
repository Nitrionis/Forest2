Shader "Unlit/MetalRef"
{
    Properties
    {
		_Cube ("Reflection Map", Cube) = "white" {}
	    _Color("Main Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
		//Cull Off

        Pass
        {
            CGPROGRAM
			#pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
		    #pragma multi_compile_instancing

            #include "UnityCG.cginc"
		    #include "Lighting.cginc"

		    inline float3 LightingLambertVS(float3 normal, float3 lightDir)
			{
				fixed diff = max(0.3, dot(normal, lightDir));
				return _LightColor0.rgb * diff;
			}

            struct appdata
			{
				float4 vertex : POSITION;
				fixed3 normal : NORMAL;
				fixed4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

            struct v2f
            {
                fixed4 color : COLOR0;
				half3 worldRefl : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

			fixed4 _Color;
			samplerCUBE _Cube;
			/*UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)*/

            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);

                o.vertex = UnityObjectToClipPos(v.vertex);

				// compute world space position of the vertex
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // compute world space view direction
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                // world space normal
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                // world space reflection vector
                o.worldRefl = reflect(-worldViewDir, worldNormal);

				o.color = fixed4(v.color.rgb * LightingLambertVS(worldNormal, _WorldSpaceLightPos0.xyz), v.color.a);

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				// sample the default reflection cubemap, using the reflection vector
                fixed4 skyData = texCUBE(_Cube, i.worldRefl);

				fixed4 col = fixed4(lerp(i.color.rgb, skyData.rgb, 0.2), 1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col * ((1 - i.color.a) * 2 + 1);
            }
            ENDCG
        }
    }
}