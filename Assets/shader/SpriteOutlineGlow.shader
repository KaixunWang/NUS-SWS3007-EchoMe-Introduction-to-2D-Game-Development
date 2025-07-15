Shader "Custom/SpriteOutlineGlow"{
Properties {
    _MainTex ("Sprite Texture", 2D) = "white" {}
    _OutlineWidth ("Outline Width", float) = 1
    _OutlineColor ("Outline Color", Color) = (1,1,1,1)
    _Alpha ("Alpha", Range(0,1)) = 1
}
SubShader {
Blend SrcAlpha OneMinusSrcAlpha
Pass {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;
half4 _MainTex_TexelSize;
float _OutlineWidth;
float4 _OutlineColor;
float _Alpha;

struct appdata {
float4 vertex : POSITION;
float2 uv : TEXCOORD0;
};

struct v2f {
float4 vertex : SV_POSITION;
half2 uv : TEXCOORD0;
half2 left : TEXCOORD1;
half2 right : TEXCOORD2;
half2 up : TEXCOORD3;
half2 down : TEXCOORD5;
};

v2f vert(appdata i) {
v2f o;
o.vertex = UnityObjectToClipPos(i.vertex);
o.uv = TRANSFORM_TEX(i.uv, _MainTex);
o.left = o.uv + half2(-1, 0) * _MainTex_TexelSize.xy * _OutlineWidth;
o.right = o.uv + half2(1, 0) * _MainTex_TexelSize.xy * _OutlineWidth;
o.up = o.uv + half2(0, 1) * _MainTex_TexelSize.xy * _OutlineWidth;
o.down = o.uv + half2(0, -1) * _MainTex_TexelSize.xy * _OutlineWidth;
return o;
}

fixed4 frag(v2f i) : SV_Target {
    fixed4 c = tex2D(_MainTex, i.uv);
    float transparent = tex2D(_MainTex, i.left).a * tex2D(_MainTex, i.right).a * tex2D(_MainTex, i.up).a * tex2D(_MainTex, i.down).a;
    c.rgb = lerp(_OutlineColor.rgb, c.rgb, transparent);
    c.a *= _Alpha;
    return c;
}
ENDCG
}
}
}