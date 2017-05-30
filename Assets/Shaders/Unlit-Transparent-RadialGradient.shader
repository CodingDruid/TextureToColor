Shader "Custom/Unlit/Transparent/RadialGradient" {
Properties {
	_InnerColor ("Inner color", Color) = (1,1,1,1)
	_OuterColor ("Outer color", Color) = (1,1,1,1)
	_Radius ("Radius", float) = 0
	_CenterOffset ("offset", Vector) = (0,0,0,0)
	}

SubShader {
	Tags{ "Queue" = "Geometry" }
	
	ZWrite On
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
				float4 diffuseColor : COLOR0;
			};

			fixed4 _InnerColor;
			fixed4 _OuterColor;
			float _Radius;
			float4 _CenterOffset;
			
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				UNITY_INITIALIZE_OUTPUT(v2f,OUT);
								
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.diffuseColor = IN.color;
				return OUT;
			}
			
			fixed4 frag(v2f IN) : SV_Target
			{
				float4 objectOrigin = mul(UNITY_MATRIX_MVP, float4(0,0,0,1.0) );
				objectOrigin.xyz += _CenterOffset.xyz;
				float distanceFromCenter = distance(IN.vertex, objectOrigin);

				if (_Radius <= 0)
					distanceFromCenter = 0;
				else
					distanceFromCenter /= _Radius;
				float4 rColor = lerp(_InnerColor, _OuterColor, saturate(distanceFromCenter));
				return rColor;
			}
		ENDCG
	}
}

}
