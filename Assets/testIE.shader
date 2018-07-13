Shader "Unlit/testIE"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color1 ("Color 1",Color) = (1,1,1,1)
		_Color2 ("Color 2",Color) = (1,1,1,1)
		_Color3 ("Color 3",Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color1;
			fixed4 _Color2;
			fixed4 _Color3;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 colM = col;
				float avg = (col.r + col.g + col.b) * 0.3333;
				if(avg < 0.333){
					col =  _Color1;
				}
				else if(avg < 0.666){
					col = _Color2;
				}
				else{
					col = _Color3;
				}
				return col;
			}
			ENDCG
		}
	}
}
