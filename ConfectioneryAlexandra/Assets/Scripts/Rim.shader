Shader "Atelier/Selection" {
 SubShader { Tags { "Queue"="Geometry+5" } Pass { Cull Front ZWrite Off
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 vert(appdata_base v):SV_POSITION {
 float3 world=mul(unity_ObjectToWorld,v.vertex).xyz+UnityObjectToWorldNormal(v.normal)*0.009;
 return mul(UNITY_MATRIX_VP,float4(world,1)); }
 fixed4 frag():SV_Target {return fixed4(1,.84,.39,1);}
 ENDCG
 } } }
