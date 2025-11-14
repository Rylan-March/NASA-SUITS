#ifndef XR_SDK_COMPOSITION_LAYERS_CORE_INC
#define XR_SDK_COMPOSITION_LAYERS_CORE_INC

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

#if COMPOSITION_LAYERS_UNIVERSAL || COMPOSITION_LAYERS_HDRENDER
#include "ColorGamut.hlsl"
#else
#include "ColorGamut.cginc"
#endif

//----------------------------------------------------------------------------------------------------------------------------------

#ifndef COMPOSITION_LAYERS_DECLARE_PROPERTIES_USER_DEFINED
#define COMPOSITION_LAYERS_DECLARE_PROPERTIES_USER_DEFINED // Nothing.
#endif

//----------------------------------------------------------------------------------------------------------------------------------

#if COMPOSITION_LAYERS_UNIVERSAL || COMPOSITION_LAYERS_HDRENDER
#define COMPOSITION_LAYERS_PI         PI
#define COMPOSITION_LAYERS_TWO_PI     TWO_PI
#define COMPOSITION_LAYERS_FOUR_PI    FOUR_PI
#else
#define COMPOSITION_LAYERS_PI         UNITY_PI
#define COMPOSITION_LAYERS_TWO_PI     UNITY_TWO_PI
#define COMPOSITION_LAYERS_FOUR_PI    UNITY_FOUR_PI
#endif

//----------------------------------------------------------------------------------------------------------------------------------

#if COMPOSITION_LAYERS_UNIVERSAL || COMPOSITION_LAYERS_HDRENDER
#define COMPOSITION_LAYERS_DECLARE_TEX2D(TEXTURE_)                  TEXTURE2D(TEXTURE_); SAMPLER(sampler##TEXTURE_);
#define COMPOSITION_LAYERS_DECLARE_TEXCUBE(TEXTURE_)                TEXTURECUBE(TEXTURE_); SAMPLER(sampler##TEXTURE_);
#define COMPOSITION_LAYERS_SAMPLE_TEX2D(TEXTURE_, COORDS2_)         SAMPLE_TEXTURE2D(TEXTURE_, sampler##TEXTURE_, COORDS2_)
#define COMPOSITION_LAYERS_SAMPLE_TEX2D_LOD(TEXTURE_, COORDS4_)     SAMPLE_TEXTURE2D_LOD(TEXTURE_, sampler##TEXTURE_, COORDS4_.xy, COORDS4_.w)
#define COMPOSITION_LAYERS_SAMPLE_TEXCUBE_LOD(TEXTURE_, COORDS4_)   SAMPLE_TEXTURECUBE_LOD(TEXTURE_, sampler##TEXTURE_, COORDS4_.xyz, COORDS4_.w)
#else
#define COMPOSITION_LAYERS_DECLARE_TEX2D(TEXTURE_)                  UNITY_DECLARE_TEX2D(TEXTURE_)
#define COMPOSITION_LAYERS_DECLARE_TEXCUBE(TEXTURE_)                UNITY_DECLARE_TEXCUBE(TEXTURE_)
#define COMPOSITION_LAYERS_SAMPLE_TEX2D(TEXTURE_, COORDS2_)         UNITY_SAMPLE_TEX2D(TEXTURE_, COORDS2_)
#define COMPOSITION_LAYERS_SAMPLE_TEX2D_LOD(TEXTURE_, COORDS4_)     UNITY_SAMPLE_TEX2D_LOD(TEXTURE_, COORDS4_.xyz, COORDS4_.w) // Note: OORDS4_.z is unused.
#define COMPOSITION_LAYERS_SAMPLE_TEXCUBE_LOD(TEXTURE_, COORDS4_)   UNITY_SAMPLE_TEXCUBE_LOD(TEXTURE_, COORDS4_.xyz, COORDS4_.w)
#endif

//----------------------------------------------------------------------------------------------------------------------------------

#if CUSTOM_RECTS_ON
#define COMPOSITION_LAYERS_DECLARE_PROPERTIES_CUSTOM_RECTS \
    UNITY_DEFINE_INSTANCED_PROP(float4, _SourceRect) \
    UNITY_DEFINE_INSTANCED_PROP(float4, _DestRect)
#else
#define COMPOSITION_LAYERS_DECLARE_PROPERTIES_CUSTOM_RECTS
#endif

