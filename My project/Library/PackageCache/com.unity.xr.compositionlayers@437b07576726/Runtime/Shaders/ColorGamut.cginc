#ifndef XR_SDK_COMPOSITION_LAYERS_COLOR_GAMUT_INC
#define XR_SDK_COMPOSITION_LAYERS_COLOR_GAMUT_INC

#define HDR_XR_DISPLAY_SUPPORTED 1
#include "UnityColorGamut.cginc"

float3 SimpleHDRDisplayToneMapAndOETF_Custom(float3 result, int colorGamut, bool forceGammaToLinear, float nitsForPaperWhite, float maxDisplayNits)
{
    if (colorGamut < 0) // Passthrough
    {
        result = forceGammaToLinear ? float3(GammaToLinearSpaceExact(result.r), GammaToLinearSpaceExact(result.g), GammaToLinearSpaceExact(result.b)) : result.rgb;
        return result;
    }
    else
    {
        return SimpleHDRDisplayToneMapAndOETF(result, colorGamut, forceGammaToLinear, nitsForPaperWhite, maxDisplayNits);
    }
}

float3 InverseSimpleHDRDisplayToneMapAndOETF_Custom(float3 result, int colorGamut, bool forceGammaToLinear, float nitsForPaperWhite, float maxDisplayNits)
{
    if (colorGamut < 0) // Passthrough
    {
        result = forceGammaToLinear ? float3(LinearToGammaSpaceExact(result.r), LinearToGammaSpaceExact(result.g), LinearToGammaSpaceExact(result.b)) : result.rgb;
        return result;
    }
    else
    {
        return InverseSimpleHDRDisplayToneMapAndOETF(result, colorGamut, forceGammaToLinear, nitsForPaperWhite, maxDisplayNits);
    }
}

// Customized. (Supporting passthrough when colorGamut < 0)
#define APPLY_HDR_TONEMAP(COLOR_, ARRAYNAME_) \
    COLOR_ = InverseSimpleHDRDisplayToneMapAndOETF_Custom(COLOR_, \
        UNITY_ACCESS_INSTANCED_PROP(ARRAYNAME_, _SourceColorGamut), \
        false, \
        UNITY_ACCESS_INSTANCED_PROP(ARRAYNAME_, _SourceNitsForPaperWhite), \
        UNITY_ACCESS_INSTANCED_PROP(ARRAYNAME_, _SourceMaxDisplayNits)); \
    COLOR_ = SimpleHDRDisplayToneMapAndOETF_Custom(COLOR_, \
        UNITY_ACCESS_INSTANCED_PROP(ARRAYNAME_, _ColorGamut), \
        false, \
        UNITY_ACCESS_INSTANCED_PROP(ARRAYNAME_, _NitsForPaperWhite), \
        UNITY_ACCESS_INSTANCED_PROP(ARRAYNAME_, _MaxDisplayNits));

#endif
