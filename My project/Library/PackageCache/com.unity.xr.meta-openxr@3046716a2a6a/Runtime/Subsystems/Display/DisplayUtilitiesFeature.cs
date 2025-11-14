#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif
using static UnityEngine.XR.OpenXR.Features.Meta.Constants.OpenXRExtensions;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Use this OpenXR feature to enable OpenXR extensions associated with the
    /// <see cref="MetaOpenXRDisplaySubsystemExtensions"/>. Without the necessary OpenXR extensions enabled, the
    /// display subsystem extension methods will always return <see langword="false"/>.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = displayName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone },
        Company = Constants.k_CompanyName,
        Desc = "Enables you to request a specific display refresh rate",
        DocumentationLink = Constants.DocsUrls.k_DisplayUtilitiesUrl,
        OpenxrExtensionStrings = k_RequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class DisplayUtilitiesFeature : MetaOpenXRFeature
    {
        /// <summary>
        /// UI display name of this feature.
        /// </summary>
        public const string displayName = "Meta Quest: Display Utilities";

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.meta-display-utilities";

        const string k_RequestedExtensions = k_XR_FB_display_refresh_rate;

        /// <summary>
        /// Called after `xrCreateInstance`. Override this method to validate that any necessary OpenXR extensions were
        /// successfully enabled
        /// (<a href="xref:UnityEngine.XR.OpenXR.OpenXRRuntime.IsExtensionEnabled(System.String)">OpenXRRuntime.IsExtensionEnabled</a>)
        /// and that any required system properties are supported. If this method returns <see langword="false"/>,
        /// the feature's [enabled](xref:UnityEngine.XR.OpenXR.Features.OpenXRFeature.enabled) property is set to <see langword="false"/>.
        /// </summary>
        /// <param name="xrInstance">Handle of the native `xrInstance`.</param>
        /// <returns><see langword="true"/> if this feature successfully initialized. Otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// If this feature is a required feature of an enabled feature set, returning <see langword="false"/> here
        /// causes the `OpenXRLoader` to fail, and XR Plug-in Management will fall back to another loader if enabled.
        /// </remarks>
        /// <seealso href="xref:openxr-features#enabling-openxr-spec-extension-strings">Enabling OpenXR spec extension strings</seealso>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            return OpenXRRuntime.IsExtensionEnabled(k_XR_FB_display_refresh_rate);
        }
    }
}
