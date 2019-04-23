Shader "Unlit/Particle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
	    _Color("Main Color", Color) = (1,1,1,1)
		_Offset("Offset", float) = 1
        _SegmentSize("SegmentSize", float) = 30
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend One OneMinusSrcAlpha
        ColorMask RGB
        Cull Off Lighting Off ZWrite Off

        Pass
        {
            CGPROGRAM
			#pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_particles
			#pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				UNITY_FOG_COORDS(1)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed4 _Color;
			float _Offset;
            float _SegmentSize;

            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				float4 pos = mul(UNITY_MATRIX_MV, v.vertex);
				//pos.z += floor(abs(pos.z / _SegmentSize)) * _SegmentSize;
				float s = (1 + sign(pos.z)) * 0.5;
				//pos.z += s * _Offset;
				// TODO 
				pos.z = -abs(pos.z) * (1 - s) + abs(pos.z) * (s) - _SegmentSize * (s);
                o.vertex = mul(UNITY_MATRIX_P, pos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed a = tex2D(_MainTex, i.uv).a;
			    fixed4 col = _Color;
			    UNITY_APPLY_FOG(i.fogCoord, col);
				return col * a;
            }
            ENDCG
        }
    }
}