#if COLOR_SCALE_BIAS_ON
#define COMPOSITION_LAYERS_DECLARE_PROPERTIES_COLOR_SCALE_BIAS \
    UNITY_DEFINE_INSTANCED_PROP(float4, _ColorScale) \
    UNITY_DEFINE_INSTANCED_PROP(float4, _ColorBias)
#else
#define COMPOSITION_LAYERS_DECLARE_PROPERTIES_COLOR_SCALE_BIAS
#endif

#if COMPOSITION_LAYERTYPE_EQUIRECT
#define COMPOSITION_LAYERS_DECLARE_PROPERTIES_EQUIRECT \
    UNITY_DEFINE_INSTANCED_PROP(float, _centralHorizontalAngle) \
    UNITY_DEFINE_INSTANCED_PROP(float, _upperVerticalAngle) \
    UNITY_DEFINE_INSTANCED_PROP(float, _lowerVerticalAngle)
#else
#define COMPOSITION_LAYERS_DECLARE_PROPERTIES_EQUIRECT
#endif

#define COMPOSITION_LAYERS_DECLARE_PROPERTIES_ALPHA_CUTOFF \
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)

#define COMPOSITION_LAYERS_DECLARE_PROPERTIES_TRANSFORM_MATRIX \
    UNITY_DEFINE_INSTANCED_PROP(int, _TransformMatrixType) \
    UNITY_DEFINE_INSTANCED_PROP(float4x4, _TransformMatrix)

#if COMPOSITION_LAYERS_HDRENDER
#define COMPOSITION_LAYERS_DECLARE_PROPERTIES_SHADER_VARIABLES_GLOBAL \
    UNITY_DEFINE_INSTANCED_PROP(float4x4, _CompositionLayers_ViewMatrix) \
    UNITY_DEFINE_INSTANCED_PROP(float4x4, _CompositionLayers_ProjectionMatrix) \
    UNITY_DEFINE_INSTANCED_PROP(float4, _CompositionLayers_ProjectionParams)
#else
#define COMPOSITION_LAYERS_DECLARE_PROPERTIES_SHADER_VARIABLES_GLOBAL
#endif

#define COMPOSITION_LAYERS_DECLARE_PROPERTIES_STEREO_EYE_IDNEX \
    UNITY_DEFINE_INSTANCED_PROP(int, _CompositionLayers_StereoEyeIndex)

#define COMPOSITION_LAYERS_DECLARE_PROPERTIES_HDR_DISPLAY \
    UNITY_DEFINE_INSTANCED_PROP(float, _SourceNitsForPaperWhite) \
    UNITY_DEFINE_INSTANCED_PROP(int, _SourceColorGamut) \
    UNITY_DEFINE_INSTANCED_PROP(float, _SourceMaxDisplayNits) \
    UNITY_DEFINE_INSTANCED_PROP(float, _NitsForPaperWhite) \
    UNITY_DEFINE_INSTANCED_PROP(int, _ColorGamut) \
    UNITY_DEFINE_INSTANCED_PROP(float, _MaxDisplayNits)

#define COMPOSITION_LAYERS_DECLARE_PROPERTIES \
    UNITY_INSTANCING_BUFFER_START(UnityPerMaterial) \
        COMPOSITION_LAYERS_DECLARE_PROPERTIES_USER_DEFINED \
        COMPOSITION_LAYERS_DECLARE_PROPERTIES_CUSTOM_RECTS \
        COMPOSITION_LAYERS_DECLARE_PROPERTIES_COLOR_SCALE_BIAS \
        COMPOSITION_LAYERS_DECLARE_PROPERTIES_EQUIRECT \
        COMPOSITION_LAYERS_DECLARE_PROPERTIES_ALPHA_CUTOFF \
        COMPOSITION_LAYERS_DECLARE_PROPERTIES_TRANSFORM_MATRIX \
        COMPOSITION_LAYERS_DECLARE_PROPERTIES_SHADER_VARIABLES_GLOBAL \
        COMPOSITION_LAYERS_DECLARE_PROPERTIES_STEREO_EYE_IDNEX \
        COMPOSITION_LAYERS_DECLARE_PROPERTIES_HDR_DISPLAY \
    UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

#define COMPOSITION_LAYERS_TRANSFORM_MATRIX_TYPE_MODEL (0)
#define COMPOSITION_LAYERS_TRANSFORM_MATRIX_TYPE_MODEL_VIEW (1)

