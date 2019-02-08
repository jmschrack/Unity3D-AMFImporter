Shader "Custom/HaloStandard"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 1
        _BumpMap("Normal Map",2D)="normal"{}
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _DetailMap("DetailMap",2D)="white"{}
        _DetailMask("DetailMap Mask",2D)="white"{}
        _OverlayMap("DetailMap(A)",2D)="white"{}
        _OverlayBump("DetailMapBump",2D)="normal"{}
        _OverlayDet("DetailDetailMap(A)",2D)="grey"
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.5

        sampler2D _MainTex,_BumpMap,_DetailMap,_DetailMask,_OverlayBump,_OverlayMap,_OverlayDet;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float2 uv_DetailMap;

            float2 uv_OverlayBump;
            float2 uv_OverlayDet;
            float2 uv_OverlayMap;
            float2 uv_DetailMask;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed blend=tex2D(_DetailMask,IN.uv_DetailMask).a;
            // Albedo comes from a texture tinted by color
            half4 c = tex2D (_MainTex, IN.uv_MainTex);// * _Color;
            half4 d =tex2D(_OverlayMap,IN.uv_OverlayMap);
            //fixed4 a=fixed4(c.a*tex2D(_DetailMap,IN.uv_DetailMap).a*2,d.a*tex2D(_OverlayDet,IN.uv_OverlayDet).a*2,0,0);
            c*=tex2D(_DetailMap,IN.uv_DetailMap)*unity_ColorSpaceDouble;//half4(LerpWhiteTo(tex2D(_DetailMap,IN.uv_DetailMap)*unity_ColorSpaceDouble,1).rgb,1);
            d*=tex2D(_OverlayDet,IN.uv_OverlayDet)*unity_ColorSpaceDouble;
            o.Albedo = lerp(c,d,blend);
            o.Normal=lerp(tex2D(_BumpMap,IN.uv_BumpMap),tex2D(_OverlayBump,IN.uv_OverlayBump),blend);
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness=(c.a*lerp(1,d.a,blend))*_Glossiness;
            
            o.Alpha = c.a;
            //*= LerpWhiteTo (detailAlbedo * unity_ColorSpaceDouble.rgb, mask);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
