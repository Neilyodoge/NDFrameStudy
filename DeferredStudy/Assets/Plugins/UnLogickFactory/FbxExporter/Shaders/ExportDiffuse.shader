// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/UnLogickFactory/FbxExporter/ExportDiffuse"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ColorSpace ("ColorSpace ConversionValue", float) = 0.454545
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _CameraGBufferTexture0; 
			half _ColorSpace;

			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4(pow(tex2D(_CameraGBufferTexture0, i.uv).rgb, _ColorSpace), tex2D(_MainTex, i.uv).a);
			}
			ENDCG
		}
	}
}
