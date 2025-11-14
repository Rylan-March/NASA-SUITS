Shader "Unlit/XRCompositionLayers/Uber"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset] _Cubemap ("Cubemap", Cube) = "grey" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [HideInInspector][ToggleUI] _ZWrite("__zw", Float) = 1.0
        [HideInInspector][Enum(UnityEngine.Rendering.CullMode)] _Cull("__cull", Float) = 2.0 // UnityEngine.Rendering.CullMode.Back

        _SourceNitsForPaperWhite("SourceNitsForPaperWhite", Float) = 160.0
        _SourceColorGamut("SourceColorGamut", Integer) = 0
        _SourceMaxDisplayNits("SourceMaxDisplayNits", Float) = 160.0
        _NitsForPaperWhite("NitsForPaperWhite", Float) = 160.0
        _ColorGamut("ColorGamut", Integer) = 0
        _MaxDisplayNits("MaxDisplayNits", Float) = 160.0
        _TransformMatrixType("TransformMatrixType", Integer) = 0

        // Blending state (based on universal)
        _Surface("__surface", Float) = 1.0 // UnityEditor.Rendering.Universal.ShaderGraph.SurfaceType.Transparent
        _Blend("__mode", Float) = 1.0 // UnityEditor.Rendering.Universal.ShaderGraph.AlphaMode.Premultiply
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
        LOD 100
        Lighting Off
        Fog { Mode Off }
        Blend [_SrcBlend] [_DstBlend], [_AlphaSrcBlend] [_AlphaDstBlend]
        ZTest Always
        ZWrite [_ZWrite]
        Cull[_Cull]

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForwardOnly" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local COMPOSITION_LAYERTYPE_LAYER COMPOSITION_LAYERTYPE_CUBEMAP COMPOSITION_LAYERTYPE_PROJECTION COMPOSITION_LAYERTYPE_EQUIRECT
            #pragma multi_compile_local _ CUSTOM_RECTS_ON
            #pragma multi_compile_local_fragment _ SOURCE_TEXTURE_ON
            #pragma multi_compile_local_fragment _ COLOR_SCALE_BIAS_ON
            #pragma multi_compile_local_fragment _ALPHATEST_ON
            #pragma multi_compile_local_vertex DEPTH_FARTHEST DEPTH_NEAREST
            #pragma multi_compile_vertex _ COMPOSITION_LAYERS_OVERRIDE_SHADER_VARIABLES_GLOBAL

            #define COMPOSITION_LAYERS_HDRENDER 1
            #include "Packages/com.unity.xr.compositionlayers/Runtime/Shaders/ForwardPass.hlsl"
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
        LOD 100
        Lighting Off
        Fog { Mode Off }
        Blend [_SrcBlend] [_DstBlend], [_AlphaSrcBlend] [_AlphaDstBlend]
        ZTest Always
        ZWrite [_ZWrite]
        Cull[_Cull]

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForwardOnly" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local COMPOSITION_LAYERTYPE_LAYER COMPOSITION_LAYERTYPE_CUBEMAP COMPOSITION_LAYERTYPE_PROJECTION COMPOSITION_LAYERTYPE_EQUIRECT
            #pragma multi_compile_local _ CUSTOM_RECTS_ON
            #pragma multi_compile_local_fragment _ SOURCE_TEXTURE_ON
            #pragma multi_compile_local_fragment _ COLOR_SCALE_BIAS_ON
            #pragma multi_compile_local_fragment _ALPHATEST_ON
            #pragma multi_compile_local_vertex DEPTH_FARTHEST DEPTH_NEAREST

            #define COMPOSITION_LAYERS_UNIVERSAL 1
            #include "Packages/com.unity.xr.compositionlayers/Runtime/Shaders/ForwardPass.hlsl"
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
        Blend [_SrcBlend] [_DstBlend], [_AlphaSrcBlend] [_AlphaDstBlend]
        ZTest Always
        ZWrite [_ZWrite]
        Cull[_Cull]

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local COMPOSITION_LAYERTYPE_LAYER COMPOSITION_LAYERTYPE_CUBEMAP COMPOSITION_LAYERTYPE_PROJECTION COMPOSITION_LAYERTYPE_EQUIRECT
            #pragma multi_compile_local _ CUSTOM_RECTS_ON
            #pragma multi_compile_local_fragment _ SOURCE_TEXTURE_ON
            #pragma multi_compile_local_fragment _ COLOR_SCALE_BIAS_ON
            #pragma multi_compile_local_fragment _ALPHATEST_ON
            #pragma multi_compile_local_vertex DEPTH_FARTHEST DEPTH_NEAREST

            #include "Packages/com.unity.xr.compositionlayers/Runtime/Shaders/ForwardPass.cginc"
            ENDCG
        }
    }

    Fallback Off
}
