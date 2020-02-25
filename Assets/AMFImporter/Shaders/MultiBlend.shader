Shader "Adjutant/MultiBlend"
{
    Properties
    {
        _Color ("Base Color", Color) = (0,0,0,0)
        //_MainTex ("Overlay", 2D) = "white" {}
        //_Glossiness("Overall Smoothness",Range(0,1))=1
        _BlendTex("Blend Map",2D)=""{}
        //_BaseMaps("Base maps",2DArray)=""{}
        //_BaseTiles("UV Scale",floatarray) ={}

        [Toggle] _ENABLE_R ("Has R Channel?", Float) = 0
        _MainTex("R Tex",2D)="black"{}
        _BumpMap("R Tex Normal",2D)="bump"{}
        _DetailAlbedoMap("R Tex Detail",2D)="grey"{}
        _RTexGloss ("R Smoothness", Range(0,1)) = 1

        [Toggle] _ENABLE_G ("Has G Channel?", Float) = 0
        _GTex("G Tex",2D)="black"{}
        _GTexBump("G Tex Normal",2D)="bump"{}
        _GTexDet("G Tex Detail",2D)="grey"{}
        _GTexGloss("G Smoothness",Range(0,1))=1

        [Toggle] _ENABLE_B ("Has B Channel?", Float) = 0
        _BTex("B Tex",2D)="black"{}
        _BTexBump("G Tex Normal",2D)="bump"{}
        _BTexDet("B Tex Detail",2D)="grey"{}
        _BTexGloss("B Smoothness",Range(0,1))=1

        [Toggle] _ENABLE_A ("Has A Channel?", Float) = 0
        _ATex("A Tex",2D)="black"{}
        _ATexBump("A Tex Normal",2D)="bump"{}
        _ATexDet("A Tex Detail",2D)="grey"{} 
        _ATexGloss("A Smoothness",Range(0,1))=1


        [Toggle] _ENABLE_HALOFLIP ("Halo3 style normal maps?",Float) =0
        _NormIntensity("Normal Intensity",Float)=1
        //_Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        #pragma multi_compile_local  _ENABLE_R_OFF _ENABLE_R_ON
        #pragma multi_compile_local  _ENABLE_G_OFF _ENABLE_G_ON
        #pragma multi_compile_local  _ENABLE_B_OFF _ENABLE_B_ON
        #pragma multi_compile_local  _ENABLE_A_OFF _ENABLE_A_ON
        #pragma multi_compile_local _ENABLE_HALOFLIP_OFF _ENABLE_HALOFLIP_ON
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.5

        
        sampler2D _BlendTex;

        sampler2D _MainTex;
        
        sampler2D _DetailAlbedoMap;

        sampler2D _GTex;
        
        sampler2D _GTexDet;

        sampler2D _BTex;
        
        sampler2D _BTexDet;

        sampler2D _ATex;
        
        sampler2D _ATexDet;

       
        //UNITY_DECLARE_TEX2DARRAY(_BaseMaps);
        struct Input
        {
            
            float2 uv_BlendTex;

            float2 uv_MainTex;
            float2 uv_DetailAlbedoMap;
            float2 uv_BumpMap;

            float2 uv_GTex;
            float2 uv_GTexDet;
            float2 uv_GTexBump;

            float2 uv_BTex;
            float2 uv_BTexDet;
            float2 uv_BTexBump;

            float2 uv_ATex;
            float2 uv_ATexDet;
            float2 uv_ATexBump;

        };

        half _Glossiness,_RTexGloss,_GTexGloss,_BTexGloss,_ATexGloss;
        half _Metallic;
        fixed4 _Color;
        fixed _OverlayAlpha,_RTexAlpha,_GTexAlpha,_BTexAlpha,_ATexAlpha,_GTexEmissive;
        //uniform fixed4 _Tints[4];
        
        float _Intensity,_BlendWeight;
       // uniform fixed _HasAlpha [4];

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        half4 LerpWhiteATo(half4 b, half t){
            half oneMinusT = 1 - t;
            return half4(oneMinusT, oneMinusT, oneMinusT,oneMinusT) + b * t;
        }
        half4 Overlay(fixed4 base, fixed4 top){
            fixed baseL=Luminance(base);
            fixed bA=base.a;
            fixed tA=top.a;
            //fixed4 ret=lerp(_Intensity*base*top,1-_Intensity*(1-base)*(1-top),step(_BlendWeight,baseL));
            //fixed4 ret=base*top*2;
            
            return base*LerpWhiteATo(top* unity_ColorSpaceDouble.rgba,1);
        }
        


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            //float3 depthUv=float3(IN.uv_MainTex.x,IN.uv_MainTex.y,0);
            fixed4 blend = tex2D(_BlendTex,IN.uv_BlendTex);
            //fixed4 overlay = tex2D(_RTexDet,IN.uv_RTexDet);
            //overlay.a*=_OverlayAlpha;
            //float3 depthUv=float3(IN.uv_MainTex.x,IN.uv_MainTex.y,0);
            float4 col=_Color;
            #ifdef _ENABLE_R_ON
            col=lerp(col,Overlay(tex2D(_MainTex,IN.uv_MainTex),tex2D(_DetailAlbedoMap,IN.uv_DetailAlbedoMap))*half4(1.0, 1.0, 1.0, _RTexGloss),blend.r);
            #endif
            
            #ifdef _ENABLE_G_ON
            col=lerp(col,Overlay(tex2D(_GTex,IN.uv_GTex),tex2D(_GTexDet,IN.uv_GTexDet))*half4(1.0, 1.0, 1.0, _GTexGloss),blend.g);
            #endif

            #ifdef _ENABLE_B_ON
            col=lerp(col,Overlay(tex2D(_BTex,IN.uv_BTex),tex2D(_BTexDet,IN.uv_BTexDet))*half4(1.0, 1.0, 1.0, _BTexGloss),blend.b);
            #endif

            #ifdef _ENABLE_A_ON
            col=lerp(col,Overlay(tex2D(_ATex,IN.uv_ATex),tex2D(_ATexDet,IN.uv_ATexDet))*half4(1.0, 1.0, 1.0, _ATexGloss),blend.a);
            #endif


            /* fixed4 mixedAlbedo=Overlay(tex2D(_MainTex,IN.uv_MainTex),tex2D(_DetailAlbedoMap,IN.uv_DetailAlbedoMap))*blend.r*half4(1.0, 1.0, 1.0, _RTexGloss);
            
            
            //depthUv.z=1;
            mixedAlbedo+=Overlay(tex2D(_GTex,IN.uv_GTex),tex2D(_GTexDet,IN.uv_GTexDet))*blend.g*half4(1.0, 1.0, 1.0, _GTexGloss);
            
            //depthUv.z=2;
            mixedAlbedo+=Overlay(tex2D(_BTex,IN.uv_BTex),tex2D(_BTexDet,IN.uv_BTexDet))*blend.b*half4(1.0, 1.0, 1.0, _BTexGloss);
            
            //depthUv.z=3;
            mixedAlbedo+=Overlay(tex2D(_ATex,IN.uv_ATex),tex2D(_ATexDet,IN.uv_ATexDet))*blend.a*half4(1.0, 1.0, 1.0, _ATexGloss); */
            
            
           /*  fixed baseL= Luminance(c);//c.r * 0.3 + c.g * 0.59 + c.b * 0.11;
            fixed overlayL = overlay.r * 0.3 + overlay.g * 0.59 + overlay.b * 0.11; */
            //step(0.5,baseL);
            o.Albedo=col;
            o.Smoothness = col.a;//*_Glossiness;
            //c=lerp(_Intensity*c*overlay,1-_Intensity*(1-c)*(1-overlay),step(0.5,baseL));
            //c=c+_Intensity*(2*overlay-1);
            //o.Albedo = mixedAlbedo;//*_Color;
            //o.Emission=_GTexEmissive*tex2D(_GTex,IN.uv_GTex)*tex2D(_GTex,IN.uv_GTex).a*blend.g;
            //o.Normal = float3(0, 0, .01);
            // Metallic and smoothness come from slider variables
            //o.Metallic = _Metallic;
            //o.Metallic=mixedAlbedo.a/2;
            
            //o.Alpha = c.a;
        }
        ENDCG
        BlendOp Max
        CGPROGRAM
        
        #pragma surface surf Standard fullforwardshadows
        #pragma multi_compile_local  _ENABLE_R_OFF _ENABLE_R_ON
        #pragma multi_compile_local  _ENABLE_G_OFF _ENABLE_G_ON
        #pragma multi_compile_local  _ENABLE_B_OFF _ENABLE_B_ON
        #pragma multi_compile_local  _ENABLE_A_OFF _ENABLE_A_ON
        #pragma multi_compile_local _ENABLE_HALOFLIP_OFF _ENABLE_HALOFLIP_ON
        #pragma target 3.5

        #include "UnityStandardUtils.cginc"
//One One

        struct Input
        {
            
            float2 uv_BlendTex;

            float2 uv_BumpMap;

            float2 uv_GTexBump;

            float2 uv_BTexBump;

            float2 uv_ATexBump;

        };
        sampler2D _BlendTex,_BumpMap;
        sampler2D _GTexBump;
        sampler2D _BTexBump;
        sampler2D _ATexBump;
        float _NormIntensity;

        float3 UnpackBump(half4 input,half intensity){
            float3 b = UnpackScaleNormal(input,intensity);
            #ifdef _ENABLE_HALOFLIP_ON
                b = float3(b.x * -1, b.y * -1, b.z);
            #endif
            return b;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 blend = tex2D(_BlendTex,IN.uv_BlendTex);

            float3 mixedNormal=float3(0.5,0.5,1);

            #ifdef _ENABLE_R_ON
            mixedNormal=lerp(mixedNormal,UnpackBump(tex2D(_BumpMap,IN.uv_BumpMap),_NormIntensity),blend.r);
            #endif 

            #ifdef _ENABLE_G_ON
            mixedNormal=lerp(mixedNormal,UnpackBump(tex2D(_GTexBump,IN.uv_GTexBump),_NormIntensity),blend.g);
            #endif

            #ifdef _ENABLE_B_ON
            mixedNormal=lerp(mixedNormal,UnpackBump(tex2D(_BTexBump,IN.uv_BTexBump),_NormIntensity),blend.b);
            #endif 

            #ifdef _ENABLE_A_ON
            mixedNormal=lerp(mixedNormal,UnpackBump(tex2D(_ATexBump,IN.uv_ATexBump),_NormIntensity),blend.a);
            #endif

            mixedNormal.z += 1e-5f; // to avoid nan after normalizing
            //o.Smoothness=0;
            o.Normal=normalize(mixedNormal);
        }

        ENDCG
        
    }
    FallBack "Diffuse"
}
