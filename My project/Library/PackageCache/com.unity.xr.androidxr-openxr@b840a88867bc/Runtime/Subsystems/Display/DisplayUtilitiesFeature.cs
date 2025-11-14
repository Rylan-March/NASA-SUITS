#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Use this OpenXR feature to enable OpenXR extensions associated with the
    /// <see cref="AndroidOpenXRDisplaySubsystemExtensions"/>. Without the necessary OpenXR extensions enabled, the
    /// display subsystem extension methods will always return <see langword="false"/>.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Android XR: Display Utilities",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = Constants.k_CompanyName,
        Desc = "Support for modifying the display refresh rate",
        DocumentationLink = "",
        OpenxrExtensionStrings = Constants.OpenXRExtensions.k_XR_FB_display_refresh_rate,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class DisplayUtilitiesFeature : AndroidXROpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.androidxr-display-utilities";
    }
}
