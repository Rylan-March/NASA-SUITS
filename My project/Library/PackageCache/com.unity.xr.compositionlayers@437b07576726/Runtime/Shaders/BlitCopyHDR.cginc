#ifndef XR_SDK_COMPOSITION_LAYERS_BLITCOPYHDR_INC
#define XR_SDK_COMPOSITION_LAYERS_BLITCOPYHDR_INC

#if COMPOSITION_LAYERS_UNIVERSAL || COMPOSITION_LAYERS_HDRENDER
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
#endif

#if COMPOSITION_LAYERS_UNIVERSAL || COMPOSITION_LAYERS_HDRENDER
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Macros.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#else
#include "UnityCG.cginc"
#endif

#if COMPOSITION_LAYERS_UNIVERSAL
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#endif

#if COMPOSITION_LAYERS_HDRENDER
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#endif

// Fix shader compile erorr on HDRP. Redirect to the static variable.
#ifdef unity_StereoEyeIndex
#undef unity_StereoEyeIndex
static uint compositionLayers_StereoEyeIndex;
#define unity_StereoEyeIndex compositionLayers_StereoEyeIndex
#endif

// Fix shader compile erorr on HDRP. Redirect to the static variable.
#if COMPOSITION_LAYERS_HDRENDER
#ifndef _ScreenSize
static float4 compositionLayers_ScreenSize;
#define _ScreenSize compositionLayers_ScreenSize
#endif
#endif

#if COMPOSITION_LAYERS_UNIVERSAL || COMPOSITION_LAYERS_HDRENDER
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
#endif

#if COMPOSITION_LAYERS_UNIVERSAL || COMPOSITION_LAYERS_HDRENDER
#include "ColorGamut.hlsl"
#else
#include "ColorGamut.cginc"
#endif

#if defined(COMPOSITION_LAYERS_DISABLE_TEXTURE2D_X_ARRAY)
#define SRC_TEXTURE2D_X_ARRAY 0
#else
#define SRC_TEXTURE2D_X_ARRAY 1
#endif

#if SRC_TEXTURE2D_X_ARRAY
#if COMPOSITION_LAYERS_UNIVERSAL || COMPOSITION_LAYERS_HDRENDER
TEXTURE2D_ARRAY(_MainTex);
SAMPLER(sampler_MainTex);
#else
UNITY_DECLARE_TEX2DARRAY(_MainTex);
#endif
#else
#if COMPOSITION_LAYERS_UNIVERSAL || COMPOSITION_LAYERS_HDRENDER
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
#else
UNITY_DECLARE_TEX2D(_MainTex);
#endif
#endif

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)

UNITY_DEFINE_INSTANCED_PROP(float4, _DestRect)
UNITY_DEFINE_INSTANCED_PROP(float4, _SourceRect)
UNITY_DEFINE_INSTANCED_PROP(float2, _MainTex_TexelSize)
UNITY_DEFINE_INSTANCED_PROP(uint, _MainTex_ArraySlice)
UNITY_DEFINE_INSTANCED_PROP(float, _SourceNitsForPaperWhite)
UNITY_DEFINE_INSTANCED_PROP(int, _SourceColorGamut)
UNITY_DEFINE_INSTANCED_PROP(int, _CompositionLayers_StereoEyeIndex)
UNITY_DEFINE_INSTANCED_PROP(float4, _CompositionLayers_ScreenSize)
UNITY_DEFINE_INSTANCED_PROP(float, _SourceMaxDisplayNits)
UNITY_DEFINE_INSTANCED_PROP(float, _NitsForPaperWhite)
UNITY_DEFINE_INSTANCED_PROP(int, _ColorGamut)
UNITY_DEFINE_INSTANCED_PROP(float, _MaxDisplayNits)

UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

