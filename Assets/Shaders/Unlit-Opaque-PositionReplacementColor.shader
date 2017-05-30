Shader "Custom/Unlit/Opaque/PositionReplacementColor" {
Properties {
	_Color ("Repl. Color (RGB)", Color) = (1,1,1,1)
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
				float4 color : COLOR;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			fixed4 _Color;
			
			v2f vert (appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.color.xyzw = _Color.xyzw;
				
				return OUT;
			}
			
			fixed4 frag (v2f IN) : SV_Target
			{
				return IN.color;
			}
		ENDCG
	}
}

}
