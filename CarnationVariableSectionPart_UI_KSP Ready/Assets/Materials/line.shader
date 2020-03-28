Shader "CVSP/Line"
{
	Properties
	{
		//_MainTex("MainTex", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Center("Center", Vector) = (0,0,0,0)
		_FadeRange("Fade Range", Range(0.001, 50)) = 1
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
		ZTest Off
		Fog{ Mode Off }
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 fade : COLOR;
			};
	
			fixed4 _Color;
			fixed4 _Center;
			fixed _FadeRange;
	
			v2f vert(appdata_base a)
			{
				v2f r;
				float4 d;
				d.xyz = _Center.xyz - a.vertex.xyz;
				d.x = d.x * d.x + d.y * d.y + d.z * d.z;
				//d.x = d.xyz * d.xyz;
				r.fade.x = clamp(1 - d.x / _FadeRange, 0, 1);
				r.fade.yzw = 0;
				r.vertex = UnityObjectToClipPos(a.vertex);
				return r;
			}
	
			fixed4 frag(v2f v) : SV_Target
			{
				float4 c = _Color;
				c.a *= v.fade.x;
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
}