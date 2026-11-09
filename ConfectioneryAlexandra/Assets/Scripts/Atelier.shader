Shader "Atelier/Porcelain" {
 SubShader { Tags { "RenderType"="Opaque" }
 Pass { Tags { "LightMode"="ForwardBase" }
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fwdbase
 #include "UnityCG.cginc"
 #include "AutoLight.cginc"
 struct meshInput {float4 vertex:POSITION;float3 normal:NORMAL;float4 color:COLOR;};
 struct interpolator {float4 pos:SV_POSITION;half3 base:COLOR;float3 normal:TEXCOORD0;SHADOW_COORDS(1)};
 interpolator vert(meshInput v){interpolator o;o.pos=UnityObjectToClipPos(v.vertex);o.base=v.color.rgb;o.normal=UnityObjectToWorldNormal(v.normal);TRANSFER_SHADOW(o);return o;}
 fixed4 frag(interpolator i):SV_Target{
 float3 n=normalize(i.normal);
 half sky=.5+.5*n.y;
 half sun=saturate(dot(n,normalize(float3(-.42,.80,-.43))));
 half shadow=SHADOW_ATTENUATION(i);
 half3 light=half3(.66,.67,.69)+half3(.12,.12,.105)*sky+half3(.24,.215,.18)*sun*lerp(.28,1.,shadow);
 return fixed4(i.base*light,1.);
 }
 ENDCG
 }
 UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
 } Fallback "Diffuse"
}