#if UNITY_PRETRANSFORM_TO_DISPLAY_ORIENTATION
float4 XR_ApplyPretransformRotation(float4 v)
{
    switch (UNITY_DISPLAY_ORIENTATION_PRETRANSFORM)
    {
    default:
    case UNITY_DISPLAY_ORIENTATION_PRETRANSFORM_0: break;
    case UNITY_DISPLAY_ORIENTATION_PRETRANSFORM_90: v.xy = float2(v.y, -v.x); break;
    case UNITY_DISPLAY_ORIENTATION_PRETRANSFORM_180: v.xy = -v.xy; break;
    case UNITY_DISPLAY_ORIENTATION_PRETRANSFORM_270: v.xy = float2(-v.y, v.x); break;
    }
    return v;
}
#endif

float4 XR_GetQuadVertexPosition(uint vertexID, float z = UNITY_NEAR_CLIP_VALUE)
{
    // 0:Left-Down 1:Left-Up 2: Right-Up 3:Right-Down
    uint topBit = vertexID >> 1;
    uint botBit = (vertexID & 1);
    float x = topBit; // 0, 0, 1, 1
    float y = 1 - (topBit + botBit) & 1; // 1, 0, 0, 1
    return float4(x, y, z, 1.0);
}

float2 XR_GetQuadTexCoord(uint vertexID)
{
    // 0:Left-Down 1:Left-Up 2: Right-Up 3:Right-Down
    uint topBit = vertexID >> 1;
    uint botBit = (vertexID & 1);
    float u = topBit; // 0, 0, 1, 1
    float v = (topBit + botBit) & 1; // 0, 1, 1, 0
    return float2(u, v);
}

struct appdata_t
{
    uint vertexID : SV_VertexID;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float2 texcoord : TEXCOORD0;
};

v2f vert(appdata_t v)
{
    v2f o;
    UNITY_SETUP_INSTANCE_ID(v);

    float4 scaleBiasRt = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _DestRect);
    float4 scaleBias = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _SourceRect);

    o.vertex = XR_GetQuadVertexPosition(v.vertexID) * float4(scaleBiasRt.x, scaleBiasRt.y, 1.0f, 1.0f) + float4(scaleBiasRt.z, scaleBiasRt.w, 0.0f, 0.0f);
    o.vertex.xy = o.vertex.xy * float2(2.0f, -2.0f) + float2(-1.0f, 1.0f);
#ifdef UNITY_PRETRANSFORM_TO_DISPLAY_ORIENTATION
    o.vertex = XR_ApplyPretransformRotation(o.vertex);
#endif

    o.texcoord = XR_GetQuadTexCoord(v.vertexID);
    o.texcoord = o.texcoord * scaleBias.xy + float2(scaleBias.z, scaleBias.w);
    return o;
}

float4 frag(v2f i) : SV_Target
{
    float2 uv = i.texcoord.xy;

#if defined(SUPPORTS_FOVEATED_RENDERING_NON_UNIFORM_RASTER)
#ifndef _FOVEATED_RENDERING_NON_UNIFORM_RASTER
#define _FOVEATED_RENDERING_NON_UNIFORM_RASTER 0
#endif
    UNITY_BRANCH if (_FOVEATED_RENDERING_NON_UNIFORM_RASTER)
    {
        unity_StereoEyeIndex = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CompositionLayers_StereoEyeIndex);
        _ScreenSize = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CompositionLayers_ScreenSize);
        uv = RemapFoveatedRenderingLinearToNonUniform(uv);
    }
#endif // SUPPORTS_FOVEATED_RENDERING_NON_UNIFORM_RASTER

    #if SRC_TEXTURE2D_X_ARRAY
    uint mainTexArraySlice = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ArraySlice);
#if COMPOSITION_LAYERS_UNIVERSAL || COMPOSITION_LAYERS_HDRENDER
    float4 scene = SAMPLE_TEXTURE2D_ARRAY(_MainTex, sampler_MainTex, uv, mainTexArraySlice);
#else
    float4 scene = UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(uv, mainTexArraySlice));
#endif
    #else
#if COMPOSITION_LAYERS_UNIVERSAL || COMPOSITION_LAYERS_HDRENDER
    float4 scene = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
#else
    float4 scene = UNITY_SAMPLE_TEX2D(_MainTex, uv);
#endif
    #endif

    float3 result = scene.rgb;

    APPLY_HDR_TONEMAP(result, UnityPerMaterial)

    return float4(result.rgb, scene.a);
}

#endif
