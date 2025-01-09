Shader "Pollen/StereoSinglePassUndistortEquirectangular"
{
   Properties
   {
        _LeftTex ("Texture", 2D) = "transparent" {}
        _RightTex ("Texture", 2D) = "transparent" {}
        _Width ("Width", Int) = 1440
        _Height ("Heigth", Int) = 1080
        //_Z ("Z", Float) = 1.0
        //_Alpha ("Alpha", Float) = 0.0
        _Fisheye_X ("Fisheye X", float) = 6.283185307
        _Fisheye_Y ("Fisheye Y", float) = 3.141592654
        _K_left ("K left", Vector) = (527.6540282950582, 526.4786183117326,749.4766407930963, 540.286919109881) //contains fx,fy,cx,cy
        _D_left ("D left", Vector) = (-0.006139676634739223, 0.003509134726759819, -0.00586743413955142, 0.001035256354060527)
        _K_right ("K right", Vector) = (526.8959046800121, 525.9280133112255,784.4829783393062, 551.859471619228) //contains fx,fy,cx,cy
        _D_right ("D right", Vector) = (-0.0012893510436226515, -0.010066566832422004, 0.005090766355451756, -0.0016813780382031582)
        _R_Row1 ("Rotation matrix right Row 1", Vector) = (0.9999931604518688, 0.003353136085725344, -0.0015606177858584164) 
        _R_Row2 ("Rotation matrix right 2", Vector) = (-0.0033505591922626653, 0.9999930241223101, 0.0016508966703280557) 
        _R_Row3 ("Rotation matrix right 3", Vector) = (0.0015661425803787025, -0.0016456564366728029, 0.9999974195028257) 
        _T_right ("Translation y right", float) = -0.0003928775140876522
        _transparency ("Transparency",  Range(0.0, 1.0)) = 0.0
   }

   SubShader
   {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite off
        Cull off
      Pass
      {
         CGPROGRAM

         #pragma vertex vert alpha
         #pragma fragment frag alpha

         sampler2D _LeftTex;
         sampler2D _RightTex;
         const int _Width;
         const int _Height;
         //float _Alpha;
         float _Fisheye_X;
         float _Fisheye_Y;
         float4 _K_left;
         float4 _D_left;
         float4 _K_right;
         float4 _D_right;
         float3 _R_Row1; 
         float3 _R_Row2; 
         float3 _R_Row3;
         float _T_right;
         float _transparency;
         const float EPSILON = 1.175494351e-38;

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

         float2 CalculateUV(float a, float b, float4 K, float4 D, int width, int height)
         {
            const float r_square = pow(a,2) + pow(b,2);
            const float r = sqrt(r_square);
            const float theta_fisheye = atan(r);
            float2 p = (0,0);            
            if (r != 0)
            {                  
               const float theta_fisheye_d = theta_fisheye + D.x * pow(theta_fisheye,3) + D.y * pow(theta_fisheye,5) + D.z * pow(theta_fisheye,7) + D.w * pow(theta_fisheye,9);
               p.x = (theta_fisheye_d / r) * a;
               p.y = (theta_fisheye_d / r) * b;
            }

            float2 uv;
            uv.x = (K.x * (p.x /*+ _Alpha * p.y*/) + K.z) / _Width;
            uv.y = (K.y * p.y + K.w) / _Height;
            return uv;
         }

         float4 GetColorOrBlack(float2 uv, sampler2D tex)
         {
            if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
            {
               return float4(0, 0, 0, _transparency);
            }

            return tex2D(tex, uv);
         }

         fixed4 frag (v2f i) : SV_Target
         {   
            const float half_size_fisheye = _Fisheye_X / 2.0;
            const float half_size_fisheye_y = _Fisheye_Y / 2.0;
            const float _Z = 1.0;

            float2 uv = i.uv;

            const float theta = half_size_fisheye - uv.x * _Fisheye_X;
            const float phi = half_size_fisheye_y - uv.y * _Fisheye_Y;

            const float MAX_FISHEYE_ANGLE = 1.5707;

            if (theta > MAX_FISHEYE_ANGLE || theta < -MAX_FISHEYE_ANGLE)
            {
               return float4(0, 0, 0, _transparency);
            }

            float3 P;
            P.z = /*_Z * */ cos(phi) * cos(theta);
            P.x = -/* _Z**/ cos(phi) * sin(theta);
            P.y = -/*_Z* */ sin(phi);

            //rectify right view
            if(unity_StereoEyeIndex == 1)
            {
               const float3x3 R = float3x3(_R_Row1, _R_Row2, _R_Row3);
               const float3 T = float3(0, _T_right, 0);
               P = mul(R, P) + T;
            }

            if (P.z == 0)
            {
               P.z = EPSILON;
            }
            
            const float a = P.x / P.z;
            const float b = P.y / P.z;

            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            if(unity_StereoEyeIndex == 0)
            {               
               float2 uv = CalculateUV(a, b, _K_left, _D_left, _Width, _Height);

               return GetColorOrBlack(uv, _LeftTex);
            }
            else
            {
               float2 uv = CalculateUV(a, b, _K_right, _D_right, _Width, _Height);

               return GetColorOrBlack(uv, _RightTex);
            }
         }
         ENDCG
      }
   }
}