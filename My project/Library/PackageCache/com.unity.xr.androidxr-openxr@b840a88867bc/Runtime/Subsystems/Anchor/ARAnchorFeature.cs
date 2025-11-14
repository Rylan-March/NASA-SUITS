using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Enables AR Foundation anchor support via OpenXR for Android XR devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Android XR: AR Anchor",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = Constants.k_CompanyName,
        Desc = "AR Foundation anchor support on Android XR devices",
        DocumentationLink = Constants.DocsUrls.k_AnchorUrl,
        OpenxrExtensionStrings = k_OpenXRRequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif

    public class ARAnchorFeature : AndroidXROpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature an id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.arfoundation-androidxr-anchor";

        /// <summary>
        /// The set of OpenXR spec extension strings to enable, separated by spaces.
        /// </summary>
        const string k_OpenXRRequestedExtensions =
            Constants.OpenXRExtensions.k_XR_ANDROID_trackables+ " " +
            Constants.OpenXRExtensions.k_XR_ANDROID_device_anchor_persistence;

        static readonly List<XRAnchorSubsystemDescriptor> s_AnchorDescriptors = new();

        /// <summary>
        /// Instantiates Android OpenXR Anchor subsystem instance, but does not start it.
        /// (Start/Stop is typically handled by AR Foundation managers.)
        /// </summary>
        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRAnchorSubsystemDescriptor, XRAnchorSubsystem>(
                s_AnchorDescriptors,
                AndroidOpenXRAnchorSubsystem.k_SubsystemId);
        }

        /// <summary>
        /// Destroys the anchor subsystem.
        /// </summary>
        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRAnchorSubsystem>();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validation Rules for ARAnchorFeature.
        /// </summary>
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            rules.AddRange(SharedValidationRules.EnableARSessionValidationRules(this));
        }
#endif
    }
}
