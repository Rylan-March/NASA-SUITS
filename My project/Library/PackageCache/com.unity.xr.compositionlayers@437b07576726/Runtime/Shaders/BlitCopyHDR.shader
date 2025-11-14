Shader "Unlit/XRCompositionLayers/BlitCopyHDR"
{
    Properties
    {
        _MainTex("SourceTexture", any) = "" {}
        _SourceColorGamut("SourceColorGamut", Integer) = 0
        _SourceNitsForPaperWhite("SourceNitsForPaperWhite", Float) = 160.0
        _SourceMaxDisplayNits("SourceMaxDisplayNits", Float) = 160.0
        _ColorGamut("ColorGamut", Integer) = 0
        _NitsForPaperWhite("NitsForPaperWhite", Float) = 160.0
        _MaxDisplayNits("MaxDisplayNits", Float) = 160.0

        [HideInInspector] _MainTex_ArraySlice("_MainTex_ArraySlice", Integer) = 0
        [HideInInspector] _DestRect("_DestRect", Vector) = (1.0, 1.0, 0.0, 0.0)
        [HideInInspector] _SourceRect("_SourceRect", Vector) = (1.0, 1.0, 0.0, 0.0)

        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("__src", Float) = 1.0 // UnityEngine.Rendering.BlendMode.One
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("__dst", Float) = 10.0 // UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _AlphaSrcBlend("__alphaSrc", Float) = 1.0 // UnityEngine.Rendering.BlendMode.One
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _AlphaDstBlend("__alphaDst", Float) = 10.0 // UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha
    }

    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.core": "0.0"
            "com.unity.render-pipelines.high-definition" : "0.0"
        }
        Tags
        {
            "RenderType" = "Transparent"
            "ForceNoShadowCasting" = "True"
            "IgnoreProjector" = "True"
            "RenderPipeline" = "HDRenderPipeline"
            "UniversalMaterialType" = "SimpleLit"
        }
        Lighting Off
        Fog { Mode Off }
        Blend[_SrcBlend][_DstBlend],[_AlphaSrcBlend][_AlphaDstBlend]
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForwardOnly" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fragment _ COMPOSITION_LAYERS_DISABLE_TEXTURE2D_X_ARRAY
            #pragma multi_compile_vertex _ COMPOSITION_LAYERS_OVERRIDE_SHADER_VARIABLES_GLOBAL

            #define COMPOSITION_LAYERS_HDRENDER 1
            #include "Packages/com.unity.xr.compositionlayers/Runtime/Shaders/BlitCopyHDR.hlsl"
            ENDHLSL
        }
    }

    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.core": "0.0"
            "com.unity.render-pipelines.universal" : "0.0"
        }
        Tags
        {
            "RenderType" = "Transparent"
            "ForceNoShadowCasting" = "True"
            "IgnoreProjector" = "True"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "SimpleLit"
        }
        Lighting Off
        Fog { Mode Off }
        Blend[_SrcBlend][_DstBlend],[_AlphaSrcBlend][_AlphaDstBlend]
        ZTest Always
        Cull Off
        ZWrite Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForwardOnly" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fragment _ COMPOSITION_LAYERS_DISABLE_TEXTURE2D_X_ARRAY

            #define COMPOSITION_LAYERS_UNIVERSAL 1
            #include "Packages/com.unity.xr.compositionlayers/Runtime/Shaders/BlitCopyHDR.hlsl"
            ENDHLSL
        }
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay"
            "RenderType" = "Transparent"
            "ForceNoShadowCasting" = "True"
            "IgnoreProjector" = "True"
        }

        LOD 100
        Lighting Off
        Fog { Mode Off }
        Blend[_SrcBlend][_DstBlend],[_AlphaSrcBlend][_AlphaDstBlend]
        ZTest Always
        Cull Off
        ZWrite Off

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fragment _ COMPOSITION_LAYERS_DISABLE_TEXTURE2D_X_ARRAY

            #include "Packages/com.unity.xr.compositionlayers/Runtime/Shaders/BlitCopyHDR.cginc"
            ENDCG
        }
    }

    Fallback Off
}
