Shader "Sprite/Pixel/Outline"
{
    Properties
    {
		// 在面板上可以不显示
		[PerRendererData] _OutlineColor ("OutlineColor", Color) = (1,1,1,1) // 描边颜色
		[PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
		[PerRendererData] _Outline("Outline", Float) = 0 // 大于0表示描边
		_Color("Color", Color) = (1,1,1,1)
    }
    SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}
 
		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
 
		Pass {
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
 
#include "UnityCG.cginc"
 
			struct appdata {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
 
			struct v2f {
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
 
			sampler2D _MainTex;
			half4 _MainTex_TexelSize;
			float _Outline;
			float4 _OutlineColor;
			float4 _Color;
 
			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				o.color = v.color * _Color;
 
				return o;
			}
 
			fixed4 frag(v2f i) : SV_Target{
				// 上下左右四个像素的颜色
				float4 upColor = tex2D(_MainTex, i.texcoord + _MainTex_TexelSize.xy * half2(0, 1));
				float4 downColor = tex2D(_MainTex, i.texcoord + _MainTex_TexelSize.xy * half2(0, -1));
				float4 leftColor = tex2D(_MainTex, i.texcoord + _MainTex_TexelSize.xy * half2(-1, 0));
				float4 rightColor = tex2D(_MainTex, i.texcoord + _MainTex_TexelSize.xy * half2(1, 0));
 
				float4 color = tex2D(_MainTex, i.texcoord) * i.color;
 
				//如果自己透明
				if (_Outline > 0 && color.a == 0) {
					//上下左右有不透明的像素
					if (upColor.a !=0 || downColor.a != 0 || leftColor.a != 0 || rightColor.a != 0) {
						//自己变成描边颜色
						color = _OutlineColor;
					}
				}
 
				color.rgb *= color.a;
 
				return color;
			}
			ENDCG
		}
    }
}