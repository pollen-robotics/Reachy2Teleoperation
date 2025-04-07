Shader "Pollen/TunnelingSinglePass"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _Radius ("Radius", Float) = 0.3
        _EdgeSmoothness ("Edge Smoothness", Range(0.001, 0.2)) = 0.08
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            UNITY_DECLARE_TEX2DARRAY(_MainTex);
            float _Radius;
            float _EdgeSmoothness;

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

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 uv = i.uv;
                float eyeOffset = (unity_StereoEyeIndex == 0) ? -0.1 : 0.1;
                float2 globalUV = uv + float2(eyeOffset, 0);

                float2 center = float2(0.5, 0.5);
                float dist = distance(globalUV, center);
                float fade = smoothstep(_Radius, _Radius + _EdgeSmoothness, dist);

                fixed4 texColor = UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(uv, unity_StereoEyeIndex));
                fixed4 dark = float4(0, 0, 0, 1);
                return lerp(texColor, dark, fade);
            }
            ENDCG
        }
    }
}