//----------------------------------------------------------------------------------------------------------------------------------
//Original Equirect Shader code.
//The swapchain image is mapped to the full sphere,such that the partial sphere shows only a window into the equirectangular subimage.
inline float4 EquirectCoords(float3 pose)
{
    const float seam_width = 0.01;

    float3 normalized = normalize(pose);
    float theta = acos(-normalized.y);
    float phi = atan2(pose.x, pose.z);

    float seam = max(0, 1 - abs(normalized.x) / seam_width) * clamp(1 + (normalized.z) / seam_width, 0, 1);
    float mip = -2.0 * log2(1.0 + -normalized.y * -normalized.y) - COMPOSITION_LAYERS_FOUR_PI * seam;

    return float4(0.5 + phi / COMPOSITION_LAYERS_TWO_PI, theta / COMPOSITION_LAYERS_PI, 0.0, mip);
}

//Remapped Equirect Shader Code
//The corners of the swapchain subimage are remapped so the equirectangular subimage fills entire partial sphere.
inline float4 RemappedEquirectCoords(float3 pose, float centralHorizontalAngle, float upperVerticalAngle, float lowerVerticalAngle)
{
    float3 p = pose;
    p = normalize(p);
    float2 uv = float2(atan2(p.x, p.z), acos(-p.y));

    centralHorizontalAngle *= COMPOSITION_LAYERS_FOUR_PI;
    uv.x = (uv.x + (centralHorizontalAngle * 0.5)) / centralHorizontalAngle;

    lowerVerticalAngle += 0.25;
    upperVerticalAngle += 0.25;
    uv.y /= COMPOSITION_LAYERS_PI;
    uv.y = (uv.y + (lowerVerticalAngle - 0.5)) / (upperVerticalAngle + lowerVerticalAngle);

    return float4(uv.x, uv.y, 0.0, 0.0);
}

// Get coords with custom rects for Layer.
inline float4 GetCoordsWithCustomRects(float2 coords, float4 sourceRects, float4 destRects)
{
    float2 destWh = destRects.zw;
    float2 destUv = (coords - destRects.xy) / destWh;
    float2 srcStart = sourceRects.xy;
    float2 srcDim = sourceRects.zw;
    return float4(srcStart + destUv * srcDim, destUv);
}

#if CUSTOM_RECTS_ON
#define GET_COORDS_WITH_CUSTOM_RECTS(COORDS_) GetCoordsWithCustomRects(COORDS_, \
    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _SourceRect), \
    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _DestRect))
#else // CUSTOM_RECTS_ON
#define GET_COORDS_WITH_CUSTOM_RECTS(COORDS_) (COORDS_)
#endif // CUSTOM_RECTS_ON

//----------------------------------------------------------------------------------------------------------------------------------

inline float4 LinearToGamma(float4 color)
{
    return float4(sqrt(color.xyz), color.a);
}

inline float4 GammaToLinear(float4 color)
{
    return float4(color.xyz * color.xyz, color.a);
}

//----------------------------------------------------------------------------------------------------------------------------------

// Generate color gradient for Layer.
inline float4 ColorGradient(float2 coords)
{
    const float2 st = coords * 10.0;
    const float2 uv = float2(1.0, 1.0) - fmod(st, 2.0);
    const float b = step(uv.x * uv.y, 0.0);
    float4 color = (1.0 - b) * float4(0.9, 0.9, 0.9, 1) + b * float4(0.1, 0.1, 0.1, 1.0);
    #if !UNITY_COLORSPACE_GAMMA
    color = GammaToLinear(color);
    #endif
    return color;
}

// Generate color gradient for Equirect.
inline float4 ColorGradientEquirect(float4 coords)
{
    return ColorGradient(coords.xy);
}

// Generate color gradient for CubeMap.
inline float4 ColorGradientCubemap(float4 coords)
{
    const float2 st = 5000.0 - coords.xy * 0.005;
    const float2 uv = float2(1.0, 1.0) - fmod(st, 2.0);
    const float b = step(uv.x * uv.y, 0.0);
    float4 color = (1.0 - b) * float4(0.9, 0.9, 0.9, 1.0) + b * float4(0.1, 0.1, 0.1, 1.0);
    #if !UNITY_COLORSPACE_GAMMA
    color = GammaToLinear(color);
    #endif
    return color;
}

//----------------------------------------------------------------------------------------------------------------------------------

// Check coords range for Layer.
inline float4 CheckCoordsRange(float4 color, float2 coords)
{
    if (coords.x > 1.0 || coords.y > 1.0 || coords.x < 0.0 || coords.y < 0.0)
        return float4(0.0, 0.0, 0.0, 0.0);
    else
        return color;
}

