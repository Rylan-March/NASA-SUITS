#if UNITY_EDITOR
using UnityEditor;

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Shared validation rules for different OpenXRFeatures.
    /// </summary>
    static class SharedValidationRules
    {
        internal static OpenXRFeature.ValidationRule[] EnableARSessionValidationRules(OpenXRFeature feature) => new OpenXRFeature.ValidationRule[]
        {
            // Check is not redundant with the UnityEditor.XR.OpenXR.Features.Android.AndroidXRProjectValidationRules AR Feature check
            // in the edge case that this feature is used without Android OpenXR
            new OpenXRFeature.ValidationRule(feature)
            {
                message = "AdroidXR ARSession feature must be enabled for this AR feature.",
                checkPredicate = () =>
                {
                    OpenXRSettings androidOpenXRSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
                    var arsessionFeature = androidOpenXRSettings.GetFeature<ARSessionFeature>();
                    return (arsessionFeature != null && arsessionFeature.enabled) ? true : false;
                },
                fixItAutomatic = true,
                fixItMessage = "Open Project Settings > XR Plug-in Management > OpenXR > Android tab. In the list of 'OpenXR Feature Groups', " +
                                "make sure 'Android XR: AR Session' is checked.",
                fixIt = () =>
                {
                    OpenXRSettings androidOpenXRSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
                    var arsessionFeature = androidOpenXRSettings.GetFeature<ARSessionFeature>();
                    if (arsessionFeature != null)
                    {
                        arsessionFeature.enabled = true;
                    }
                },
                error = false
            }
        };
    }
}
#endif
