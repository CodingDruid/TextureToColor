Shader "Custom/Unlit/Transparent/PositionReplacementColor" {
Properties {
	_Color ("Repl. Color (RGBA)", Color) = (1,1,1,1)
}

SubShader {
	Tags{ "Queue" = "Transparent" }
	
	ZWrite Off
	Blend SrcAlpha OneMinusSrcAlpha 
	
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
				OUT.color = _Color;
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
