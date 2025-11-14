#ifndef XR_SDK_COMPOSITION_LAYERS_COLOR_GAMUT_hLSL_INC
#define XR_SDK_COMPOSITION_LAYERS_COLOR_GAMUT_HLSL_INC

// OETF() / InverseOETF() will be supported on HDROutput.hlsl.

// define built-in functions for UnityColorGamut.cginc.

#pragma warning (disable : 3571)

inline bool IsGammaSpace()
{
#ifdef UNITY_COLORSPACE_GAMMA
    return true;
#else
    return false;
#endif
}

inline float GammaToLinearSpaceExact(float value)
{
    if (value <= 0.04045F)
        return value / 12.92F;
    else if (value < 1.0F)
        return pow((value + 0.055F) / 1.055F, 2.4F);
    else
        return pow(value, 2.2F);
}

inline float LinearToGammaSpaceExact(float value)
{
    if (value <= 0.0F)
        return 0.0F;
    else if (value <= 0.0031308F)
        return 12.92F * value;
    else if (value < 1.0F)
        return 1.055F * pow(value, 0.4166667F) - 0.055F;
    else
        return pow(value, 0.45454545F);
}

// Now UnityColorGamut.cginc is reusable on SRP.
#include "ColorGamut.cginc"

#endif
