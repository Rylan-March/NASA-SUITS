using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Enables AR Foundation raycast support via OpenXR for Android XR devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Android XR: AR Raycast",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = Constants.k_CompanyName,
        Desc = "AR Foundation raycast detection support on Android XR devices",
        DocumentationLink = Constants.DocsUrls.k_RaycastUrl,
        OpenxrExtensionStrings = k_OpenXRRequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif

    public class ARRaycastFeature : AndroidXROpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.arfoundation-androidxr-raycast";

        /// <summary>
        /// The set of OpenXR spec extension strings to enable, separated by spaces. The depth texture
        /// extension is not required by the raycast manager even when it is casting against depth textures.
        /// </summary>
        const string k_OpenXRRequestedExtensions =
            Constants.OpenXRExtensions.k_XR_ANDROID_trackables + " " +
            Constants.OpenXRExtensions.k_XR_ANDROID_raycast;

        static readonly List<XRRaycastSubsystemDescriptor> s_RaycastDescriptors = new();

        /// <summary>
        /// Instantiates Android OpenXR Raycast subsystem instance, but does not start it.
        /// (Start/Stop is typically handled by AR Foundation managers.)
        /// </summary>
        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRRaycastSubsystemDescriptor, XRRaycastSubsystem>(
                s_RaycastDescriptors,
                AndroidOpenXRRaycastSubsystem.k_SubsystemId);
        }

        /// <summary>
        /// Destroys the raycast subsystem.
        /// </summary>
        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRRaycastSubsystem>();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validation Rules for ARRaycastFeature.
        /// </summary>
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            rules.AddRange(SharedValidationRules.EnableARSessionValidationRules(this));
        }
#endif
    }
}
