Shader "Custom/Unlit/Opaque/PositionTexTintColor" {
Properties {
	_MainTex ("Texture", 2D) = "white" {}
	_TintColor ("Tint", Color) = (1,1,1,1)
}

SubShader {
	Tags{ "Queue" = "Geometry" }
	
	ZWrite On
	Blend One Zero
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				float4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _TintColor;
			
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				UNITY_INITIALIZE_OUTPUT(v2f,OUT);

				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
				OUT.color.xyz = IN.color.xyz * _TintColor.xyz;
				return OUT;
			}
			
			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 color = tex2D(_MainTex, IN.texcoord) * IN.color;
				return color;
			}
		ENDCG
	}
}

}