#define CHECK_COORDS_RANGE(COLOR_, COORDS_) COLOR_ = CheckCoordsRange(COLOR_, COORDS_)

// Check coords range for Equirect.
inline float4 CheckCoordsRangeEquirect(float4 color, float4 coords, float centralHorizontalAngle, float lowerVerticalAngle, float upperVerticalAngle)
{
    if (coords.x < 0.5 - centralHorizontalAngle || coords.x > 0.5 + centralHorizontalAngle
        || coords.y < 0.25 - lowerVerticalAngle || coords.y > 0.75 + upperVerticalAngle)
        return float4(0.0, 0.0, 0.0, 0.0);
    else
        return color;
}

#define CHECK_COORDS_RANGE_EQUIRECT(COLOR_, COORDS_) COLOR_ = CheckCoordsRangeEquirect(COLOR_, COORDS_, \
    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _centralHorizontalAngle), \
    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _lowerVerticalAngle), \
    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _upperVerticalAngle))

//----------------------------------------------------------------------------------------------------------------------------------

// Apply color scale & bias.
inline float4 ApplyColorScaleBias(float4 color, float4 scale, float4 bias)
{
    if (color.a > 0.0)
    {
        color.rgb = color.rgb / color.a;
    }
    else
    {
        color.rgb = 0.0;
    }

    color = color * scale + bias;
    color.rgb *= color.a;
    return color;
}

#if COLOR_SCALE_BIAS_ON
#define APPLY_COLOR_SCALE_BIAS(COLOR_) COLOR_ = ApplyColorScaleBias(COLOR_, \
    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _ColorScale), \
    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _ColorBias))
#endif

//----------------------------------------------------------------------------------------------------------------------------------

#define COMPOSITION_LAYERS_FARTHEST_DEPTH_BIAS (0.0001)

#ifdef UNITY_REVERSED_Z
#define COMPOSITION_LAYERS_NEARET_DEPTH     1.0
#define COMPOSITION_LAYERS_FARTHEST_DEPTH   (0.0 + COMPOSITION_LAYERS_FARTHEST_DEPTH_BIAS)
#else
#define COMPOSITION_LAYERS_NEARET_DEPTH     0.0
#define COMPOSITION_LAYERS_FARTHEST_DEPTH   (1.0 - COMPOSITION_LAYERS_FARTHEST_DEPTH_BIAS)
#endif

inline float4 PostfixTransformObjectPos(float4 pos)
{
#if DEPTH_NEAREST
    pos.z = COMPOSITION_LAYERS_NEARET_DEPTH;
#elif DEPTH_FARTHEST
    // Note: Prevent overwritten by skybox on underlay. (URP)
    pos.z = pos.w * COMPOSITION_LAYERS_FARTHEST_DEPTH;
#endif
    return pos;
}

inline float4x4 GetCameraTranslationMatrix()
{
    return float4x4(
        float4(1, 0, 0, _WorldSpaceCameraPos.x),
        float4(0, 1, 0, _WorldSpaceCameraPos.y),
        float4(0, 0, 1, _WorldSpaceCameraPos.z),
        float4(0, 0, 0, 1)
        );
}

inline float4 TransformCubePos(float4 pos)
{
    // Merge camera position transform matrix and cube layer's rotation matrix.
    float4x4 model_matrix = mul(GetCameraTranslationMatrix(), UNITY_MATRIX_M);
    pos = mul(UNITY_MATRIX_VP, mul(model_matrix, pos));
#if DEPTH_NEAREST
    pos.z = COMPOSITION_LAYERS_NEARET_DEPTH;
#elif DEPTH_FARTHEST
    // Note: Prevent overwritten by skybox on underlay. (URP)
    pos.z = pos.w * COMPOSITION_LAYERS_FARTHEST_DEPTH;
#endif
    return pos;
}

inline float4 TransformWorldObjectPos(float4 pos)
{
#if COMPOSITION_LAYERS_UNIVERSAL || COMPOSITION_LAYERS_HDRENDER
    pos = TransformObjectToHClip(pos.xyz);
#else
    pos = UnityObjectToClipPos(pos);
#endif
    return PostfixTransformObjectPos(pos);
}

