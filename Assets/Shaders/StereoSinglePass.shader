Shader "Pollen/StereoSinglePass"
{
   Properties
   {
       _MainTex ("Texture", 2D) = "white" {}
       _MainTex_right ("Texture", 2D) = "white" {}
   }

   SubShader
   {
      Tags { "RenderType" = "Opaque" }

      Pass
      {
         CGPROGRAM

         #pragma vertex vert
         #pragma fragment frag

         sampler2D _MainTex;
         sampler2D _MainTex_right;

         #include "UnityCG.cginc"

         struct appdata
         {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;

            UNITY_VERTEX_INPUT_INSTANCE_ID
         };

         struct v2f
         {
            float4 vertex : SV_POSITION;
            float2 uv : TEXCOORD0;

            UNITY_VERTEX_INPUT_INSTANCE_ID 
            UNITY_VERTEX_OUTPUT_STEREO
         };

         v2f vert (appdata v)
         {
            v2f o;

            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_OUTPUT(v2f, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;

            return o;
         }

         fixed4 frag (v2f i) : SV_Target
         {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            if(unity_StereoEyeIndex == 0)
                return tex2D(_MainTex, i.uv);
            else
                return tex2D(_MainTex_right, i.uv);
         }
         ENDCG
      }
   }
}