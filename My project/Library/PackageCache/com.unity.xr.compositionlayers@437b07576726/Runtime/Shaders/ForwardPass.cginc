#ifndef XR_SDK_COMPOSITION_LAYERS_FORWARD_PASS_INC
#define XR_SDK_COMPOSITION_LAYERS_FORWARD_PASS_INC

#include "Packages/com.unity.xr.compositionlayers/Runtime/Shaders/Core.cginc"

//----------------------------------------------------------------------------------------------------------------------------------

#ifndef COMPOSITION_LAYERS_DECLARE_VERTEX_INPUT_USER_DEEFINED
#define COMPOSITION_LAYERS_DECLARE_VERTEX_INPUT_USER_DEEFINED // Nothing
#endif

#ifndef COMPOSITION_LAYERS_DECLARE_VERTEX_OUTPUT_USER_DEEFINED
#define COMPOSITION_LAYERS_DECLARE_VERTEX_OUTPUT_USER_DEEFINED // Nothing
#endif

#ifndef COMPOSITION_LAYERS_DECLARE_ENTRY_POINT_VERTEX
#define COMPOSITION_LAYERS_DECLARE_ENTRY_POINT_VERTEX 1
#endif

#ifndef COMPOSITION_LAYERS_DECLARE_ENTRY_POINT_FRAGMENT
#define COMPOSITION_LAYERS_DECLARE_ENTRY_POINT_FRAGMENT 1
#endif

//----------------------------------------------------------------------------------------------------------------------------------

// Declare vertex input / output structures.

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    COMPOSITION_LAYERS_DECLARE_VERTEX_INPUT_USER_DEEFINED
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float4 vertex : SV_POSITION;
#if COMPOSITION_LAYERTYPE_LAYER || COMPOSITION_LAYERTYPE_PROJECTION
    float4 uv : TEXCOORD0;
#elif COMPOSITION_LAYERTYPE_EQUIRECT
    float3 uv : TEXCOORD0;
#elif COMPOSITION_LAYERTYPE_CUBEMAP
    float4 uv : TEXCOORD0;
#endif
    COMPOSITION_LAYERS_DECLARE_VERTEX_OUTPUT_USER_DEEFINED
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

// Declare variables.

#if COMPOSITION_LAYERTYPE_LAYER || COMPOSITION_LAYERTYPE_PROJECTION || COMPOSITION_LAYERTYPE_EQUIRECT
COMPOSITION_LAYERS_DECLARE_TEX2D(_MainTex);
#elif COMPOSITION_LAYERTYPE_CUBEMAP
COMPOSITION_LAYERS_DECLARE_TEXCUBE(_Cubemap);
#endif

COMPOSITION_LAYERS_DECLARE_PROPERTIES

//----------------------------------------------------------------------------------------------------------------------------------

// Common initializing for vertex.
#define COMPOSITION_LAYERS_INITIALIZE_FORWARD_PASS_VERTEX(o, v) \
    UNITY_SETUP_INSTANCE_ID(v); \
    UNITY_TRANSFER_INSTANCE_ID(v, o); \
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o)

inline void ForwardPassVertex(inout v2f o, in appdata v)
{
    COMPOSITION_LAYERS_INITIALIZE_FORWARD_PASS_VERTEX(o, v);

#if COMPOSITION_LAYERTYPE_PROJECTION
    o.vertex = TRANSFORM_PROJECTION_POS(v.vertex);
#elif COMPOSITION_LAYERTYPE_CUBEMAP
    o.vertex = TransformCubePos(v.vertex);
#else
    o.vertex = TRANSFORM_OBJECT_POS(v.vertex);
#endif

#if COMPOSITION_LAYERTYPE_LAYER || COMPOSITION_LAYERTYPE_PROJECTION // for Layer / Projection
#if CUSTOM_RECTS_ON
    o.uv = GET_COORDS_WITH_CUSTOM_RECTS(v.uv);
#else
    o.uv.xy = v.uv;
    o.uv.zw = v.uv;
#endif
#elif COMPOSITION_LAYERTYPE_EQUIRECT // for Equirect
    o.uv = v.vertex.xyz;
#elif COMPOSITION_LAYERTYPE_CUBEMAP // for Cubemap
    o.uv = float4(v.vertex.xyz, 0.0);
#endif
}

#if COMPOSITION_LAYERS_DECLARE_ENTRY_POINT_VERTEX
v2f vert(appdata v)
{
    v2f o;
    ForwardPassVertex(o, v);

    return o;
}
#endif

// Common initializing for fragment.
#define COMPOSITION_LAYERS_INITIALIZE_FORWARD_PASS_FRAGMENT(i) \
    UNITY_SETUP_INSTANCE_ID(i); \
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i)

//----------------------------------------------------------------------------------------------------------------------------------

inline float4 ForwardPassFragmentColor(in v2f i)
{
    float4 color;

#if COMPOSITION_LAYERTYPE_LAYER || COMPOSITION_LAYERTYPE_PROJECTION // for Layer / Projection
#if SOURCE_TEXTURE_ON
    color = COMPOSITION_LAYERS_SAMPLE_TEX2D(_MainTex, i.uv.xy);
#else
    color = ColorGradient(i.uv.xy);
#endif
#if CUSTOM_RECTS_ON
    CHECK_COORDS_RANGE(color, i.uv.zw);
#endif

#elif COMPOSITION_LAYERTYPE_EQUIRECT // for Equirect
    float4 coords = RemappedEquirectCoords(i.uv, _centralHorizontalAngle, _upperVerticalAngle, _lowerVerticalAngle);
#if SOURCE_TEXTURE_ON
    color = COMPOSITION_LAYERS_SAMPLE_TEX2D_LOD(_MainTex, coords);
#else
    color = ColorGradientEquirect(coords);
#endif
    //Recaluclating coords due to the RemappedEquirectCoords (UV Mapping) function causing
    //unexpected behavior in CHECK_COORDS_RANGE_EQUIRECT (Clipping) function.
    coords = EquirectCoords(i.uv);
    CHECK_COORDS_RANGE_EQUIRECT(color, coords);

#elif COMPOSITION_LAYERTYPE_CUBEMAP // for Cubemap
#if SOURCE_TEXTURE_ON
    color = COMPOSITION_LAYERS_SAMPLE_TEXCUBE_LOD(_Cubemap, i.uv);
#else
    color = ColorGradientCubemap(i.uv);
#endif
#else
    color = (float4)0;
#endif

    return color;
}

//----------------------------------------------------------------------------------------------------------------------------------

inline float4 ForwardPassFragment(in v2f i)
{
    COMPOSITION_LAYERS_INITIALIZE_FORWARD_PASS_FRAGMENT(i);

    float4 color = ForwardPassFragmentColor(i);

#if _ALPHATEST_ON
    clip(color.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
#endif

#if COLOR_SCALE_BIAS_ON
    APPLY_COLOR_SCALE_BIAS(color);
#endif

    APPLY_HDR_TONEMAP(color.rgb, UnityPerMaterial);

    return color;
}

#if COMPOSITION_LAYERS_DECLARE_ENTRY_POINT_FRAGMENT
float4 frag(v2f i) : SV_Target
{
    return ForwardPassFragment(i);
}
#endif

#endif // XR_SDK_COMPOSITION_LAYERS_FORWARD_PASS_INC