inline float4 TransformResolveCameraForwardZ(float4 pos, in float4x4 viewMatrix)
{
    // Note: viewMatrix.GetColumn(2) is reversed on BiRP/URP.
    // Note: viewMatrix.GetColumn(2) is reversed when CameraRelativeRendering is enabled on HDRP.
    pos.z *= sign(dot(cross(viewMatrix._m00_m10_m20, viewMatrix._m01_m11_m21), viewMatrix._m02_m12_m22));
    return pos;
}

inline float4 TransformModelViewObjectPos(float4 pos, float4x4 modelViewMatrix)
{
#if COMPOSITION_LAYERS_UNIVERSAL || COMPOSITION_LAYERS_HDRENDER
    float4x4 worldToViewMatrix = GetWorldToViewMatrix(), viewToHClipMatrix = GetViewToHClipMatrix();
#else // BiRP
    float4x4 worldToViewMatrix = UNITY_MATRIX_V, viewToHClipMatrix = UNITY_MATRIX_P;
#endif
    // Note: Can't use UNITY_MATRIX_M / UNITY_MATRIX_V immediately when CameraRelativeRendering(SHADEROPTIONS_CAMERA_RELATIVE_RENDERING) is enabled on HDRP. See also, "Packages/High Definition RP Config/Runtime/ShaderConfig.cs" in the Project Window.
    pos = mul(modelViewMatrix, pos);
    pos = TransformResolveCameraForwardZ(pos, worldToViewMatrix);
    pos = mul(viewToHClipMatrix, pos);
    return PostfixTransformObjectPos(pos);
}

inline float4 TransformObjectPos(float4 pos, int transformMatrixType, float4x4 transformMatrix)
{
    if (transformMatrixType == COMPOSITION_LAYERS_TRANSFORM_MATRIX_TYPE_MODEL)
    {
        return TransformWorldObjectPos(pos);
    }
    else // transformMatrixType == COMPOSITION_LAYERS_TRANSFORM_MATRIX_TYPE_MODEL_VIEW
    {
        return TransformModelViewObjectPos(pos, transformMatrix);
    }
}

inline float4 TransformObjectPos(float4 pos, int transformMatrixType, float4x4 transformMatrix, float4x4 viewMatrix, float4x4 projectionMatrix)
{
    if (transformMatrixType == COMPOSITION_LAYERS_TRANSFORM_MATRIX_TYPE_MODEL)
    {
        pos = mul(transformMatrix, pos);
        pos = mul(viewMatrix, pos);
        pos = mul(projectionMatrix, pos);
    }
    else // transformMatrixType == COMPOSITION_LAYERS_TRANSFORM_MATRIX_TYPE_MODEL_VIEW
    {
        pos = mul(transformMatrix, pos);
        pos = TransformResolveCameraForwardZ(pos, viewMatrix);
        pos = mul(projectionMatrix, pos);
    }

    return PostfixTransformObjectPos(pos);
}

#if COMPOSITION_LAYERS_OVERRIDE_SHADER_VARIABLES_GLOBAL
#define TRANSFORM_OBJECT_POS(POS_) TransformObjectPos(POS_, \
    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _TransformMatrixType), \
    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _TransformMatrix), \
    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CompositionLayers_ViewMatrix), \
    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CompositionLayers_ProjectionMatrix))
#else
#define TRANSFORM_OBJECT_POS(POS_) TransformObjectPos(POS_, \
    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _TransformMatrixType), \
    UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _TransformMatrix))
#endif

inline float4 TransformProjectionPos(float4 pos, float4 projectionParams)
{
    pos.y *= projectionParams.x;
#if DEPTH_NEAREST
    pos.z = COMPOSITION_LAYERS_NEARET_DEPTH;
#elif DEPTH_FARTHEST
    pos.z = COMPOSITION_LAYERS_FARTHEST_DEPTH;
#endif
    return float4(pos.xyz, 1.0);
}

#if COMPOSITION_LAYERS_OVERRIDE_SHADER_VARIABLES_GLOBAL
#define TRANSFORM_PROJECTION_POS(POS_) TransformProjectionPos(POS_, UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CompositionLayers_ProjectionParams))
#else
#define TRANSFORM_PROJECTION_POS(POS_) TransformProjectionPos(POS_, _ProjectionParams)
#endif

inline int GetStereoEyeIndex(int stereoEyeIndex)
{
    return stereoEyeIndex | unity_StereoEyeIndex;
}

#define GET_STEREOEYEINDEX() GetStereoEyeIndex(UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _CompositionLayers_StereoEyeIndex))

#endif // XR_SDK_COMPOSITION_LAYERS_CORE_INC
