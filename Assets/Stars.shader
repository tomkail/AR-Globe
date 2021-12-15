// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Overboard!/Stars"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_GlimmerTex ("Glimmer Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha One
        // Blend One OneMinusSrcAlpha
		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				float4 worldSpacePos   : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 worldSpacePos   : TEXCOORD1;
			};
			
			fixed4 _Color;
			uniform fixed _StarsOpacity;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif
                OUT.worldSpacePos = mul(unity_ObjectToWorld, IN.vertex);

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _GlimmerTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
				// fixed mask = tex2D(_GlimmerTex, IN.texcoord).rgb;
                // mask = saturate(pow(mask,1.5) * 20);
                // mask = saturate(mask*5);
                // mask = 0.5;
                // c.a *= ;
				fixed glimmer = tex2D(_GlimmerTex, IN.worldSpacePos * 0.0014 + _Time.y * float2(0.011,0)).rgb;
				glimmer += tex2D(_GlimmerTex, IN.worldSpacePos * 0.001 + _Time.y * float2(-0.016,0)).rgb;
                glimmer += 0.1;
                glimmer *= 1.25;
                glimmer = pow(glimmer, 2) * 25;
                // glimmer = max(mask, glimmer);
                // glimmer = max(0.75, glimmer);
                glimmer = saturate(glimmer);
                c.a *= glimmer;
                c.a *= _StarsOpacity;
                c.rgb *= c.a;
				// c.rgba = glimmer;
				return c;
			}
		ENDCG
		}
	}
}