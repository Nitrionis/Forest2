Shader "Unlit/Texture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
	    _Color("Glow Color", Color) = (1,1,1,1)
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
		    #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				fixed3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				fixed3 diff : COLOR0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _Color;

			inline fixed3 LightingLambertVS(float3 normal, float3 lightDir)
			{
				fixed diff = max(0.3, dot(normal, lightDir));
				return _LightColor0.rgb * diff;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				o.diff = LightingLambertVS(worldNormal, _WorldSpaceLightPos0.xyz);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
			    col *= fixed4(i.diff, 1) + _Color * (1 - col.a) * 4;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col/* * fixed4(i.diff, 1) + _Color * (1 - col.a) * 2*/;
            }
            ENDCG
        }
    }
}
